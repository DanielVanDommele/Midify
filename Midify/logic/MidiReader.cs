using Midify.enums;
using Midify.logic;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
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
                buffer = BinaryHelper.ReadNext(fs, 4, true);
                if (GetChunkIdentifier(buffer) == "MTrk")
                {
                    buffer = BinaryHelper.ReadNext(fs, 4);
                    int chunklen = (int)BitConverter.ToInt32(buffer, 0);
                    int remaining = chunklen;

                    MidiTrackStream mts = MidiTrackStream.FromFileStream(fs, remaining);
                    MidiTrack? track = ReadTrack(mts, mf, i);
                    if (track is not null)
                    {
                        mf.Tracks.AddTrack(track);
                    }
                    else
                    {
                        Console.WriteLine("Error reading track, aborting read");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Error: Invalid track chunk identifier, aborting read");
                    return null;
                }

            }
        }
        else
        {
            Console.WriteLine("Error: Invalid file, this file may not be a proper MIDI file");
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

    private MidiTrack? ReadTrack(MidiTrackStream mts, MidiFile mf, int trackIndex)
    {
        MidiTrack track = new();
        track.TrackNumber = trackIndex;

        long remaining = mts.Length;
        int posAbsolute = 0;

        // this one is set for each event and is checked by a scan on the first byte after 
        // reading the MidiEventPosition to see if the status actually is running or a new status byte is present
        int currentStatusByte = 0;

        while (remaining > 0)
        {
            // reading the delta time for the event
            MidiEventPosition pos = new();
            VariableLengthQuantity vlq = BinaryHelper.ReadVariableLengthQuantity(mts);
            posAbsolute += vlq.Value;
            pos.Delta = vlq.Value;
            pos.Absolute = posAbsolute;
            remaining -= vlq.ByteCount;

            IEventBase mEvent;

            int scannedByte = mts.ScanByte();
            if (scannedByte >= 0x80)
            {
                // there is a new status byte, so we read it.
                // a potential running status is cancelled by a new status byte
                int eventTypeAndChannelValue = mts.ReadByte();
                remaining--;

                currentStatusByte = eventTypeAndChannelValue;
            }

            byte eventTypeValue = (byte)(currentStatusByte & 0xF0);

            // not used in SysEx or Meta events
            // the plus 1 is because channels are 1-16, not 0-15
            int channel = (currentStatusByte & 0x0F) + 1; 

            switch (eventTypeValue)
            {
                case 0x80:
                case 0x90:
                case 0xA0:
                    MidiNote note = (MidiNote)mts.ReadByte();
                    remaining--;
                    int event2ndValue = mts.ReadByte();
                    remaining--;
                    if (eventTypeValue == 0xA0)
                    {
                        mEvent = new PolyphonicPressureEvent(pos, note, event2ndValue, channel);
                    }
                    else if (eventTypeValue == 0x90)
                    {
                        mEvent = new NoteOnEvent(pos, note, event2ndValue, channel);
                    }
                    else if (eventTypeValue == 0x80)
                    {
                        mEvent = new NoteOffEvent(pos, note, event2ndValue, channel);
                    }
                    else
                    {
                        Console.WriteLine("Error: unknown MIDI event where note event was expected");
                        return null;
                    }
                    track.Events.AddEvent(mEvent);
                    break;
                case 0xB0:
                    MidiController controller = (MidiController)mts.ReadByte();
                    remaining--;
                    int value = mts.ReadByte();
                    remaining--;
                    mEvent = new ControllerEvent(pos, controller, value, channel);
                    track.Events.AddEvent(mEvent);
                    break;
                case 0xC0:
                    MidiInstrument instr = (MidiInstrument)mts.ReadByte();
                    remaining--;
                    mEvent = new ProgramChangeEvent(pos, instr, channel);
                    if (track.Instrument == MidiInstrument.NotSet)
                    {
                        track.Instrument = instr;
                    }
                    track.Events.AddEvent(mEvent);
                    break;
                case 0xD0:
                    int pressure = mts.ReadByte();
                    remaining--;
                    mEvent = new ChannelPressureEvent(pos, pressure, channel);
                    track.Events.AddEvent(mEvent);
                    break;
                case 0xE0:
                    int bendValue = mts.ReadByte();
                    remaining--;
                    mEvent = new PitchBendEvent(pos, bendValue, channel);
                    track.Events.AddEvent(mEvent);
                    break;
                case 0xF0:
                    if (currentStatusByte == 0xFF)
                    {
                        MidiMetaType metaType = (MidiMetaType)mts.ReadByte();
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
                                VariableLengthQuantity vlqText = BinaryHelper.ReadVariableLengthQuantity(mts);
                                remaining -= vlqText.ByteCount;
                                byte[] textBytes = BinaryHelper.ReadNext(mts, vlqText.Value, true);
                                remaining -= vlqText.Value;
                                string text = System.Text.Encoding.ASCII.GetString(textBytes);
                                mEvent = new TextEvent(pos, vlq.Value, text, metaType);
                                track.Events.AddEvent(mEvent);

                                if (metaType == MidiMetaType.SequenceOrTrackName && track.TrackName == "")
                                {
                                    track.TrackName = text;
                                }
                                break;
                            case MidiMetaType.ChannelPrefix:
                                seeminglyRandomByte = (byte)mts.ReadByte();
                                remaining--;
                                if (seeminglyRandomByte != 0x01)
                                {
                                    Console.WriteLine($"Warning: byte {0x01} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent ChannelPrefix on track #{track.TrackNumber}");
                                }
                                int prefixChannel = mts.ReadByte();
                                remaining--;
                                mEvent = new ChannelPrefixEvent(pos, 0, prefixChannel);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.KeySignature:
                                seeminglyRandomByte = (byte)mts.ReadByte();
                                remaining--;
                                if (seeminglyRandomByte != 0x02)
                                {
                                    Console.WriteLine($"Warning: byte {0x02} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent KeySignature on track #{track.TrackNumber}");
                                }
                                KeySignature signature = (KeySignature)mts.ReadByte();
                                int majorOrMinor = mts.ReadByte();
                                remaining -= 2;
                                mEvent = new KeySignatureEvent(pos, majorOrMinor == 1 ? true : false, signature);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.TimeSignature:
                                seeminglyRandomByte = (byte)mts.ReadByte();
                                remaining--;
                                if (seeminglyRandomByte != 0x04)
                                {
                                    Console.WriteLine($"Warning: byte {0x04} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent TimeSignature on track #{track.TrackNumber}");
                                }
                                int numerator = mts.ReadByte();
                                int denominator = mts.ReadByte();
                                int clocks = mts.ReadByte();
                                int thirtyTwosPerQuaver = mts.ReadByte();
                                remaining -= 4;
                                mEvent = new TimeSignatureEvent(pos, numerator, denominator, clocks, thirtyTwosPerQuaver);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.SequenceSpecific:
                                // FF 7F Length data
                                VariableLengthQuantity vlqSequenceSpecific = BinaryHelper.ReadVariableLengthQuantity(mts);
                                remaining -= vlqSequenceSpecific.ByteCount;
                                byte[] dataBytes = BinaryHelper.ReadNext(mts, vlqSequenceSpecific.Value, true);
                                remaining -= vlqSequenceSpecific.Value;
                                mEvent = new SequencerSpecificEvent(pos, vlqSequenceSpecific.Value, dataBytes);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.EndOfTrack:
                                int endOfTrackByte = mts.ReadByte();
                                remaining--;
                                if (endOfTrackByte != 0x00)
                                {
                                    Console.WriteLine($"Error: The end of track byte must be {0x00} but {endOfTrackByte} was read for MetaEvent EndOfTrack on track #{track.TrackNumber}. The midi file is invalid.");
                                    return null;
                                }
                                if (remaining > 0)
                                {
                                    Console.WriteLine("Error: The end of track event is not the last event of this track. The midi file is invalid");
                                    return null;
                                }
                                mEvent = new EndOfTrackEvent(pos);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.Port:
                                // FF 21 01 pp
                                seeminglyRandomByte = (byte)mts.ReadByte();
                                remaining--;
                                if (seeminglyRandomByte != 0x01)
                                {
                                    Console.WriteLine($"Warning: byte {0x01} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent Port on track #{track.TrackNumber}");
                                }
                                int port = mts.ReadByte();
                                remaining--;
                                mEvent = new PortEvent(pos, port);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.SequenceNumber:
                                // FF 00 02 ss ss
                                seeminglyRandomByte = (byte)mts.ReadByte();
                                remaining--;
                                if (seeminglyRandomByte != 0x01)
                                {
                                    Console.WriteLine($"Warning: byte {0x02} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent SequenceNumber on track #{track.TrackNumber}");
                                }
                                byte[] sequenceNumberBytes = BinaryHelper.ReadNext(mts, 2);
                                int sequenceNumber = BitConverter.ToUInt16(sequenceNumberBytes, 0);
                                remaining -= 2;
                                mEvent = new SequenceNumberEvent(pos, sequenceNumber);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.SMTPEOffset:
                                // FF 54 05 hr mn se fr ff
                                seeminglyRandomByte = (byte)mts.ReadByte();
                                remaining--;
                                if (seeminglyRandomByte != 0x05)
                                {
                                    Console.WriteLine($"Warning: byte {0x05} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent SequenceNumber on track #{track.TrackNumber}");
                                }
                                byte frh = (byte)mts.ReadByte();
                                int minute = mts.ReadByte();
                                int second = mts.ReadByte();
                                int frame = mts.ReadByte();
                                int frameFraction = mts.ReadByte();
                                remaining -= 5;
                                int hour = frh & 0x1F;
                                int frameRate = frh & 0x60;
                                mEvent = new SMTPEOffsetEvent(pos, frameRate, hour, minute, second, frame, frameFraction);
                                track.Events.AddEvent(mEvent);
                                break;
                            case MidiMetaType.Tempo:
                                // FF 51 03 tt tt tt
                                seeminglyRandomByte = (byte)mts.ReadByte();
                                remaining--;
                                if (seeminglyRandomByte != 0x03)
                                {
                                    Console.WriteLine($"Warning: byte {0x03} was expected after metatype indicator, but received {seeminglyRandomByte} for MetaEvent SequenceNumber on track #{track.TrackNumber}");
                                }
                                byte[] tempoBytes = [0x00, .. BinaryHelper.ReadNext(mts, 3)];
                                remaining -= 3;
                                int tempo = BitConverter.ToInt32(tempoBytes, 0);
                                mEvent = new TempoEvent(pos, tempo);
                                track.Events.AddEvent(mEvent);
                                break;
                            default:
                                Console.WriteLine($"Error: Unknown Meta Event found ({metaType})");
                                return null;
                        }
                        break;
                    }
                    else if (currentStatusByte == 0xF0 || currentStatusByte == 0xF7)
                    {
                        // F0/F7 length data
                        VariableLengthQuantity vlqSysEx = BinaryHelper.ReadVariableLengthQuantity(mts);
                        remaining -= vlqSysEx.ByteCount;
                        byte[] dataBytes = BinaryHelper.ReadNext(mts, vlqSysEx.Value, true);
                        remaining -= vlqSysEx.Value;
                        mEvent = new SysExEvent(pos, vlqSysEx.Value, dataBytes);
                        track.Events.AddEvent(mEvent);
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Error: Unknown event type: {currentStatusByte:X2}, aborting read");
                        return null;
                    }
            }

            if (remaining < 0)
            {
                Console.WriteLine("Error: Bytes have been incorrectly read, because this line should never be allowed to be visible in normal circumstances.");
                return null;
            }
        }
        return track;
    }
}