using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class DataItemARCH : DataItem
    {
        public DataItemARCH(string name, byte[] data, int offsetInFile, byte[] entireData) : base(name, data, offsetInFile, entireData)
        {

        }
    }
}
