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
            //var bw = new ByteWalker(data);
            
            //bw.GetBytes(0x1A);
            
            //if (bw.CurrentIndex + 6 >= bw.Length)
            //    return;

            //var subSection = DataItem.CreateDataItem(offsetInFile + 4 + name.Length + 1 + 2 + 4 +bw.CurrentIndex, entireData);

            //if (subSection is MListNode)
            //{

            //}
        }

        public override string ToString()
        {
            return $"[Audio] {AudioTrackName} ";
        }
    }
}
