using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class MFolderTrackDataItem : DataItem
    {
        public MFolderTrackDataItem(string name, byte[] data, int offsetInFile, byte[] entireData) : base(name, data, offsetInFile, entireData)
        {
            ByteWalker bw = new ByteWalker(data);

            //// eat 3 bytes 00 00 00
            //bw.GetBytes(3);
            //SectionSize = bw.GetInt();

        }
    }
}
