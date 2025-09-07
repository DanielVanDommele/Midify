using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices;

public class MidiReader
{
    private FileInfo _file;
    public MidiReader(FileInfo file)
    {
        _file = file;
    }

    public MidiFile Read()
    {
        long numBytes = _file.Length;
        int byteIdx = 0;

        MidiFile mf = new(_file.FullName, numBytes);
        FileStream fs = _file.OpenRead();

        byte[] buffer = BinaryHelper.ReadNext(fs, byteIdx, 4);

        // look for the MThd MIDI header chunk identifier
        if (GetChunkIdentifier(buffer) == "MThd")
        {
            // valid (MIDI Header found)
            int trackCount = ReadHeader(fs, mf, buffer, byteIdx);
            for (int i = 0; i < trackCount; i++)
            {
                ReadTrack(fs, mf, buffer, byteIdx);
            }
        }
        else
        {
            Console.WriteLine("Invalid file, this file may not be a proper MIDI file");
        }

        fs.Close();
        return mf;
    }

    private string GetChunkIdentifier(byte[] bytes)
    {
        string result = "";
        foreach (var b in bytes)
        {
            result += ((char)b);
        }
        return result;
    }

    private int ReadHeader(FileStream fs, MidiFile mf, byte[] buffer, int byteIdx)
    {
        buffer = BinaryHelper.ReadNext(fs, byteIdx, 4);
        int chunklenHeader = BitConverter.ToInt32(buffer, 0);
        if (chunklenHeader == 6)
        {
            buffer = BinaryHelper.ReadNext(fs, byteIdx, 2);
            MidiFormat format = (MidiFormat)BitConverter.ToInt32(buffer, 0);
            mf.Header.Format = format;

            buffer = BinaryHelper.ReadNext(fs, byteIdx, 2);
            int numberOfTracks = BitConverter.ToInt32(buffer, 0);
            mf.Header.TrackCount = numberOfTracks;

            buffer = BinaryHelper.ReadNext(fs, byteIdx, 2);
            MidiTickDiv tickDiv = ToTickDiv(buffer);
            mf.Header.TickDiv = tickDiv;

            return numberOfTracks;
        }
        return 0;
    }

    private MidiTickDiv ToTickDiv(byte[] bytes)
    {
        MidiTickDiv tickDiv = new();
        tickDiv.Timing = (bytes[0] & (1 << 7)) == 1 ? MidiTiming.TimeCode : MidiTiming.Metric;
        if (tickDiv.Timing == MidiTiming.TimeCode)
        {
            tickDiv.PulsePerQuarterNote = BitConverter.ToInt32(bytes);
        }
        else
        {
            tickDiv.FramesPerSecond = BitConverter.ToInt32([bytes[0]]);
            tickDiv.SubFrames = (int)BitConverter.ToUInt32([bytes[1]]);
        }
        return tickDiv;
    }

    private void ReadTrack(FileStream fs, MidiFile mf, byte[] buffer, int byteIdx)
    {
        
    }
}