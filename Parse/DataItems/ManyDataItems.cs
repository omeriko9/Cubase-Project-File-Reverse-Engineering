using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.DataItems
{
    public class PDrumMapEntryDataItem : DataItem
    {
        public PDrumMapEntryDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;            
        }
    }

    public class PLayoutTrackDataItem : DataItem
    {
        public PLayoutTrackDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;            
        }
    }

    public class FAttributesDataItem : DataItem
    {
        public FAttributesDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class UColorSetDataItem : DataItem
    {
        public UColorSetDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class PPoolDataItem : DataItem
    {
        public PPoolDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class GTreeDataItem : DataItem
    {
        public GTreeDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            
        }
    }

    public class FNPathDataItem : DataItem
    {
        public FNPathDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            
        }
    }

    public class MMarkerTrackEventDataItem : DataItem
    {
        public MMarkerTrackEventDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class MAutoFadeSettingDataItem : DataItem
    {
        public MAutoFadeSettingDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class MInterpolatorDataItem : DataItem
    {
        public MInterpolatorDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class MLinearInterpolatorDataItem : DataItem
    {
        public MLinearInterpolatorDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    internal class FMemoryStreamDataItem : DataItem
    {
        public FMemoryStreamDataItem(string name, byte[] pdata, int offsetInFile) : base(name, pdata, offsetInFile)
        {

        }
    }

    public class MDataNodeDataItem : DataItem
    {
        public MDataNodeDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }
    }

    public class MFolderTrackDataItem : DataItem
    {
        public MFolderTrackDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }
    }

    public class MGridQuantizeDataItem : DataItem
    {
        public MGridQuantizeDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class MMidiQuantizeDataItem : DataItem
    {
        public MMidiQuantizeDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;

        }
    }

    public class MTrackEventDataItem : DataItem
    {
        public MTrackEventDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {

        }
    }

    public class MRootDataItem : DataItem
    {
        public MRootDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class PAMDDataItem : DataItem
    {
        public PAMDDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }

    public class PArrangementDataItem : DataItem
    {
        public PArrangementDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            SectionSize = BitConverter.ToInt32(data.Reverse().ToArray(), 0);
            IsDataLengthPartOfData = true;
        }
    }

    public class PDrumMapDataItem : DataItem
    {
        public PDrumMapDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }
    public class PDrumMapPoolDataItem : DataItem
    {
        public PDrumMapPoolDataItem(string name, byte[] data, int offsetInFile) : base(name, data, offsetInFile)
        {
            IsDataLengthPartOfData = true;
        }
    }
}
