using System;
using System.IO;
using System.Text;
using AudioMog.Application.AudioFileRebuilder;
using AudioMog.Core;
using AudioMog.Core.Audio;
using AudioMog.Core.Audio.ExtraData;
using VGAudio.Containers.At9;
using VGAudio.Containers.Hca;
using VGAudio.Containers.Wave;
using VGAudio.Formats;

namespace AudioMog.Application.Codecs
{
    public class Atrac9Codec : ACodec
    {
        private static readonly byte[] HcaEncryptionKey = new byte[0x100]
        {
            0x3A, 0x32, 0x32, 0x32, 0x03, 0x7E, 0x12, 0xF7, 0xB2, 0xE2, 0xA2, 0x67, 0x32, 0x32, 0x22, 0x32, // 00-0F
            0x32, 0x52, 0x16, 0x1B, 0x3C, 0xA1, 0x54, 0x7B, 0x1B, 0x97, 0xA6, 0x93, 0x1A, 0x4B, 0xAA, 0xA6, // 10-1F
            0x7A, 0x7B, 0x1B, 0x97, 0xA6, 0xF7, 0x02, 0xBB, 0xAA, 0xA6, 0xBB, 0xF7, 0x2A, 0x51, 0xBE, 0x03, // 20-2F
            0xF4, 0x2A, 0x51, 0xBE, 0x03, 0xF4, 0x2A, 0x51, 0xBE, 0x12, 0x06, 0x56, 0x27, 0x32, 0x32, 0x36, // 30-3F
            0x32, 0xB2, 0x1A, 0x3B, 0xBC, 0x91, 0xD4, 0x7B, 0x58, 0xFC, 0x0B, 0x55, 0x2A, 0x15, 0xBC, 0x40, // 40-4F
            0x92, 0x0B, 0x5B, 0x7C, 0x0A, 0x95, 0x12, 0x35, 0xB8, 0x63, 0xD2, 0x0B, 0x3B, 0xF0, 0xC7, 0x14, // 50-5F
            0x51, 0x5C, 0x94, 0x86, 0x94, 0x59, 0x5C, 0xFC, 0x1B, 0x17, 0x3A, 0x3F, 0x6B, 0x37, 0x32, 0x32, // 60-6F
            0x30, 0x32, 0x72, 0x7A, 0x13, 0xB7, 0x26, 0x60, 0x7A, 0x13, 0xB7, 0x26, 0x50, 0xBA, 0x13, 0xB4, // 70-7F
            0x2A, 0x50, 0xBA, 0x13, 0xB5, 0x2E, 0x40, 0xFA, 0x13, 0x95, 0xAE, 0x40, 0x38, 0x18, 0x9A, 0x92, // 80-8F
            0xB0, 0x38, 0x00, 0xFA, 0x12, 0xB1, 0x7E, 0x00, 0xDB, 0x96, 0xA1, 0x7C, 0x08, 0xDB, 0x9A, 0x91, // 90-9F
            0xBC, 0x08, 0xD8, 0x1A, 0x86, 0xE2, 0x70, 0x39, 0x1F, 0x86, 0xE0, 0x78, 0x7E, 0x03, 0xE7, 0x64, // A0-AF
            0x51, 0x9C, 0x8F, 0x34, 0x6F, 0x4E, 0x41, 0xFC, 0x0B, 0xD5, 0xAE, 0x41, 0xFC, 0x0B, 0xD5, 0xAE, // B0-BF
            0x41, 0xFC, 0x3B, 0x70, 0x71, 0x64, 0x33, 0x32, 0x12, 0x32, 0x32, 0x36, 0x70, 0x34, 0x2B, 0x56, // C0-CF
            0x22, 0x70, 0x3A, 0x13, 0xB7, 0x26, 0x60, 0xBA, 0x1B, 0x94, 0xAA, 0x40, 0x38, 0x00, 0xFA, 0xB2, // D0-DF
            0xE2, 0xA2, 0x67, 0x32, 0x32, 0x12, 0x32, 0xB2, 0x32, 0x32, 0x32, 0x32, 0x75, 0xA3, 0x26, 0x7B, // E0-EF
            0x83, 0x26, 0xF9, 0x83, 0x2E, 0xFF, 0xE3, 0x16, 0x7D, 0xC0, 0x1E, 0x63, 0x21, 0x07, 0xE3, 0x01, // F0-FF
        };
        
