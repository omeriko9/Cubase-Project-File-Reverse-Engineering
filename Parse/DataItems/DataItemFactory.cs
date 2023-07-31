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
        public const string sCmObject = "CmObject";
        public const string sCmIDLink = "CmIDLink";
        public const string sMDataNode = "MDataNode";
        public const string sPArrangement = "PArrangement";
        public const string sMRoot = "MRoot";
        public const string sPAMD = "PAMD";
        public const string sPDrumMapPool = "PDrumMapPool";

        public static DataItem Create(string name, byte[] data, int offsetInFile)
        {

            var listOfBs = (
               from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                   // alternative: from domainAssembly in domainAssembly.GetExportedTypes()
               from type in domainAssembly.GetTypes()
               where typeof(DataItem).IsAssignableFrom(type)
               // alternative: && type != typeof(B)
               // alternative: && ! type.IsAbstract
               // alternative: where type.IsSubclassOf(typeof(B))
               select type).ToArray();

            var ourType = listOfBs.Where(x => x.Name.Replace("DataItem", "").Replace("_", " ") == name).FirstOrDefault();

            if (ourType == null)
            {
                return new DataItem(name, data, offsetInFile);
            }

            var inst = Activator.CreateInstance(ourType, name, data, offsetInFile);

            return (DataItem)inst;



            switch (name)
            {
                case sMListNode: return new MListNodeDataItem(name, data, offsetInFile);
                case sMMidiTrackEvent: return new MMidiTrackEventDataItem(name, data, offsetInFile);
                case sMTrackList: return new MTrackListDataItem(name, data, offsetInFile);
                case sMAudioTrackEvent: return new MAudioTrackEventDataItem(name, data, offsetInFile);
                case sROOT: return new DataItemROOT(name, data, offsetInFile);
                case sARCH: return new DataItemARCH(name, data, offsetInFile);
                case sFMemoryStream: return new FMemoryStreamDataItem(name, data, offsetInFile);
                case sCmObject: return new CmObjectDataItem(name, data, offsetInFile);
                case sCmIDLink: return new CmIDLinkDataItem(name, data, offsetInFile);
                case sMDataNode: return new MDataNodeDataItem(name, data, offsetInFile);
                case sPArrangement: return new PArrangementDataItem(name, data, offsetInFile);
                case sMRoot: return new MRootDataItem(name, data, offsetInFile);
                case sPAMD: return new PAMDDataItem(name, data, offsetInFile);
                case sPDrumMapPool: return new PDrumMapPoolDataItem(name, data, offsetInFile);
                default: return new DataItem(name, data, offsetInFile);
            }
        }

    }
}

