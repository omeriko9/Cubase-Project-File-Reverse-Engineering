using Parse.DataItems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parse
{
    public class DataItem
    {
        public string Name { get; set; }
        public byte[] Data { get; set; } = new byte[0];
        public bool IsDataLengthPartOfData { get; set; } = false;
        public byte[] Suffix { get; set; } = new byte[0];   
        public int SectionSize { get; set; }
        public int DataOffsetInSection { get; set; }
        public int DataOffsetInFile { get { return DataOffsetInSection + OffsetInFile; } }
        public int Nick { get; set; }
        public int OffsetInFile { get; set; }
        public byte[] PostName { get; set; } = new byte[] { };
        public int OffsetInSection { get; set; }
        public List<DataItem> SubSections { get; set; } = new List<DataItem>();



        public override string ToString()
        {
            var str = $"{Name} [0x{OffsetInFile.ToString("x4")}]";
            if (Data?.Length > 0)
                str += $" (0x{Data.Length.ToString("x4")})";
            else
                str += " (0)";
            return str;
        }


        public virtual byte[] GetHeader()
        {
            return null;
        }

        public virtual byte[] GetBytes()
        {
            return null;
        }

        public byte[] GetSectionNameBytes()
        {
            List<byte> toReturn = new List<byte>();

            if (Nick != 0)
            {
                toReturn.AddRange(ToBigEndian(Nick));
            }
            else
            {
                toReturn.AddRange(GetStringSizeBigEndian(Name));
                toReturn.AddRange(StringToBytes(Name));
            }

            return toReturn.ToArray();
        }

        public byte[] GetStringSizeBigEndian(string toGet)
        {
            return BitConverter.GetBytes(toGet.Length + 1).Reverse().ToArray();
        }

        public byte[] ToBigEndian(int i)
        {
            return BitConverter.GetBytes(i).Reverse()  .ToArray();
        }

      

        public byte[] StringToBytes(string str)
        {
            var tr = new List<byte>(Encoding.ASCII.GetBytes(str));
            tr.Add(0x0);
            return tr.ToArray();
        }

        public string DataStr
        {
            get
            {
                return string.Join(Environment.NewLine,
                    Encoding.ASCII.GetString(Data.Select(c => (c >= 32 && c <= 127) ? c : (byte)63).ToArray())
                    .Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Count() > 1));
            }
        }

       

        public DataItem(string name, byte[] data, int offsetInFile)
        {
            Name = name;
            Data = data;           
            OffsetInFile = offsetInFile;
        }

        public static (int, string) GetBackReferenceString(int backReferenceOffset, byte[] _data)
        {
            ByteWalker backReferenceBW = new ByteWalker(_data);
            backReferenceBW.CurrentIndex = (int)((long)backReferenceOffset - 0x80000000) + 0x40;
            var DataItemNameSize = backReferenceBW.GetInt();
            var DataItemName = backReferenceBW.GetString(DataItemNameSize);

            return (DataItemNameSize, DataItemName);
        }


      
        public static bool IsSectionNameBackReference(int pSectionNameSize)
        {
            return ((pSectionNameSize >> 31) & 1) == 1;
        }
              
    }
}
