using Parse.DataItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parse
{
    public class CPR2
    {
        public byte[] Header { get; private set; }
        public byte[] Data { get; private set; }

        public readonly string sROOT = "ROOT";
        public readonly string sARCH = "ARCH";
        public readonly string sPArrangement = "PArrangement";

        public static byte[] Pad1 = ByteWalker.Pad1;
        public static byte[] Pad2 = ByteWalker.Pad2;

        public static byte[] VSTSeparator = new byte[] { 0xAB, 0x19, 0xCD, 0x2B };

        public List<DataItem> FoundSections { get; set; } = new List<DataItem>();

        public DataItem VSTMixer { get; set; } = null;
        public DataItem TrackList { get; set; } = null;

        private byte[] bs2 = new byte[0];
        //bool bLastOneSkipped = false;

        public void Parse(byte[] bytes)
        {
            Data = bytes;
            FillHeader();
            FillSections();
            FillTracks();
            FillVSTMixer();
            VSTMixer = GetSection(DataItemFactory.sVSTMixer);
            TrackList = GetSection(DataItemFactory.sMTrackList);

        }

        void FillHeader()
        {
            ByteWalker bw = new ByteWalker(Data);
            Header = bw.GetBytesUntil(true, Encoding.ASCII.GetBytes("ROOT"));
        }

        void FillSections()
        {
            ByteWalker bw = new ByteWalker(Data);
            bw.GetBytesUntil(true, Encoding.ASCII.GetBytes("ROOT"));

            // Top Level Sections
            while (bw.CurrentIndex < bw.Length)
            {
                bw.bLastOneSkipped = false;

                bs2 = bw.GetBytes(4); // ROOT

                if (bw.CurrentIndex + 1 > bw.Length)
                    break;

                var rootSize = bw.GetInt();

                FoundSections.Add(DataItemFactory.Create(DataItemFactory.sROOT, bw.GetBytes(rootSize), bw.CurrentIndex));
                bs2 = bw.GetBytes(4); // ARCH
                var archSize = bw.GetInt();

                var nextROOToffset = archSize + bw.CurrentIndex;
                var arch = DataItemFactory.Create(DataItemFactory.sARCH, bw.PeekBytes(archSize), bw.CurrentIndex);
                FoundSections.Add(arch);

                DataItem LastDataItem = arch;

                while (bw.CurrentIndex < nextROOToffset)
                {
                    var di = LastDataItem = GetNextSection(bw, nextROOToffset, LastDataItem);
                    if (di != null)
                    {
                        arch.SubSections.Add(di);
                    }
                    else
                        break;
                }

            }
        }

        void FillTracks()
        {
            // MTrackList
            var trackList = GetSection(DataItemFactory.sMTrackList) as MTrackListDataItem;
            var bwTrackList = new ByteWalker(trackList.Data);
            trackList.UntitledString = bwTrackList.GetStringBySize();

            DataItem LastDataItem = null;
            // Get Tracks
            while (bwTrackList.Length - bwTrackList.CurrentIndex - 0x1f > 0)
            {
                var track = LastDataItem = GetNextSection(bwTrackList, bwTrackList.Length, LastDataItem);
                track.OffsetInSection = track.OffsetInFile;
                track.OffsetInFile = track.OffsetInSection + trackList.OffsetInFile + 4 + trackList.Name.Length + 1 + 2 + 4;

                if (track is MMidiTrackEventDataItem || track is MAudioTrackEventDataItem)
                {
                    var bwnode = new ByteWalker(track.Data);
                    bwnode.bLastOneSkipped = true;
                    bwnode.CurrentIndex += 26;
                    var node = LastDataItem = GetNextSection(bwnode, track.SectionSize, LastDataItem);
                    var bwname = new ByteWalker(node.Data);
                    bwname.CurrentIndex += 4;
                    if (track is MAudioTrackEventDataItem)
                    {
                        (track as MAudioTrackEventDataItem).AudioTrackName = bwname.GetStringBySize();
                    }
                    if (track is MMidiTrackEventDataItem)
                    {
                        (track as MMidiTrackEventDataItem).MIDITrackName = bwname.GetStringBySize();
                    }
                }
                trackList.SubSections.Add(track);
            }
        }

        void FillVSTMixer()
        {
            FMemoryStreamDataItem memoryStream = GetSection(DataItemFactory.sFMemoryStream) as FMemoryStreamDataItem;
            ByteWalker bwMemoryStream = new ByteWalker(memoryStream.Data);
            var rollingOffset = memoryStream.DataOffsetInFile + 8;
            bwMemoryStream.GetBytes(8);

            while (bwMemoryStream.CurrentIndex < memoryStream.Data.Length)
            {
                var msItemName = bwMemoryStream.GetStringBySize();
                var msItemSize = bwMemoryStream.GetInt();
                var offsetInSec = bwMemoryStream.CurrentIndex;
                var offsetInF = rollingOffset;
                var dataOffset = 4 + msItemName.Length + 1 + 4;
                DataItem di = DataItemFactory.Create(msItemName, bwMemoryStream.GetBytes(msItemSize), offsetInF);
                di.DataOffsetInSection = dataOffset;

                di.SectionSize = msItemSize;
                memoryStream.SubSections.Add(di);
                rollingOffset = bwMemoryStream.CurrentIndex + memoryStream.DataOffsetInFile;
            }

            VST_MixerDataItem vstMixer = GetSection("VST Mixer") as VST_MixerDataItem;// memoryStream.SubSections.Where(x => x.Name.Equals("VST Mixer")).First();
            var bwVSTMixer = new ByteWalker(vstMixer.Data, vstMixer.DataOffsetInFile);

            //bwVSTMixer.CurrentIndex += (4 + 29 + (5 * 4) + 3);
            vstMixer.PostName = bwVSTMixer.GetBytes((4 + 29 + (5 * 4) + 3));

            var stereoInStereoOutTotalOffset = 0;

            // Stereo In, Stereo Out
            VSTBuiltInChannelDataItem StereoOut = null;
            var distancesToNextString = new int[] { 0x26, 0x12A, 0x11C, 0x36 - 4 };
            for (int i = 0; i < 2; i++)
            {
                var preName = bwVSTMixer.GetBytes(8);
                var vstItemName = bwVSTMixer.GetStringBySize();
                var four = bwVSTMixer.GetBytes(4); //bwVSTMixer.GetInt();
                var vstSize = bwVSTMixer.GetInt();
                var offsetInF = bwVSTMixer.CurrentIndex + vstMixer.OffsetInFile - 1;
                var di = new VSTBuiltInChannelDataItem(vstItemName, bwVSTMixer.PeekBytes(vstSize), offsetInF);
                di.PreName = preName.ToArray();
                di.PostName = four;
                di.SectionSize = vstSize;

                for (int z = 0; z < distancesToNextString.Length - 1; z++)
                {
                    bs2 = bwVSTMixer.GetBytes(distancesToNextString[z]);
                    di.PostSize = di.PostSize.Merge(bs2);
                    var sSize = bwVSTMixer.GetInt();
                    var sString = bwVSTMixer.GetBytes(sSize);
                    di.PostSize = di.PostSize.Merge(BitConverter.GetBytes(sSize).Reverse().ToArray(), sString);

                    //var str = bwVSTMixer.GetStringBySize();
                }

                bs2 = bwVSTMixer.GetBytes(distancesToNextString[distancesToNextString.Length - 1]);
                di.PostSize = di.PostSize.Merge(bs2);

                FillEffects(di, bwVSTMixer, vstMixer.OffsetInFile);
                if (i == 0)
                {
                    vstMixer.SubSections.Add(di);
                    bs2 = bwVSTMixer.GetBytesUntil(true, new byte[] { 0xAB, 0x19, 0xCD, 0x2A });
                    di.PostEffects = di.PostEffects.Merge(bs2);
                }
                else
                {
                    StereoOut = di;
                }

                distancesToNextString = new int[] { 0x26, 0x87 };
            }

            stereoInStereoOutTotalOffset = vstMixer.SubSections.Last().OffsetInFile + vstMixer.SubSections.Last().SectionSize + 0x13;

            bs2 = bwVSTMixer.GetBytesUntil(true, VSTSeparator);
            StereoOut.PostEffects = StereoOut.PostEffects.Merge(bs2);
            StereoOut.SubstractFromTotalSize = 8;

            if (!bwVSTMixer.PeekBytes(4).SequenceEqual(VSTSeparator))
                return;

            var indexAfterStereoOut = bwVSTMixer.CurrentIndex;

            // All Vsts Prefix
            //var abThing2 = bwVSTMixer.GetInt();         // AB 19 CD 2B (VSTSeparator)
            //var oneThing2 = bwVSTMixer.GetInt();        // 00 00 00 01 
            StereoOut.PostEffects = StereoOut.PostEffects.Merge(bwVSTMixer.GetBytes(4 + 4));
            var allVstSize = bwVSTMixer.GetInt();
            vstMixer.AllVstSize = allVstSize;

            //var vstPrefix = bwVSTMixer.GetBytes(4 + 4 + 4 + 4); // 12        // 00 00 00 00 00 00 00 06 00 00 00 40 00 00 00 02                                             00 00 00 02
            vstMixer.AfterStereoBeforeVSTs = bwVSTMixer.GetBytes(16);


            while (bwVSTMixer.CurrentIndex < allVstSize)
            {
                var startOffset = bwVSTMixer.CurrentIndex + vstMixer.OffsetInFile + 18;
                var sizeTillProlog = bwVSTMixer.GetInt();
                var ChannelOnlyData = bwVSTMixer.GetBytes(sizeTillProlog);
                var endOfChannelOffset = bwVSTMixer.CurrentIndex + sizeTillProlog;

                var isData = bwVSTMixer.GetInt();


                // No channels in VST Mixer
                if (isData == 0)
                {
                    vstMixer.NoChannelsInMixer = BitConverter.GetBytes(sizeTillProlog).Reverse().ToArray().Merge(ChannelOnlyData);

                    return;
                }

                var bs2PreInsDeviceName = bwVSTMixer.GetBytes(8);
                var InsDeviceName = bwVSTMixer.GetString(bwVSTMixer.GetInt());
                var bs2PreAudioChannelName = bwVSTMixer.GetBytes(4 + 4 + (9 * 4) + 2);
                var InsAudioChannelName = bwVSTMixer.GetString(bwVSTMixer.GetInt());
                var bs2PostAudioChannelName = bwVSTMixer.GetBytes((16 * 4) + 2 - 10);

                VSTChannelDataItem VSTChannel = new VSTChannelDataItem(InsDeviceName, ChannelOnlyData, startOffset);
                VSTChannel.SectionSize = sizeTillProlog;
                VSTChannel.SizeTillProlog = sizeTillProlog;
                VSTChannel.PreInsDeviceName = bs2PreInsDeviceName;
                VSTChannel.PreAudioChannelName = bs2PreAudioChannelName;
                VSTChannel.PostAudioChannelName = bs2PostAudioChannelName;
                VSTChannel.AudioChannelName = InsAudioChannelName;
                VSTChannel.IsData = isData;
                VSTChannel.ChannelOnlyData = ChannelOnlyData;



                FillEffects(VSTChannel, bwVSTMixer, vstMixer.OffsetInFile);

                vstMixer.SubSections.Add(VSTChannel);

                // try get next one:
                var indexBeforeThingy = bwVSTMixer.CurrentIndex;
                var intt = bwVSTMixer.GetInt();
                bs2 = bwVSTMixer.GetBytes(intt);
                bs2 = bwVSTMixer.GetBytes(6);
                intt = bwVSTMixer.GetInt();
                bs2 = bwVSTMixer.GetBytes(intt);
                bs2 = bwVSTMixer.GetBytes(6);
                intt = bwVSTMixer.GetInt();
                bs2 = bwVSTMixer.GetBytes(intt);
                bs2 = bwVSTMixer.GetBytes(34 + 4);
                var indexAfterThiny = bwVSTMixer.CurrentIndex;
                bwVSTMixer.CurrentIndex = indexBeforeThingy;
                VSTChannel.PostEffects = VSTChannel.PostEffects.Merge(bwVSTMixer.GetBytes(indexAfterThiny - indexBeforeThingy));

                //if (bwVSTMixer.PeekInt() == 9)
                //{
                //    VSTChannel.PostEffects.Merge(new byte[] { 0, 0, 0, 9 });
                //    break;
                //}

                // Last one
                if (bwVSTMixer.PeekInt() == 9 || bwVSTMixer.CurrentIndex >= allVstSize)
                {
                    var sectionSizeUpdated = vstMixer.SectionSize;// + 8 + vstMixer.Name.Length + 1;
                    var toRead = sectionSizeUpdated - bwVSTMixer.CurrentIndex;
                    VSTChannel.PostEffects = VSTChannel.PostEffects.Merge(bwVSTMixer.GetBytes(toRead));
                }
            }

            vstMixer.SubSections.Add(StereoOut);
        }


        public void Save(string pFileName)
        {
            List<byte> toSave = new List<byte>();
            toSave.AddRange(Header);
            foreach (var s in FoundSections)
            {
                toSave.AddRange(s.GetBytes());
            }

            // Total size
            var tSize = BitConverter.GetBytes(toSave.Count() - 12).Reverse().ToArray();
            toSave[4] = tSize[0];
            toSave[5] = tSize[1];
            toSave[6] = tSize[2];
            toSave[7] = tSize[3];

            File.WriteAllBytes(pFileName, toSave.ToArray());
        }


        public DataItem GetSection(string SectionName)
        {
            return GetSection(SectionName, FoundSections);
        }

        public DataItem GetSection(string SectionName, List<DataItem> sections = null)
        {
            foreach (var sec in sections)
            {
                if (sec.Name.Equals(SectionName))
                    return sec;

                var found = GetSection(SectionName, sec.SubSections);
                if (found != null)
                    return found;
            }

            return null;
        }

        public void FillEffects(VSTGenericChannelDataItem Channel, ByteWalker bw, int iMixerOffsetInFile)
        {
            while (true)
            {
                var indexBefore = bw.CurrentIndex;
                var effect = GetEffectsForChannel(bw);

                if (String.IsNullOrEmpty(effect.Name))
                {
                    Channel.PostEffects = Channel.PostEffects.Merge(effect.Data);
                    break;
                }

                if (effect == null)
                    break;

                effect.OffsetInFile = bw.OriginalOffsetInFile + indexBefore + 8;

                Channel.SubSections.Add(effect);
            }
        }

        public DataItem GetEffectsForChannel(ByteWalker bwVSTMixer)
        {
            var bPreSize = bwVSTMixer.GetBytes(4);
            var EffectTotalSize = bwVSTMixer.GetInt();
            var effectBytes = bwVSTMixer.PeekBytes(EffectTotalSize);
            var postSize = bwVSTMixer.GetBytes(2);

            if (EffectTotalSize < 0x10)
            {
                postSize = postSize.Merge(bwVSTMixer.GetBytes(EffectTotalSize - 2));
                VSTEffectDataItem emptySlot = new VSTEffectDataItem("{Empty}", new byte[] { }, 0);
                emptySlot.PreSize = bPreSize;
                emptySlot.PostSize = postSize;

                return emptySlot;
            }

            if (EffectTotalSize == 0x60000)
                return new DataItem("", bPreSize.Merge(BitConverter.GetBytes(EffectTotalSize).Reverse().ToArray(), postSize), 0);

            var sFirstEffect = bwVSTMixer.GetStringBySize();

            if (String.IsNullOrEmpty(sFirstEffect))
                return new DataItem("", bPreSize.Merge(BitConverter.GetBytes(EffectTotalSize).Reverse().ToArray(), postSize), 0);


            var postFullName = bwVSTMixer.GetBytes(5 * 4);
            var sizeToEffectEpilog = bwVSTMixer.GetInt();
            var postSizeToEpilog = bwVSTMixer.GetBytes(sizeToEffectEpilog);
            //bs2 = bwVSTMixer.GetBytes(30);

            var indexBeforeStuff = bwVSTMixer.CurrentIndex;

            for (int j = 0; j < 2; j++)
            {
                var howMany = bwVSTMixer.GetInt();
                if (howMany > 0x10000)
                    return null;

                for (int i = 0; i < howMany; i++)
                    bs2 = bwVSTMixer.GetBytes(4);
            }

            var indexAfterStuff = bwVSTMixer.CurrentIndex;
            bwVSTMixer.CurrentIndex = indexBeforeStuff;
            postSizeToEpilog = postSizeToEpilog.Merge(bwVSTMixer.GetBytes(indexAfterStuff - indexBeforeStuff));
            postSizeToEpilog = postSizeToEpilog.Merge(bwVSTMixer.GetBytes(2 + 4));

            var sizeUntilStringEpilogSize = bwVSTMixer.GetInt();
            var bytesUntilStringEpilog = bwVSTMixer.GetBytes(sizeUntilStringEpilogSize); // bwVSTMixer.CurrentIndex += sizeUntilStringEpilogSize;
            var strbs = bwVSTMixer.GetStringBySize();


            VSTEffectDataItem effect = new VSTEffectDataItem(strbs, effectBytes, 0);
            effect.SizeToEffectEpilog = sizeToEffectEpilog;
            effect.PostSize = postSize;
            effect.PostSizeToEpilog = postSizeToEpilog;
            effect.UntilStringEpilogSize = bytesUntilStringEpilog;
            effect.PreSize = bPreSize;
            effect.FullName = sFirstEffect;
            effect.PostFullName = postFullName;
            return effect;
        }

        public DataItem GetNextSection(ByteWalker bw, int maxOffset, DataItem LastDataItem = null)
        {
            // OK let's do this
            // 
            // Scan until next FF FF FF FF or FF FF FF FE prefix
            // Then, Check if it's a standard section name or backreference
            // If it's standard section name, get size, get string, get 2 bytes
            // If it's backreference, get string name from back referencing it duh
            // Now, if there's no size - add all bytes until next delimiter.
            // If there's size, get size as data
            // Skip if required
            // Use constant size if required (FveD arggghhhh)
            // Add extra length if required (FTree arggshfdgjko)
            // Return section and wait for further calls


            if (!bw.bLastOneSkipped)
            {
                var res = bw.GetBytesUntilDoNotExceed(false, maxOffset, Pad1, Pad2);

                if (res == null)
                    return null;

                if (LastDataItem != null)
                    LastDataItem.Suffix = res;
            }
            else
                bw.bLastOneSkipped = false;

            var offsetInFile = bw.CurrentIndex;

            var test = bw.GetInt();

            if (BitConverter.GetBytes(test).SequenceEqual(Pad2))
            {
                //bw.GetBytes(2);
                offsetInFile = bw.CurrentIndex;
                test = bw.GetInt();
            }

            var sectionName = "";
            byte[] sectionPostName = new byte[] { };
            var sectionNameSize = 0;
            int nick = 0;

            if (DataItem.IsSectionNameBackReference(test))
            {
                (sectionNameSize, sectionName) = DataItem.GetBackReferenceString(test, Data);
                nick = test;
            }
            else
            {
                sectionNameSize = test;
                sectionName = bw.GetString(sectionNameSize);
                var twoB = 2;

                if (Sections.ContainsKey(sectionName))
                    twoB = Sections[sectionName].TwoBytesLength;

                sectionPostName = bw.GetBytes(twoB);
            }


            var indexBeforeData = bw.CurrentIndex;
            byte[] sectionData = new byte[] { };
            byte[] suffixCurrent = null;

            if (Sections.ContainsKey(sectionName))
            {
                var secMeta = Sections[sectionName];

                if (secMeta.HasSize)
                {

                    var sectionSize = bw.GetInt();
                    indexBeforeData += 4;
                    sectionData = bw.GetBytes(sectionSize);
                }
                else if (secMeta.FixedSize > 0)
                {
                    sectionData = bw.GetBytes(secMeta.FixedSize);
                }

                if (secMeta.Skip)
                {
                    bw.bLastOneSkipped = true;
                }

                if (secMeta.AddToSize > 0)
                {
                    //bw.CurrentIndex += secMeta.AddToSize;
                    suffixCurrent = bw.GetBytes(secMeta.AddToSize);
                }

                if (secMeta.ShouldEatZeroz)
                {
                    var howManyZeros = bw.EatZeros();
                    suffixCurrent = Enumerable.Repeat((byte)0x0, howManyZeros).ToArray();
                }
            }
            else
            {
                sectionData = bw.GetBytesUntil(true, Pad1, Pad2);
            }

            var toReturn = DataItemFactory.Create(sectionName, sectionData, offsetInFile);
            toReturn.Nick = nick;
            toReturn.SectionSize = bw.CurrentIndex - offsetInFile;
            toReturn.DataOffsetInSection += indexBeforeData - offsetInFile;
            toReturn.PostName = sectionPostName;

            if (suffixCurrent != null)
                toReturn.Suffix = suffixCurrent;

            return toReturn;

        }



        public static Dictionary<string, SectionProperties> Sections = new Dictionary<string, SectionProperties>()
        {
            { "GDocument", new SectionProperties() { HasSize = false} },
            { "GModel", new SectionProperties() { HasSize = false} },
            { "FShared", new SectionProperties() { HasSize = false} },
            { "CmObject", new SectionProperties() { HasSize = false} },
            { "CmString", new SectionProperties() { HasSize = true, Skip = true} },
            //{ "PArrangement", new SectionProperties() { HasSize = true} },
            { "MGroupEvent", new SectionProperties() { HasSize = false} },
            { "MPartEvent", new SectionProperties() { HasSize = false} },
            { "MEvent", new SectionProperties() { HasSize = false} },
            { "CmIDLink", new SectionProperties() { HasSize = false} },
            { "MDataNode", new SectionProperties() { HasSize = false} },
            { "MTrackEvent", new SectionProperties() { HasSize = false} },
            { "MPartNode", new SectionProperties() { HasSize = false} },
            { "MMidiEvent", new SectionProperties() { HasSize = false} },
            { "PMixerChannel", new SectionProperties() { HasSize = false} },
            { "PChannel", new SectionProperties() { HasSize = false} },
            { "PDeviceNode", new SectionProperties() { HasSize = false} },
            { "FNode", new SectionProperties() { HasSize = false} },
            { "PDevice", new SectionProperties() { HasSize = false} },
            { "PMultiInput", new SectionProperties() { HasSize = false} },
            { "PMultiOutput", new SectionProperties() { HasSize = false} },
            { "MAudioTrackEvent", new SectionProperties() { HasSize = true, Skip = true } },
            { "MMidiTrackEvent", new SectionProperties() { HasSize = true, Skip = true } },
            { "MDeviceTrackEvent", new SectionProperties() { HasSize = true, Skip = true } },
            { "FveD", new SectionProperties() { HasSize = false, Skip = true, FixedSize = 0x3F} },
            { "MTrackList" , new SectionProperties() { HasSize = true, Skip=true } },
            { "GTree" , new SectionProperties() { HasSize = true, Skip=true, AddToSize = 4 } },
            { "FNPath" , new SectionProperties() { HasSize = true, Skip=false } },
            { "FMemoryStream", new SectionProperties() { HasSize = true, Skip=true} },
            { "UWindowLayout", new SectionProperties() { HasSize = true, Skip=true} },
            { "PAppVersion", new SectionProperties() { HasSize = true, Skip=true} },
            { "PArrangeSetup", new SectionProperties() { HasSize = true, Skip=true, ShouldEatZeroz=true} },
            { "MTrack", new SectionProperties() { HasSize = true, Skip=false, ShouldEatZeroz=true} },
            { "MFolderTrack", new SectionProperties() { HasSize = true, Skip=true } },
//            { "PArrangement", new SectionProperties() { HasSize = true, Skip=true } },
            { "PAMD", new SectionProperties() { TwoBytesLength = 1, HasSize = false } },
            //{ "PDrumMap", new SectionProperties() { HasSize = true, Skip=true } },
            
            // Cubase 10
            //{ "FAttributes", new SectionProperties() { HasSize = true, Skip=true} },
            
            //{ "StMedia::PAttributes", new SectionProperties() { HasSize = true, Skip=true} },

        };
    }

    public class SectionProperties
    {
        //public string Name { get; set; }
        public bool HasSize { get; set; } = true;
        public int PrefixLength { get; set; } = 2;

        public bool Skip { get; set; } = false;

        public int FixedSize { get; set; } = 0;

        public int AddToSize { get; set; } = 0;

        public bool ShouldEatZeroz { get; set; } = false;

        public int TwoBytesLength { get; set; } = 2;

    }
}
