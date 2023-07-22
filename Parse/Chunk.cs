using Parse.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class Chunk
    {
        private ByteWalker _fileWalker;
        public int Length { get; set; }

        public int InitialOffset { get; set; }

        public string Name { get; set; }
        public List<Chunk> Chunks { get; set; } = new List<Chunk>();

        public List<DataItem> DataItems { get; set; } = new List<DataItem>();

        public static byte[] Pad1 = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFE };
        public static byte[] Pad2 = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };

        static uint uPad1 = 0xFFFFFFFF;
        static uint uPad2 = 0xFFFFFFFE;

        public byte[] Data { get; set; }
        public string DataStr
        {
            get
            {
                return string.Join(Environment.NewLine,
                    Encoding.ASCII.GetString(Data.Select(c => (c >= 32 && c <= 127) ? c : (byte)63).ToArray())
                    .Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public Chunk(ByteWalker _fw)
        {
            _fileWalker = _fw;
            InitialOffset = _fw.CurrentIndex;
        }

        public static Chunk ReadChunk(ByteWalker fw)
        {
            var initialOffset = fw.CurrentIndex;
            var Name = fw.GetString(4);
            var toReturn = ChunkFactory(Name, fw);
            toReturn.Name = Name;
            toReturn.Length = fw.GetInt();
            toReturn.Data = fw.GetBytes(toReturn.Length);
            toReturn.InitialOffset = initialOffset;


            if (Name == "ROOT")
                return toReturn;

            ByteWalker fwData = new ByteWalker(toReturn.Data);
            int maxSize = fwData.Length;

            for (int i = 0; i < fwData.Length || i < maxSize;)
            {
                var eat4Bytes = fwData.GetInt();
                var sectionNameSize = fwData.GetInt();

                if (((eat4Bytes == (int)uPad1 || eat4Bytes == (int)uPad2) &&
                    (sectionNameSize == (int)uPad1 || sectionNameSize == (int)uPad2))
                    || (uint)sectionNameSize > 0x100000)
                {
                    string ExtraName = "Extra" + (toReturn.DataItems.Where(x => x.Name.StartsWith("Extra")).Count() + 1).ToString();
                    DataItem diExtra = DataItemFactory.Create("Extra", fwData.GetBytes(fwData.Length - i), initialOffset + i, fw._data);
                    toReturn.DataItems.Add(diExtra);
                    break;
                }

                if (sectionNameSize == 0)
                {
                    break;
                }

                var sectionName = fwData.GetString(sectionNameSize);
                var sectionData = fwData.GetBytesUntil(true, Pad1, Pad2);
                DataItem di = DataItemFactory.Create(sectionName, sectionData, initialOffset + i, fw._data);
                if (di is MMidiTrackEventDataItem)
                {
                    maxSize = (di as MMidiTrackEventDataItem).MIDITrackSize;

                }
                toReturn.DataItems.Add(di);
                i += 4 + 4 + sectionNameSize + sectionData.Length + 4;
            }

            return toReturn;
        }

        public override string ToString()
        {
            return $"{this.Name} (0x{Length.ToString("x4")})";
        }

        public static Chunk ChunkFactory(string SectionName, ByteWalker fw)
        {
            switch (SectionName)
            {
                case "ROOT": return new ROOTChunk(fw);
                case "ARCH": return new ARCHChunk(fw);
            }

            throw new Exception($"Unknown section name: {SectionName}");

            //return null;
        }

        public void ReadSubChunk()
        {
            if (_fileWalker.PeekBytes(4).SequenceEqual(Pad1) || _fileWalker.PeekBytes(4).SequenceEqual(Pad2))
                _fileWalker.CurrentIndex += 4;

            Length = _fileWalker.GetInt();
            Data = _fileWalker.GetBytes(Length);
        }
    }
}