        public override string FileFormat => ".at9";
        
        public override void ExtractOriginal(IApplicationLogger logger, MaterialSection.MaterialEntry entry, byte[] fullFileBytes, string outputPath)
        {
            var rawContentBytes = fullFileBytes.SubArray(entry.InnerStreamStartPosition, entry.InnerStreamSize);
            var rawPath = ExtensionMethods.ChangeExtension(outputPath, FileFormat);
            File.WriteAllBytes(rawPath, At9ToRiff(entry, rawContentBytes));
            logger.Log($"Created at9 audio track at: {rawPath}");
        }

        public override void ExtractAsWav(IApplicationLogger logger, MaterialSection.MaterialEntry entry, byte[] fullFileBytes, string wavPath)
        {
	        var rawContentBytes = fullFileBytes.SubArray(entry.InnerStreamStartPosition, entry.InnerStreamSize - 48);
	        var riff = At9ToRiff(entry, rawContentBytes);
	        File.WriteAllBytes(@"C:\Modding\Chocomog\Testing\At9\test.at9", riff);

	        //riff = File.ReadAllBytes(@"C:\Users\Kieran\Downloads\b.wav");
	        AudioData audioData = null;
	        var reader = new At9Reader();
	        audioData = reader.Read(riff);
	        var writer = new WaveWriter();
	        var wavBytes = writer.GetFile(audioData);

	        File.WriteAllBytes(wavPath, wavBytes);
	        logger.Log($"Created wav audio track at: {wavPath}");
        }

        public override void FixHeader(TemporaryTrack track)
        {
            ExposedHcaReader reader = new ExposedHcaReader();
			var data = reader.GetStructure(track.RawPortion);


			if (track.OriginalEntry.Codec != track.CurrentCodec)
			{
				var tempMaterialData = track.HeaderPortion.SubArray(0, 32);
				WriteByte(tempMaterialData, 0x05, (byte)MaterialCodecType.HCA);
				
				var tempHCAData = new byte[16];
				WriteByte(tempHCAData,  0x00, 1);
				WriteByte(tempHCAData,  0x01, 16); //extraDataSize
				WriteUshort(tempHCAData,  0x02, (ushort)data.HeaderSize);
				WriteUshort(tempHCAData,  0x04, (ushort)data.Hca.FrameSize);
				WriteUshort(tempHCAData,  0x06, (ushort)data.Hca.LoopStartFrame);
				WriteUshort(tempHCAData,  0x08, (ushort)data.Hca.LoopEndFrame);
				WriteUshort(tempHCAData,  0x0a, (ushort)data.Hca.InsertedSamples);
				WriteByte(tempHCAData, 0x0c, 0); //remove mixer flag!
				WriteByte(tempHCAData, 0x0d, 0); //remove encryption flag!
				WriteUshort(tempHCAData, 0x0e, 0); //clean reserves!

				var tempMerged = new byte[48];
				tempMaterialData.CopyTo(tempMerged,0);
				tempHCAData.CopyTo(tempMerged, 32);
				
				track.HeaderPortion = tempMerged;
			}
			
			
			int loopStart = 0;
			int loopEnd = 0;
			if (data.Hca.Looping)
			{
				loopStart = data.Hca.LoopStartSample;
				loopEnd = data.Hca.LoopEndSample;
			}
			
			var hcaHeaderSize = data.HeaderSize;
			var newExtraDataSize = 16 + hcaHeaderSize;
			var streamSize = track.RawPortion.Length - hcaHeaderSize;
			
			var headerBytes = track.HeaderPortion;
			WriteByte(headerBytes, 0x04, (byte)data.Hca.ChannelCount);
			WriteUint(headerBytes, 0x08, (uint)data.Hca.SampleRate);
			WriteUint(headerBytes, 0x0c, (uint)loopStart);
			WriteUint(headerBytes, 0x10, (uint)loopEnd);
			WriteUint(headerBytes, 0x14, (uint)newExtraDataSize);
			WriteUint(headerBytes, 0x18, (uint)streamSize);

			var extraData = 0x20;
			WriteByte(headerBytes, extraData + 0x00, 1);
			//WriteByte(headerBytes, extraData + 0x01, 16); //extraDataSize
			WriteUshort(headerBytes, extraData + 0x02, (ushort)data.HeaderSize);
			WriteUshort(headerBytes, extraData + 0x04, (ushort)data.Hca.FrameSize);
			WriteUshort(headerBytes, extraData + 0x06, (ushort)data.Hca.LoopStartFrame);
			WriteUshort(headerBytes, extraData + 0x08, (ushort)data.Hca.LoopEndFrame);
			WriteUshort(headerBytes, extraData + 0x0a, (ushort)data.Hca.InsertedSamples);
			// 0x0c: "use mixer" flag
			WriteByte(headerBytes, extraData + 0x0d, 0); //remove encryption flag!
			// 0x0e: reserved x2 
        }

