using Parse.DataItems;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parse
{
    public class CPR
    {
        private byte[] _data;

        private ByteWalker _walker;

        public readonly string sMTrackList = "MTrackList";

        public readonly string sMidiTrackStart = "MMidiTrackEvent";
        public readonly string sMidiPartStart = "MMidiPart";

        public byte[] Delim1 = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
        public byte[] Delim2 = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFE };

        public readonly string sAudioTrackStart = "MAudioTrackEvent";

        public readonly int offsetConst = 0x43;

        public CPR(byte[] _bytes)
        {
            _data = _bytes;
            _walker = new ByteWalker(_data);
        }


        public void ParseSections2()
        {

        }

        public void ParseSections(int mTrackListOffset)
        {
            List<DataItem> dis = new List<DataItem>();

            mTrackListOffset = new ByteWalker(_data).GetBytesUntil(true, Encoding.ASCII.GetBytes(sMTrackList)).Length - 4;

            // Get MTrackList
            var mTrackListDI = DataItem.CreateDataItem(mTrackListOffset, _data) as MTracklistDataItem;

            ByteWalker bw = new ByteWalker(mTrackListDI.Data);
            var s = bw.GetInt();
            mTrackListDI.UntitledString = bw.GetString(s);
            bw.GetBytes(2 + 4 + 8);
            mTrackListDI.NumberOfTracks = bw.GetInt();
            bw.GetBytesUntil(false, Chunk.Pad1, Chunk.Pad2);

            //mTrackListDI.HeaderEndOffset = mTrackListDI.offsetInFile + 4 + mTrackListDI.Name.Length + 1 + 2 + 4 + bw.CurrentIndex;
            //mTrackListDI.Data = new List<byte>(data).Skip(bw.CurrentIndex).ToArray();



            var mTrackEventDI = DataItem.CreateDataItem(mTrackListDI.HeaderEndOffset, _data);
            var mNextTrack = DataItem.CreateDataItem(mTrackEventDI.HeaderEndOffset + mTrackEventDI.SectionSize, _data);

            List<DataItem> Tracks = new List<DataItem>();

            while (mNextTrack.NextSectionOffset < _data.Length)
            {
                var testFF = new ByteWalker(_data);
                testFF.CurrentIndex = mNextTrack.NextSectionOffset;

                while (testFF.PeekIsDelimiter())
                {
                    mNextTrack.NextSectionOffset += 4;
                    testFF.GetInt();
                }

                Tracks.Add(mNextTrack);

                mNextTrack = DataItem.CreateDataItem(mNextTrack.NextSectionOffset, _data);                
            }

        }

        
        public string GetSectionNameFromOffset(int offset)
        {
            var bw = new ByteWalker(_data);
            bw.CurrentIndex = offset + offsetConst;
            var size = bw.GetInt();
            return bw.GetString(size);
        }

        public void ParseFuture()
        {
            // Midi Tracks
            _walker.GetBytesUntil(true, Encoding.ASCII.GetBytes(sMidiTrackStart));
            _walker.GetBytes(3);
            var midiSize = _walker.GetInt();

            var midiStartOffset = _walker.CurrentIndex;

        }
    }
}
