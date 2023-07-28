using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    internal class MAudioTrackEventDataItem : DataItem
    {
        public string AudioTrackName { get; set; }

        public MAudioTrackEventDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
         
        }

        public override string ToString()
        {
            return $"[Audio] {AudioTrackName} ";
        }
    }
}
