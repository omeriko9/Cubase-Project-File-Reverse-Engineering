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
        

        public static DataItem Create(string name, byte[] data, int offsetInFile, byte[] entireData)
        {
            switch (name)
            {
                case "MListNode": return new MListNodeDataItem(name, data, offsetInFile, entireData);
                case "MMidiTrackEvent": return new MMidiTrackEventDataItem(name, data, offsetInFile, entireData);
                case sMTrackList: return new MTracklistDataItem(name, data, offsetInFile, entireData);
                case "MAudioTrackEvent": return new MAudioTrackEventDataItem(name, data, offsetInFile, entireData);
                case sFMemoryStream: return new FMemoryStreamDataItem(name, data, offsetInFile, entireData);
                default: return new DataItem(name, data, offsetInFile, entireData);
            }
        }

    }
}
