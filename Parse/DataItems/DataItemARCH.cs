using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class DataItemARCH : DataItem
    {
        public DataItemARCH(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }

        public override byte[] GetBytes()
        {
            List<byte> toReturn = new List<byte>();
            toReturn.AddRange(GetHeader());
            toReturn.AddRange(Suffix);

            foreach (var s in SubSections)
            {
                toReturn.AddRange(s.GetSectionNameBytes());
                toReturn.AddRange(s.PostName);

                if (!s.IsDataLengthPartOfData && s.Data.Length > 0)
                {
                    toReturn.AddRange(ToBigEndian(s.Data.Length));
                }

                toReturn.AddRange(s.Data);
                toReturn.AddRange(s.Suffix);

            }

            return toReturn.ToArray();
        }

        public override byte[] GetHeader()
        {
            List<byte> toReturn = new List<byte>();
            toReturn.AddRange(Encoding.ASCII.GetBytes(Name));
            toReturn.AddRange(ToBigEndian(Data.Length));

            return toReturn.ToArray();
        }
    }
}
