using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    internal class FMemoryStreamDataItem : DataItem
    {

        public int AnotherSize { get; set; }
        public int NumberOfSomething { get; set; }
        public FMemoryStreamDataItem(string name, byte[] pdata, int offsetInFile, byte[] entireData) : base(name, pdata, offsetInFile, entireData)
        {
            ByteWalker bw = new ByteWalker(Data);
            this.AnotherSize = bw.GetInt();
            this.NumberOfSomething = bw.GetInt();
            this.DataOffsetInSection += 8;
            Data = Data.Skip(8).ToArray();
        }
    }
}
