using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class CmObjectDataItem : DataItem
    {
        public CmObjectDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            
        }
    }

    public class CmIDLinkDataItem : DataItem
    {
        public CmIDLinkDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            
        }
    }
}
