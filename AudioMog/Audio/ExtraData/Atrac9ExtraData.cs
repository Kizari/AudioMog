using System.IO;

namespace AudioMog.Core.Audio.ExtraData
{
    public class Atrac9ExtraData : ACodecExtraData
    {
        public byte Version { get; set; }
        public byte Reserved { get; set; }
        public ushort BlockAlign { get; set; }
        public ushort SamplesPerBlock { get; set; }
        public uint ChannelMask { get; set; }
        public byte[] Configuration { get; set; }
        public uint TotalSamples { get; set; }
        public uint OverlapDelay { get; set; }
        public uint EncoderDelay { get; set; }
        public uint SampleRate { get; set; }
        public uint LoopStart { get; set; }
        public uint LoopEnd { get; set; }
        public uint[] Reserved2 { get; set; }

        public Atrac9ExtraData(MaterialSection.MaterialEntry entry, BinaryReader reader)
        {
            reader.BaseStream.Seek(entry.ExtraDataOffset, SeekOrigin.Begin);
            Version = reader.ReadByte();
            Reserved = reader.ReadByte();
            Size = reader.ReadUInt16();
            BlockAlign = reader.ReadUInt16();
            SamplesPerBlock = reader.ReadUInt16();
            ChannelMask = reader.ReadUInt32();
            Configuration = reader.ReadBytes(4);
            TotalSamples = reader.ReadUInt32();
            OverlapDelay = reader.ReadUInt32();
            EncoderDelay = reader.ReadUInt32();
            SampleRate = reader.ReadUInt32();
            LoopStart = reader.ReadUInt32();
            LoopEnd = reader.ReadUInt32();
            Reserved2 = new[]
            {
                reader.ReadUInt32(),
                reader.ReadUInt32()
            };
        }
    }
}