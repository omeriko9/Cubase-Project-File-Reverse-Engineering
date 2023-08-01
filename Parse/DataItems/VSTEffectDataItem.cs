using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class VSTEffectDataItem : DataItem
    {
        public byte[] PreSize { get; set; } = new byte[] { };
        public byte[] PostSize { get; set; } = new byte[] { };
        public byte[] PostEffect { get; set; } = new byte[] { };

        public int SizeToEffectEpilog { get; set; }
        public byte[] PostSizeToEpilog { get; set; } = new byte[] { };

        public byte[] UntilStringEpilogSize { get; set; } = new byte[] { };

        public byte[] PostFullName { get; set; } = new byte[] { };

        private string pFullName = "";
        public string FullName
        {
            get { return pFullName; }
            set
            {
                pFullName = value;
                FullNameWithoutName = FullName.IndexOf(Name) > 0 ? FullName.Substring(0, FullName.IndexOf(Name)) : FullName;
            }
        }

        public string FullNameWithoutName { get; set; }


        public VSTEffectDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }

        public override byte[] GetBytes()
        {
            var toReturn = new List<byte>();

            // Empty Effect Slot
            if (Data.Length < 0x10)
            {
                toReturn.AddRange(PreSize.Merge(ToBigEndian(PostSize.Length), PostSize));
                return toReturn.ToArray();
            }

            FullName = FullNameWithoutName + Name;
            var iNameLength = GetStringSizeBigEndian(Name);
            var iNameBytes = StringToBytes(Name);
            var iFullNameLength = GetStringSizeBigEndian(FullName);
            var iFullNameBytes = StringToBytes(FullName);


            var totalSize =
                PreSize.Length +
                4 + // Total Size
                PostSize.Length +

                4 + // Full Name Size
                FullName.Length + 1 +

                PostFullName.Length +

                4 + // SizeToEffectEpilog 
                PostSizeToEpilog.Length +
                4 + // Size Until String Epilog
                UntilStringEpilogSize.Length + // Bytes until string epilog

                4 + // name size
                Name.Length + 1; // name


            toReturn.AddRange(PreSize);
            toReturn.AddRange(ToBigEndian(totalSize-8));
            toReturn.AddRange(PostSize);

            toReturn.AddRange(iFullNameLength);
            toReturn.AddRange(iFullNameBytes);

            toReturn.AddRange(PostFullName);

            toReturn.AddRange(ToBigEndian(SizeToEffectEpilog));
            toReturn.AddRange(PostSizeToEpilog);
            toReturn.AddRange(ToBigEndian(UntilStringEpilogSize.Length));
            toReturn.AddRange(UntilStringEpilogSize);

            toReturn.AddRange(iNameLength);
            toReturn.AddRange(iNameBytes);

            toReturn.AddRange(PostName);

            return toReturn.ToArray();
        }
    }
}
