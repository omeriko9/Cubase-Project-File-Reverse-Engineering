using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class DataItemFactory
    {
        public const string sMTrackList = "MTrackList";
        public const string sFMemoryStream = "FMemoryStream";
        public const string sVSTMixer = "VST Mixer";
        public const string sROOT = "ROOT";
        public const string sARCH = "ARCH";
        public const string sMListNode = "MListNode";
        public const string sMAudioTrackEvent = "MAudioTrackEvent";
        public const string sMMidiTrackEvent = "MMidiTrackEvent";


        public static DataItem Create(string name, byte[] data, int offsetInFile)
        {
            switch (name)
            {
                case sMListNode: return new MListNodeDataItem(name, data, offsetInFile);
                case sMMidiTrackEvent: return new MMidiTrackEventDataItem(name, data, offsetInFile);
                case sMTrackList: return new MTracklistDataItem(name, data, offsetInFile);
                case sMAudioTrackEvent: return new MAudioTrackEventDataItem(name, data, offsetInFile);
                case sROOT: return new DataItemROOT(name, data, offsetInFile);
                case sARCH: return new DataItemARCH(name, data, offsetInFile);
                case sFMemoryStream: return new FMemoryStreamDataItem(name, data, offsetInFile);
                default: return new DataItem(name, data, offsetInFile);
            }
        }

    }
}
