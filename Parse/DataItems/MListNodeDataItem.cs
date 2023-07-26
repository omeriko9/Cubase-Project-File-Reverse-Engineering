using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class MListNodeDataItem : DataItem
    {
        public string MIDIChannelName { get; set; }
        public int Size { get; set; }

        /*
            MListNode 
            (Const) 00 00 
            4 byte size
            4 byte MIDI channel Name size
            (variable) MIDI channel Name
            (var?) 00 00 00 00 00
            FF FF FF FF (delimiter?)
         */

        public MListNodeDataItem(string name, byte[] data, int offsetInFile)
            : base(name, data, offsetInFile)
        {
            //var bw = new ByteWalker(Data);

            //bw.GetBytes(2);
            //Size = bw.GetInt();
            //var midiChannelNameLength = bw.GetInt();
            //MIDIChannelName = bw.GetString(midiChannelNameLength);
        }
    }
}
