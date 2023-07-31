using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class FMemoryStreamDataItem : DataItem
    {
        public FMemoryStreamDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }

        public override byte[] GetBytes()
        {
            List<byte> toReturn = new List<byte>();
            toReturn.AddRange(GetSectionNameBytes());
            toReturn.AddRange(PostName);

            int numOfSections = SubSections.Count;
            int TotalDataLengthInSection =
                4 + 4 +                 // Second Data Length + Number of sections
                SubSections.Sum(x => x.GetBytes().Length);
           
            toReturn.AddRange(ToBigEndian(TotalDataLengthInSection));
            toReturn.AddRange(ToBigEndian(TotalDataLengthInSection - 4));
            toReturn.AddRange(ToBigEndian(numOfSections));

            foreach (var s in SubSections)
            {
                toReturn.AddRange(s.GetBytes());
            }

            return toReturn.ToArray();
        }
    }
}
