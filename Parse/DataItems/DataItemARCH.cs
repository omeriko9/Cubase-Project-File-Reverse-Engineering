using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class DataItemARCH : DataItem
    {
        public DataItemARCH(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }

        public override byte[] GetBytes()
        {
            List<byte> toReturn = new List<byte>();
            toReturn.AddRange(GetHeader());
            toReturn.AddRange(Suffix);

            foreach (var s in SubSections)
            {

                if (s.Nick != 0)
                {
                    toReturn.AddRange(ToBigEndian(s.Nick));
                }
                else
                {
                    toReturn.AddRange(GetStringSizeBigEndian(s.Name));
                    toReturn.AddRange(StringToBytes(s.Name));
                }

                if (s.PostName != null)
                    toReturn.AddRange(s.PostName);

                if (s.Data == null || s.Data.Length == 0)
                {
                    if (s.Suffix.Length > 0)
                        toReturn.AddRange(s.Suffix);
                    //toReturn.AddRange(s.EndDelimiter);
                }
                else
                {
                    if (!s.IsContainer)
                    {
                        toReturn.AddRange(ToBigEndian(s.Data.Length));
                        toReturn.AddRange(s.Data);

                        //if (s.AddDelimiterEvenNotContainer)
                        //    toReturn.AddRange(s.EndDelimiter);
                        if (s.Suffix.Length > 0)
                            toReturn.AddRange(s.Suffix);
                    }
                    else
                    {
                        toReturn.AddRange(s.Data);
                        if (s.Suffix.Length > 0)
                            toReturn.AddRange(s.Suffix);
                        //else
                        //    toReturn.AddRange(s.EndDelimiter);
                    }

                }
            }

            return toReturn.ToArray();
        }

        public override byte[] GetHeader()
        {
            List<byte> toReturn = new List<byte>();
            toReturn.AddRange(Encoding.ASCII.GetBytes(Name));
            toReturn.AddRange(ToBigEndian(Data.Length));
            //toReturn.AddRange(EndDelimiter);           

            return toReturn.ToArray();
        }
    }
}
