using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{

    public class VSTGenericChannelDataItem : DataItem
    {

        public byte[] PostEffects { get; set; } = new byte[] { };
        public VSTGenericChannelDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }
    }

    public class VSTBuiltInChannelDataItem : VSTGenericChannelDataItem
    {

        public byte[] PreName { get; set; } = new byte[] { };
        public byte[] PostSize { get; set; } = new byte[] { };



        public int SizeTillProlog { get; set; }

        public byte[] PreInsDeviceName { get; set; } = new byte[] { };
        public byte[] PreAudioChannelName { get; set; } = new byte[] { };
        public byte[] PostAudioChannelName { get; set; } = new byte[] { };

        public int SubstractFromTotalSize { get; set; }

        public string AudioChannelName { get; set; }


        public VSTBuiltInChannelDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }

        public override byte[] GetBytes()
        {
            List<byte> toReturn = new List<byte>();
            toReturn.AddRange(PreName);
            toReturn.AddRange(GetSectionNameBytes());
            toReturn.AddRange(PostName);

            var numOfSections = SubSections.Count();

            var totalChannelSize =
                PostSize.Length +
                //(4 * numOfSections) +   // Sections String Size
                //SubSections.Sum(x => x.Name.Length + 1) +
                //(4 * numOfSections) +   // SubSections Data Size in 4 byte
                //SubSections.Sum(x => x.Data.Length) +
                SubSections.Sum(x => x.GetBytes().Length) +
                PostEffects.Length;

            toReturn.AddRange(ToBigEndian(totalChannelSize - SubstractFromTotalSize));
            toReturn.AddRange(PostSize);

            foreach (var s in SubSections)
            {
                toReturn.AddRange(s.GetBytes());
            }

            toReturn.AddRange(PostEffects);


            return toReturn.ToArray();
        }
    }

    public class VSTChannelDataItem : VSTGenericChannelDataItem
    {

        public byte[] PreName { get; set; } = new byte[] { };
        //public byte[] PostSize { get; set; } = new byte[] { };

        //public byte[] PostData { get; set; } = new byte[] { };

        //public byte[] PostEffects { get; set; } = new byte[] { };

        public int SizeTillProlog { get; set; }

        public byte[] PreInsDeviceName { get; set; } = new byte[] { };
        public byte[] PreAudioChannelName { get; set; } = new byte[] { };
        public byte[] PostAudioChannelName { get; set; } = new byte[] { };

        public byte[] ChannelOnlyData { get; set; } = new byte[] { };
        public string AudioChannelName { get; set; }


        public int IsData { get; set; }


        public VSTChannelDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }

        public override byte[] GetBytes()
        {
            List<byte> toReturn = new List<byte>();

            var totalChannelSize =
                4 + // data size
                ChannelOnlyData.Length +
                4 + // isData
                PreInsDeviceName.Length +
                4 + Name.Length + 1 +
                PreAudioChannelName.Length +
                4 + AudioChannelName.Length + 1 +
                PostAudioChannelName.Length +
                SubSections.Sum(x => x.GetBytes().Length) +
                PostEffects.Length;

            //toReturn.AddRange(ToBigEndian(totalChannelSize));
            toReturn.AddRange(ToBigEndian(ChannelOnlyData.Length));
            toReturn.AddRange(ChannelOnlyData);
            toReturn.AddRange(ToBigEndian(IsData));
            toReturn.AddRange(PreInsDeviceName);
            toReturn.AddRange(GetStringSizeBigEndian(Name));
            toReturn.AddRange(StringToBytes(Name));
            toReturn.AddRange(PreAudioChannelName);
            toReturn.AddRange(GetStringSizeBigEndian(AudioChannelName));
            toReturn.AddRange(StringToBytes(AudioChannelName));
            toReturn.AddRange(PostAudioChannelName);

            foreach (var s in SubSections)
            {
                toReturn.AddRange(s.GetBytes());
            }

            toReturn.AddRange(PostEffects);

            return toReturn.ToArray();
        }
    }
}
