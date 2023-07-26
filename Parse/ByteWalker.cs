using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class ByteWalker
    {
        public byte[] _data = null;
        public int CurrentIndex = 0;
        public int OriginalOffsetInFile = 0;

        public static byte[] Pad1 = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFE };
        public static byte[] Pad2 = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
        public bool bLastOneSkipped { get; set; } = false;
        public int TotalSize { get { return _data.Length; } }
        public ByteWalker(byte[] data)
        {
            _data = data;
        }

        public ByteWalker(byte[] data, int pOriginalOffset) : this(data)
        {
            OriginalOffsetInFile = pOriginalOffset;
        }

        public int Length { get { return _data.Length; } }

        public int EatZeros()
        {
            var cur = _data.Skip(CurrentIndex).ToArray();
            int iCur = 0;
            while (cur[iCur] == 0x0)
                iCur++;

            //var noLeadingZeros = cur.TrimLeadingZeros(CurrentIndex);
            CurrentIndex += iCur; //cur.Length - noLeadingZeros.Length;
            return iCur; //cur.Length - noLeadingZeros.Length;
        }

        public uint GetUInt()
        {
            var toReturn = _data.GetUInt(CurrentIndex);
            CurrentIndex += 4;
            return toReturn;
        }

        public int GetInt()
        {
            var toReturn = PeekInt();
            CurrentIndex += 4;
            return toReturn;
        }

        public bool PeekIsDelimiter()
        {
            return PeekBytes(4).SequenceEqual(Pad1) || PeekBytes(4).SequenceEqual(Pad2);
        }

        public byte[] GetInts(int howMany)
        {
            List<byte> toReturn = new List<byte>();

            for (int i = 0; i < howMany; i++)
            {
                toReturn.AddRange(BitConverter.GetBytes(GetInt()));
            }

            return toReturn.ToArray();
        }

        public int PeekInt()
        {
            return _data.GetInt(CurrentIndex);
        }

        public byte[] GetBytes(int length)
        {
            var toReturn = _data.Skip(CurrentIndex).Take(length).ToArray();
            CurrentIndex += length;
            return toReturn;
        }

        public byte[] GetBytesUntil(bool removeStopBytes = true, params byte[][] StopBytes)
        {
            return GetBytesUntilDoNotExceed(removeStopBytes, 0, StopBytes);
        }


        public byte[] GetBytesUntilDoNotExceed(bool removeStopBytes = true, int doNotExceed = 0, params byte[][] StopBytes)
        {
            var skip = _data.Skip(CurrentIndex).ToArray();
            byte cur = skip[0];
            int iStopBytesIndex = 0;
            int iDataIndex = 1;
            List<byte> toReturn = new List<byte>();
            while (iStopBytesIndex < StopBytes.First().Length && this.CurrentIndex + iDataIndex < this.TotalSize)
            {
                if (doNotExceed > 0 && this.CurrentIndex + iDataIndex >= doNotExceed)
                    return null;

                toReturn.Add(cur);

                if (StopBytes.Any(x => x[iStopBytesIndex] == cur))
                {
                    iStopBytesIndex++;
                    cur = skip[iDataIndex++];
                    continue;
                }

                iStopBytesIndex = 0;

                cur = skip[iDataIndex++];
            }

            if (removeStopBytes)
                toReturn = toReturn.Take(toReturn.Count - StopBytes.First().Length).ToList();

            CurrentIndex += toReturn.Count();
            return toReturn.ToArray();
        }

     
        public byte[] PeekBytes(int length)
        {
            return _data.Skip(CurrentIndex).Take(length).ToArray();
        }


        public string GetStringBySize()
        {
            var size = GetInt();
            return GetString(size);
        }
        public string GetString(int length)
        {
            var toReturn = _data.GetString(CurrentIndex, length);
            CurrentIndex += length;
            return toReturn.TrimEnd('\0');
        }

    }
}