        private byte[] At9ToRiff(MaterialSection.MaterialEntry entry, byte[] rawContentBytes)
        {
	        var extraData = (Atrac9ExtraData)entry.ExtraDataObject;
	        
	        using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Encoding.UTF8.GetBytes("RIFF"));
                    writer.Write(entry.StreamSize - 4); // File size?
                    writer.Write(Encoding.UTF8.GetBytes("WAVE"));
                    
                    // Write the format chunk
                    writer.Write(Encoding.UTF8.GetBytes("fmt "));
                    writer.Write(52);               // Chunk size
                    writer.Write((ushort)0xFFFE);   // Format code
                    writer.Write((ushort)entry.ChannelCount);
                    writer.Write(extraData.SampleRate);
                    writer.Write(52500);
                    writer.Write(extraData.BlockAlign);
                    writer.Write((ushort)0);        // Bits per sample (always zero)
                    writer.Write((ushort)34);       // Extension size (unsure if 34 is correct)
                    writer.Write(extraData.SamplesPerBlock);
                    writer.Write(extraData.ChannelMask);
                    var guid = Guid.Parse("47E142D2-36BA-4d8d-88FC-61654F8C836C");
                    writer.Write(guid.ToByteArray());
                    writer.Write(1);                // Atrac9 version
                    Array.Reverse(extraData.Configuration);
                    writer.Write(extraData.Configuration);
                    writer.Write(0);                // Reserved
                    
                    // Write the fact chunk
                    writer.Write(Encoding.UTF8.GetBytes("fact"));
                    writer.Write(12);                       // Chunk size
                    writer.Write(extraData.TotalSamples);
                    writer.Write(extraData.EncoderDelay);
                    writer.Write(extraData.EncoderDelay);   // This is meant to be a repeat
                    
                    // Write the sample chunk
                    writer.Write(Encoding.UTF8.GetBytes("smpl"));
                    writer.Write(60);   // Chunk size
                    writer.Write(0);    // Manufacturer (0 = not specified)
                    writer.Write(0);    // Product (0 = not specified)
                    writer.Write((uint)(1.0 / extraData.SampleRate * 1000000000));  // Sample period

                    if (extraData.LoopEnd <= 0)
                    {
	                    writer.Write(60);
	                    writer.Write(0);
	                    writer.Write(0);
	                    writer.Write(0);
                    }
                    
                    writer.Write(1);  // MIDI unity note
                    writer.Write(24); // MIDI pitch fraction
                    writer.Write(0);  // SMPTE format
                    writer.Write(0);  // SMPTE offset
                    //writer.Write(0);  //writer.Write(256);  // Number of sample loops
                    //writer.Write(0);  //writer.Write(extraData.TotalSamples + 255); // Sample data size
                    writer.Write(extraData.LoopEnd > 0 ? 1 : 0);
                    writer.Write(0);	// VGAudio just reads past this so guess the value doesn't matter?
                    //writer.Write(0);
                    //writer.Write(0);
                    if (extraData.LoopEnd > 0)
                    {
	                    writer.Write(0);	// Loop ID
	                    writer.Write(0);	// Loop type (0 = forward looping type)
	                    writer.Write(extraData.LoopStart);
	                    writer.Write(extraData.LoopEnd);
	                    writer.Write(0);	// Loop fraction (0 = current resolution)
	                    writer.Write(0);	// Loop count (0 = infinite)
                    }
                    else
                    {
	                    writer.Write(0);
	                    writer.Write(0);
                    }

                    // Write the data chunk
                    writer.Write(Encoding.UTF8.GetBytes("data"));
                    writer.Write(rawContentBytes.Length);
                    writer.Write(rawContentBytes);

                    var result = stream.ToArray();
                    File.WriteAllBytes(@"C:\Code\AudioMog\AudioMogTerminal\bin\Debug\walla_loop.at9", result);
                    return result;
                }
            }
        }
    }
}