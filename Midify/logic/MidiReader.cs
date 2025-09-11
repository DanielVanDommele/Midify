using Midify.enums;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

public class MidiReader
{
    private FileInfo _file;
    public MidiReader(FileInfo file)
    {
        _file = file;
    }

    public MidiFile? Read()
    {
        long numBytes = _file.Length;

        MidiFile mf = new(_file.FullName, numBytes);
        FileStream fs = _file.OpenRead();

        byte[] buffer = BinaryHelper.ReadNext(fs, 4, true);

        // look for the MThd MIDI header chunk identifier
        if (GetChunkIdentifier(buffer) == "MThd")
        {
            // valid (MIDI Header found)
            int trackCount = ReadHeader(fs, mf, buffer);
            if (trackCount == -1)
            {
                return null;
            }
            for (int i = 0; i < trackCount; i++)
            {
                bool valid = ReadTrack(fs, mf, buffer, i);
                if (!valid)
                {
                    Console.WriteLine("Invalid track found, aborting read");
                    return null;
                }
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

    private int ReadHeader(FileStream fs, MidiFile mf, byte[] buffer)
    {
        buffer = BinaryHelper.ReadNext(fs, 4);
        int chunklenHeader = (int)BitConverter.ToUInt32(buffer, 0);
        if (chunklenHeader == 6)
        {
            buffer = BinaryHelper.ReadNext(fs, 2);
            MidiFormat format = (MidiFormat)BitConverter.ToInt16(buffer, 0);
            mf.Header.Format = format;

            buffer = BinaryHelper.ReadNext(fs, 2);
            int numberOfTracks = BitConverter.ToInt16(buffer, 0);
            mf.Header.TrackCount = numberOfTracks;

            buffer = BinaryHelper.ReadNext(fs, 2);
            MidiTickDiv tickDiv = ToTickDiv(buffer);
            mf.Header.TickDiv = tickDiv;

            return numberOfTracks;
        }
        else
        {
            Console.WriteLine("Invalid header chunk length, aborting read");
            return -1;
        }
    }

    private MidiTickDiv ToTickDiv(byte[] bytes)
    {
        MidiTickDiv tickDiv = new();
        MidiTiming timing = (bytes[0] & (1 << 7)) == 1 ? MidiTiming.TimeCode : MidiTiming.Metric;

        if (timing == MidiTiming.TimeCode)
        {
            tickDiv.SetAsTimeCode(fps: -(sbyte)bytes[0], sf: bytes[1]);
        }
        else
        {
            tickDiv.SetAsMetric(ppqn: BitConverter.ToInt16(bytes));
            // tickDiv.FramesPerSecond = BitConverter.ToInt32([bytes[0]]);
            // tickDiv.SubFrames = (int)BitConverter.ToUInt32([bytes[1]]);
        }
        return tickDiv;
    }

    private bool ReadTrack(FileStream fs, MidiFile mf, byte[] buffer, int trackIndex)
    {
        buffer = BinaryHelper.ReadNext(fs, 4, true);
        if (GetChunkIdentifier(buffer) == "MTrk")
        {
            MidiTrack track = new();
            track.TrackNumber = trackIndex;

            int chunklen = (int)BitConverter.ToUInt32(BinaryHelper.ReadNext(fs, 4), 0);
            int remaining = chunklen;

            int posAbsolute = 0;

            while (remaining > 0)
            {
                // reading the delta time for the event
                MidiEventPosition pos = new();
                VariableLengthQuantity vlq = BinaryHelper.ReadVariableLengthQuantity(fs);
                posAbsolute += vlq.Value;
                pos.Delta = vlq.Value;
                pos.Absolute = posAbsolute;
                remaining -= vlq.ByteCount;

                IEventBase mEvent;

                // reading the event itself
                int eventTypeAndChannelValue = fs.ReadByte();
                byte eventTypeValue = (byte)(eventTypeAndChannelValue & 0xF0);
                int channel = eventTypeAndChannelValue & 0x0F; // not used in SysEx or Meta events
                remaining--;

                switch (eventTypeValue)
                {
                    case 0x80:
                    case 0x90:
                    case 0xA0:
                        MidiNote note = (MidiNote)fs.ReadByte();
                        remaining--;
                        int event2ndValue = fs.ReadByte();
                        remaining--;
                        if (eventTypeValue == 0xA0)
                        {
                            mEvent = new PolyphonicPressureEvent(pos, note, event2ndValue, channel);
                        }
                        else if (eventTypeValue == 0x90)
                        {
                            mEvent = new NoteOffEvent(pos, note, event2ndValue, channel);
                        } else
                        {
                            mEvent = new NoteOnEvent(pos, note, event2ndValue, channel);
                        }
                        track.Events.AddEvent(mEvent);
                        break;
                    case 0xB0:
                        MidiController controller = (MidiController)fs.ReadByte();
                        remaining--;
                        int value = fs.ReadByte();
                        remaining--;
                        mEvent = new ControllerEvent(pos, controller, value, channel);
                        track.Events.AddEvent(mEvent);
                        break;
                    case 0xC0:
                        MidiInstrument instr = (MidiInstrument)fs.ReadByte();
                        remaining--;
                        mEvent = new ProgramChangeEvent(pos, instr, channel);
                        if (track.Instrument == MidiInstrument.NotSet)
                        {
                            track.Instrument = instr;
                        }
                        track.Events.AddEvent(mEvent);
                        break;
                    case 0xD0:
                        int pressure = fs.ReadByte();
                        remaining--;
                        mEvent = new ChannelPressureEvent(pos, pressure, channel);
                        track.Events.AddEvent(mEvent);
                        break;
                    case 0xE0:
                        int bendValue = fs.ReadByte();
                        remaining--;
                        mEvent = new PitchBendEvent(pos, bendValue, channel);
                        track.Events.AddEvent(mEvent);
                        break;
                    case 0xF0:
                        if (eventTypeAndChannelValue == 0xFF)
                        {
                            MidiMetaType metaType = (MidiMetaType)fs.ReadByte();
                            remaining--;

                            byte seeminglyRandomByte;
                            switch (metaType)
                            {
                                case MidiMetaType.Text:
                                case MidiMetaType.Copyright:
                                case MidiMetaType.Lyric:
                                case MidiMetaType.Marker:
                                case MidiMetaType.CuePoint:
                                case MidiMetaType.DeviceName:
                                case MidiMetaType.InstrumentName:
                                case MidiMetaType.ProgramName:
                                case MidiMetaType.SequenceOrTrackName:
                                    VariableLengthQuantity vlqText = BinaryHelper.ReadVariableLengthQuantity(fs);
                                    remaining -= vlqText.ByteCount;
                                    byte[] textBytes = BinaryHelper.ReadNext(fs, vlqText.Value, true);
                                    remaining -= vlqText.Value;
                                    string text = System.Text.Encoding.ASCII.GetString(textBytes);
                                    if (metaType  == MidiMetaType.Text)
                                    {
                                        mEvent = new TextEvent(pos, vlq.Value, text, metaType);
                                        track.Events.AddEvent(mEvent);
                                    }
                                    if (metaType == MidiMetaType.SequenceOrTrackName && track.TrackName == "")
                                    {
                                        track.TrackName = text;
                                    }
                                    break;
                                case MidiMetaType.ChannelPrefix:
                                    seeminglyRandomByte = (byte)fs.ReadByte();
                                    remaining--;
                                    if (seeminglyRandomByte != 0x01)
                                    {
                                        Console.WriteLine($"Warning: byte {0x01} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent ChannelPrefix on track #{track.TrackNumber}");
                                    }
                                    int prefixChannel = fs.ReadByte();
                                    remaining--;
                                    mEvent = new ChannelPrefixEvent(pos, 0, prefixChannel);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.KeySignature:
                                    seeminglyRandomByte = (byte)fs.ReadByte();
                                    remaining--;
                                    if (seeminglyRandomByte != 0x02)
                                    {
                                        Console.WriteLine($"Warning: byte {0x02} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent KeySignature on track #{track.TrackNumber}");
                                    }
                                    KeySignature signature = (KeySignature)fs.ReadByte();
                                    int majorOrMinor = fs.ReadByte();
                                    remaining -= 2;
                                    mEvent = new KeySignatureEvent(pos, majorOrMinor == 1 ? true : false, signature);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.TimeSignature:
                                    seeminglyRandomByte = (byte)fs.ReadByte();
                                    remaining--;
                                    if (seeminglyRandomByte != 0x04)
                                    {
                                        Console.WriteLine($"Warning: byte {0x04} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent TimeSignature on track #{track.TrackNumber}");
                                    }
                                    int numerator = fs.ReadByte();
                                    int denominator = fs.ReadByte();
                                    int clocks = fs.ReadByte();
                                    int thirtyTwosPerQuaver = fs.ReadByte();
                                    remaining -= 4;
                                    mEvent = new TimeSignatureEvent(pos, numerator, denominator, clocks, thirtyTwosPerQuaver);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.SequenceSpecific:
                                    // FF 7F Length data
                                    VariableLengthQuantity vlqSequenceSpecific = BinaryHelper.ReadVariableLengthQuantity(fs);
                                    remaining -= vlqSequenceSpecific.ByteCount;
                                    byte[] dataBytes = BinaryHelper.ReadNext(fs, vlqSequenceSpecific.Value, true);
                                    remaining -= vlqSequenceSpecific.Value;
                                    mEvent = new SequencerSpecificEvent(pos, vlqSequenceSpecific.Value, dataBytes);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.EndOfTrack:
                                    int endOfTrackByte = fs.ReadByte();
                                    remaining--;
                                    if (endOfTrackByte != 0x00)
                                    {
                                        Console.WriteLine($"Error: The end of track byte must be {0x00} but {endOfTrackByte} was read for MetaEvent EndOfTrack on track #{track.TrackNumber}. The midi file is invalid.");
                                        return false;
                                    }
                                    if (remaining > 0)
                                    {
                                        Console.WriteLine("Error: The end of track event is not the last event of this track. The midi file is invalid");
                                        return false;
                                    }
                                    mEvent = new EndOfTrackEvent(pos);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.Port:
                                    // FF 21 01 pp
                                    seeminglyRandomByte = (byte)fs.ReadByte();
                                    remaining--;
                                    if (seeminglyRandomByte != 0x01)
                                    {
                                        Console.WriteLine($"Warning: byte {0x01} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent Port on track #{track.TrackNumber}");
                                    }
                                    int port = fs.ReadByte();
                                    remaining--;
                                    mEvent = new PortEvent(pos, port);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.SequenceNumber:
                                    // FF 00 02 ss ss
                                    seeminglyRandomByte = (byte)fs.ReadByte();
                                    remaining--;
                                    if (seeminglyRandomByte != 0x01)
                                    {
                                        Console.WriteLine($"Warning: byte {0x02} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent SequenceNumber on track #{track.TrackNumber}");
                                    }
                                    byte[] sequenceNumberBytes = BinaryHelper.ReadNext(fs, 2);
                                    int sequenceNumber = BitConverter.ToUInt16(sequenceNumberBytes, 0);
                                    remaining -= 2;
                                    mEvent = new SequenceNumberEvent(pos, sequenceNumber);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.SMTPEOffset:
                                    // FF 54 05 hr mn se fr ff
                                    seeminglyRandomByte = (byte)fs.ReadByte();
                                    remaining--;
                                    if (seeminglyRandomByte != 0x05)
                                    {
                                        Console.WriteLine($"Warning: byte {0x05} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent SequenceNumber on track #{track.TrackNumber}");
                                    }
                                    byte frh = (byte)fs.ReadByte();
                                    int minute = fs.ReadByte();
                                    int second = fs.ReadByte();
                                    int frame = fs.ReadByte();
                                    int frameFraction = fs.ReadByte();
                                    remaining -= 5;
                                    int hour = frh & 0x1F;
                                    int frameRate = frh & 0x60;
                                    mEvent = new SMTPEOffsetEvent(pos, frameRate, hour, minute, second, frame, frameFraction);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                case MidiMetaType.Tempo:
                                    // FF 51 03 tt tt tt
                                    seeminglyRandomByte = (byte)fs.ReadByte();
                                    remaining--;
                                    if (seeminglyRandomByte != 0x03)
                                    {
                                        Console.WriteLine($"Warning: byte {0x03} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent SequenceNumber on track #{track.TrackNumber}");
                                    }
                                    byte[] tempoBytes = [0x00, ..BinaryHelper.ReadNext(fs, 3)];
                                    remaining -= 3;
                                    int tempo = BitConverter.ToInt32(tempoBytes, 0);
                                    mEvent = new TempoEvent(pos, tempo);
                                    track.Events.AddEvent(mEvent);
                                    break;
                                default:
                                    Console.WriteLine($"Unknown Meta Event found ({metaType})");
                                    break;
                            }
                            break;
                        }
                        else if (eventTypeAndChannelValue == 0xF0 || eventTypeAndChannelValue == 0xF7)
                        {
                            // F0/F7 length data
                            VariableLengthQuantity vlqSysEx = BinaryHelper.ReadVariableLengthQuantity(fs);
                            remaining -= vlqSysEx.ByteCount;
                            byte[] dataBytes = BinaryHelper.ReadNext(fs, vlqSysEx.Value, true);
                            remaining -= vlqSysEx.Value;
                            mEvent = new SysExEvent(pos, vlqSysEx.Value, dataBytes);
                            track.Events.AddEvent(mEvent);
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Unknown event type: {eventTypeAndChannelValue:X2}, aborting read");
                            return false;
                        }
                }
            }
            mf.Tracks.AddTrack(track);
            return true;
        }
        else
        { 
            Console.WriteLine("Invalid track chunk identifier, aborting read");
            return false;
        }
    }
}