using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class VST_MixerDataItem : DataItem
    {
        public int AllVstSize { get; set; } = 0;
        public byte[] AfterStereoBeforeVSTs { get; set; } = new byte[0] { };

        public byte[] NoChannelsInMixer { get; set; } = new byte[0] { };

        public byte[] PreEffects { get; set; } = new byte[0] { };

        public VST_MixerDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }

        public override byte[] GetBytes()
        {
            List<byte> toReturn = new List<byte>();
            toReturn.AddRange(GetSectionNameBytes());
            

            int numOfSections = SubSections.Count;
            int TotalDataLengthInSection =
                //0x38 +                 // 0x60000003 + .... 
                PostName.Length +
                //(4 * numOfSections) +   // Sections String Size
                //SubSections.Sum(x => x.Name.Length + 1) +
                //(4 * numOfSections) +   // SubSections Data Size in 4 byte
                //SubSections.Sum(x => x.Data.Length) +
                SubSections.Sum(x => x.GetBytes().Length) +
                4 + // AllVstSize
                AfterStereoBeforeVSTs.Length;

            //toReturn.AddRange(Data.Take(0x38));
            toReturn.AddRange(ToBigEndian(TotalDataLengthInSection));
            toReturn.AddRange(PostName);

            var so = SubSections.Where(x => x.Name == "Stereo Out").First();
            SubSections.Remove(so);
            SubSections.Insert(1, so);
            var counter = 0;

            foreach (var s in SubSections)
            {
                toReturn.AddRange(s.GetBytes());
                counter++;

                // Insert after Stereo Out before VSTs
                if (counter == 2)
                {
                    toReturn.AddRange(ToBigEndian(AllVstSize));
                    toReturn.AddRange(AfterStereoBeforeVSTs);
                }
            }
            
            SubSections.Remove(so);
            SubSections.Add(so);

            return toReturn.ToArray();
        }
    }
}
