using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class MMidiTrackEventDataItem : DataItem
    {
        public int MIDITrackSize { get; set; }
        public string MIDITrackName { get; set; }

        public int MListNodeSize { get; set; }

        public MMidiTrackEventDataItem(string name, byte[] data, int offsetInFile, byte[] entireData) : base(name, data, offsetInFile, entireData)
        {
            //var bw = new ByteWalker(data);
            ////bw.GetBytes(3);
            ////MIDITrackSize = bw.GetInt();
            //bw.GetBytes(30);
            //if (bw.CurrentIndex + 4 >= bw.Length)
            //    return;

            //MListNodeSize = bw.GetInt();
            //var strSize = bw.GetInt();
            //MIDITrackName = bw.GetString(strSize);

        }

        public override string ToString()
        {
            return $"[MIDI] {MIDITrackName} (0x{MIDITrackSize.ToString("x4")})";
        }

    }

    public class MListNode : DataItem
    {
        public MListNode(string name, byte[] data, int offsetInFile, byte[] entire) : base(name, data, offsetInFile, entire)
        {
        }
    }
}
