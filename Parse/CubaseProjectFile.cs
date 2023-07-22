using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class CubaseProjectFile
    {
        string _filename = "";
        public int _size = 0;
        private ByteWalker _fileWalker;
        int currentIndex = 0;
        public byte[] Bytes = new byte[0];

        public List<Chunk> Chunks = new List<Chunk>();

        public CubaseProjectFile()
        {

        }

        public CubaseProjectFile(string filename)
        {
            _filename = filename;
        }

        public List<DataItem> GetDataItems()
        {
            return Chunks.SelectMany(x => x.DataItems).ToList();
                ;
        }

        public DataItem GetDataItem(string pDataItemName)
        {
            return GetDataItems().Where(x => x.Name == pDataItemName).FirstOrDefault();
        }

        public void Parse()
        {
            Bytes = File.ReadAllBytes(_filename);
            _fileWalker = new ByteWalker(Bytes);

            var Header = _fileWalker.GetString(4);
            Debug.Assert(Header == "RIFF"); // Resource Interchange File Format

            var DataSize = _fileWalker.GetInt();
            var Header2 = _fileWalker.GetString(4);
            Debug.Assert(Header2 == "NUND"); // Nuendo(?) magic header

            List<Chunk> chunks = new List<Chunk>();
            while (_fileWalker.CurrentIndex < _fileWalker.TotalSize)
            {
                var c = Chunk.ReadChunk(_fileWalker);
                chunks.Add(c);

            }

            ///Chunk cROOT = new Chunk(_fileWalker);
            //cROOT.ReadChunk();

            //Chunk cARCH = new Chunk(_fileWalker);
            //cARCH.ReadChunk();


            foreach (var c in chunks)
            {
                Console.WriteLine(c.Name);
                foreach (var di in c.DataItems)
                {
                    Console.WriteLine('\t' + di.ToString());
                }
            }

            Chunks = chunks;
        }
    }
}
