using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class DataItemROOT : DataItem
    {
        public DataItemROOT(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

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
