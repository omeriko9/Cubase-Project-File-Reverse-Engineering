﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.DataItems
{
    internal class MTracklistDataItem : DataItem
    {
        public string UntitledString { get; set; }
        public int NumberOfTracks { get; set; }
        public MTracklistDataItem(string name, byte[] data, int offsetInFile, byte[] entireData) : base(name, data, offsetInFile, entireData)
        {

            //ByteWalker bw = new ByteWalker(Data);           
            //var s = bw.GetInt();
            //UntitledString = bw.GetString(s);
            //bw.GetBytes(2 + 4 + 8);
            //NumberOfTracks = bw.GetInt();
            //bw.GetBytesUntil(false, Chunk.Pad1, Chunk.Pad2);
            
            //HeaderEndOffset = offsetInFile + 4 + Name.Length + 1 + 2 + 4 + bw.CurrentIndex;
            //Data = new List<byte>(data).Skip(bw.CurrentIndex).ToArray();


        }
    }
}