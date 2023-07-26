using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class MTrackEventDataItem : DataItem
    {
        public MTrackEventDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
        }
    }
}
