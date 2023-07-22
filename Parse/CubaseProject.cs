using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public class CubaseProject
    {
        public int NumOfTracks { get; set; }
        public byte[] bNumOfTracksMagic = new byte[] { 0x01, 0x3F, 0xF0 };
        public readonly string strMTrackList = "MTrackList";
        public int Cubase2BytesTrackAdd = 12;
        public int Cubase5BytesTrackAdd = 16;

        public byte[] bTrackStartMagic = Encoding.ASCII.GetBytes("VST Multitrack");
        public byte[] bTrackMagicVer5Above = new byte[] { 0xFF, 0xFE, 0xA4, 0xC8 };

        public List<CubaseAudioTrack> Tracks { get; set; }  = new List<CubaseAudioTrack>();

        public CubaseProjectFile cpf { get; set; }

        public CubaseProject(CubaseProjectFile pCPF)
        {
            cpf = pCPF;
        }

        public void Parse()
        {
            var mTrackList = cpf.GetDataItem(strMTrackList);
            ByteWalker bw = new ByteWalker(mTrackList.Data);
            var b1 = bw.GetBytesUntil(true, bNumOfTracksMagic);
            var b2 = bw.GetInts(3);
            NumOfTracks = bw.GetInt();

            if (NumOfTracks > 0)
            {
                ByteWalker bwTracks = new ByteWalker(cpf.Bytes);
                var isCubase2b = new ByteWalker(cpf.Bytes).GetBytesUntil(false, bTrackMagicVer5Above);
                var isCubase2 = isCubase2b.Length == cpf.Bytes.Length - 1;                

                for (int i = 0; i < NumOfTracks; i++)
                {
                    bwTracks.GetBytesUntil(false, bTrackStartMagic);
                    var offset = bwTracks.CurrentIndex;

                    if (isCubase2)
                    {
                        offset += Cubase2BytesTrackAdd;
                    }
                    else
                    {
                        offset += bwTracks.GetBytesUntil(false, Encoding.ASCII.GetBytes("String")).Count() + 6 ;
                    }
                    bwTracks.CurrentIndex = offset;
                    var trackNameSize = (int)bwTracks.GetBytes(1).First();
                    var trackName = bwTracks.GetString(trackNameSize);
                    CubaseAudioTrack track = isCubase2 ? (CubaseAudioTrack) new Cubase2AudioTrack(bwTracks) : new CUbase5AudioTrack(bwTracks);
                    track.Name = trackName;
                    Tracks.Add(track);
                }
            }
        }
    }

    public abstract class CubaseAudioTrack {
        //public int Size { get; set; }
        public string Name { get; set; }

        private ByteWalker bw;

        public CubaseAudioTrack(ByteWalker pBW)
        {
            bw = pBW;
        }

        public List<Effect> Effects { get; set;  }

        public abstract void Parse();
    
    }

    public class Cubase2AudioTrack : CubaseAudioTrack {

        public Cubase2AudioTrack(ByteWalker pBW) : base(pBW) { }

        public override void Parse()
        {
            
        }
    }
    public class CUbase5AudioTrack : CubaseAudioTrack {

        public CUbase5AudioTrack(ByteWalker pBW) : base(pBW) { }

        public override void Parse()
        {
            throw new NotImplementedException();
        }
    }

    public class Effect { }

}
