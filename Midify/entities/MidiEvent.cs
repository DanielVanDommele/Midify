using Midify.enums;
using System.Diagnostics.Metrics;
using System.Dynamic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

public interface IEventBase
{
    public MidiEventType Type { get; }

    public MidiEventPosition Position { get; set; }

    public Dictionary<string, string> Parameters();
}

public interface IMidiEvent : IEventBase
{
  public int Channel { get; set; }
}

public interface INoteEvent : IMidiEvent
{
    public MidiNote Note { get; set; }
    public int Velocity { get; set; }
}

public interface IAfterTouchEvent : IMidiEvent
{
    public MidiNote? Note { get; set; }

    public int Pressure { get; set; }
}

public interface IControllerEvent : IMidiEvent
{
    public MidiController Controller { get; set; }
    public int Value { get; set; }
}

public interface IProgramChangeEvent : IMidiEvent
{
    public MidiInstrument Instrument { get; set; }
}

public interface IPitchBendEvent : IMidiEvent
{
    public int BendValue { get; set; }
}

public class NoteOffEvent : INoteEvent
{
    public NoteOffEvent(MidiEventPosition pos, MidiNote note, int velocity, int channel)
    {
        Position = pos;
        Note = note;
        Velocity = velocity;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote Note { get; set; } = MidiNote.CSC;
    public int Velocity { get; set; } = 64;

    public int Channel { get; set; } = 1;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "NoteOff");
        output.Add("Channel", Channel.ToString());
        output.Add("Note", Note.ToString());
        output.Add("Velocity", Velocity.ToString());
        return output;
    }
}

public class NoteOnEvent : INoteEvent
{
    public NoteOnEvent(MidiEventPosition pos, MidiNote note, int velocity, int channel)
    {
        Position = pos;
        Note = note;
        Velocity = velocity;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote Note { get; set; } = MidiNote.CSC;
    public int Velocity { get; set; } = 64;
    public int Channel { get; set; } = 1;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "NoteOn");
        output.Add("Channel", Channel.ToString());
        output.Add("Note", Note.ToString());
        output.Add("Velocity", Velocity.ToString());
        return output;
    }
}

public class PolyphonicPressureEvent : IAfterTouchEvent
{
    public PolyphonicPressureEvent(MidiEventPosition pos, MidiNote note, int pressure, int channel)
    {
        Position = pos;
        Note = note;
        Pressure = pressure;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote? Note { get; set; } = MidiNote.CSC;
    public int Pressure { get; set; } = 0;
    public int Channel { get; set; } = 1;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "PolyphnicPressure");
        output.Add("Channel", Channel.ToString());
        output.Add("Note", (Note ?? MidiNote.ASC).ToString());
        output.Add("Pressure", Pressure.ToString());
        return output;
    }
}

public class ChannelPressureEvent : IAfterTouchEvent
{
    public ChannelPressureEvent(MidiEventPosition pos, int pressure, int channel)
    {
        Position = pos;
        Pressure = pressure;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote? Note { get; set; } = null;
    public int Channel { get; set; } = 1;
    public int Pressure { get; set; } = 0;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "ChannelPressure");
        output.Add("Channel", Channel.ToString());
        output.Add("Pressure", Pressure.ToString());
        return output;
    }
}

public class ControllerEvent : IControllerEvent
{
    public ControllerEvent(MidiEventPosition pos, MidiController controller, int value, int channel)
    {
        Position = pos;
        Controller = controller;
        Value = value;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiController Controller { get; set; } = MidiController.BankSelect;
    public int Channel { get; set; } = 1;
    public int Value { get; set; } = 0;
    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Controller");
        output.Add("Channel", Channel.ToString());
        output.Add("Controller", Controller.ToString());
        output.Add("Value", Value.ToString());
        return output;
    }
}

public class ProgramChangeEvent : IProgramChangeEvent
{
    public ProgramChangeEvent (MidiEventPosition pos, MidiInstrument instrument, int channel)
    {
        Position = pos;
        Instrument = instrument;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
   
    public int Channel { get; set; }
    public MidiInstrument Instrument { get; set; }

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Program Change");
        output.Add("Channel", Channel.ToString());
        output.Add("Instrument", Instrument.ToString());
        return output;
    }
}

public class PitchBendEvent : IPitchBendEvent
{
    public PitchBendEvent(MidiEventPosition pos, int bendValue, int channel)
    {
        Position = pos;
        BendValue = bendValue;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public int Channel { get; set; } = 1;
    public int BendValue { get; set; } = 0;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Pitch Bend");
        output.Add("Channel", Channel.ToString());
        output.Add("BendValue", BendValue.ToString());
        return output;
    }
}

public interface ISysExEvent : IEventBase
{
    public int Length { get; set; }
    public byte[] Message { get; set; }
}

public class SysExEvent : ISysExEvent
{
    public SysExEvent(MidiEventPosition position, int length, byte[] message)
    {
        Position = position;
        Length = length;
        Message = message;
    }

    public MidiEventType Type { get; } = MidiEventType.SysExEvent;
    public MidiEventPosition Position { get; set; } = new();
    public int Length { get; set; } = 0;
    public byte[] Message { get; set; } = [];

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "SysEx Event");
        output.Add("Length", Length.ToString());
        output.Add("Message", String.Join(" ", Message.Select(b => b.ToString())));
        return output;
    }
}

public interface IMetaEvent : IEventBase
{
    public MidiMetaType MetaType { get; }
    public int ByteCount { get; set; }
}

//public class MetaEvent : IMetaEvent
//{
//    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
//    public MidiEventPosition Position { get; set; } = new();
//    public virtual MidiMetaType MetaType { get; } = MidiMetaType.Unspecified;
//    public int ByteCount { get; set; } = 0;
//}

public interface IMetaTextEvent : IMetaEvent
{
    public string Text { get;  set; }
}

public class SequenceNumberEvent : IMetaEvent
{
    public SequenceNumberEvent(MidiEventPosition pos, int sequenceNumber)
    {
        Position = pos;
        SequenceNumber = sequenceNumber;
    }
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiMetaType MetaType { get; } = MidiMetaType.SequenceNumber;
    public int ByteCount { get; set; } = 0;
    public int SequenceNumber { get; set; } = 0;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta Sequence Number");
        output.Add("Sequence Number", SequenceNumber.ToString());
        return output;
    }
}

public class TextEvent : IMetaTextEvent
{
    public TextEvent(MidiEventPosition pos, int count, string text, MidiMetaType metaType) 
    {
        Position = pos;
        ByteCount = count;
        Text = text;
        MetaType = metaType;
    }
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiMetaType MetaType { get; set; } = MidiMetaType.Text;
    public int ByteCount { get; set; } = 0;
    public string Text { get; set; } = "";

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", $"Meta {MetaType.ToString()}");
        output.Add("Text", Text);
        return output;
    }
}

public class ChannelPrefixEvent : IMetaEvent
{
    public ChannelPrefixEvent(MidiEventPosition pos, int count, int channel)
    {
        Position = pos;
        ByteCount = count;
        Channel = channel;
    }

    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.ChannelPrefix;
    public int ByteCount { get; set; } = 0;
    public int Channel { get; set; } = 1;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta Channel Prefix");
        output.Add("Channel", Channel.ToString());
        return output;
    }
}

public class PortEvent : IMetaEvent
{
    public PortEvent(MidiEventPosition pos, int port)
    {
        Position = pos;
        Port = port;
    }

    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.Port;
    public int ByteCount { get; set; } = 0;
    public int Port { get; set; } = 1;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta Port");
        output.Add("Port", Port.ToString());
        return output;
    }
}

public class EndOfTrackEvent : IMetaEvent
{
    public EndOfTrackEvent (MidiEventPosition pos)
    {
        Position = pos;
    }

    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.EndOfTrack;
    public int ByteCount { get; set; } = 0;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta End Of Track");
        return output;
    }
}
public class TempoEvent : IMetaEvent
{
    public TempoEvent (MidiEventPosition pos, long tempo)
    {
        Position = pos;
        Tempo = tempo;
    }

    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.Tempo;
    public int ByteCount { get; set; } = 0;
    public long Tempo { get; set; } = 1;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta Tempo");
        output.Add("Tempo", Tempo.ToString());
        return output;
    }
}
public class SMTPEOffsetEvent : IMetaEvent
{
    public SMTPEOffsetEvent(MidiEventPosition pos, int frameRate, int hour, int minute, int second, int frame, int fractionalFrame)
    {
        Position = pos;
        FrameRate = frameRate;
        Hour = hour;
        Minute = minute;
        Second = second;
        Frame = frame;
        FractionalFrame = fractionalFrame;
    }
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.SMTPEOffset;
    public int ByteCount { get; set; } = 0;
    public int FrameRate { get; set; } = 0;
    public int Hour { get; set; } = 0;
    public int Minute { get; set; } = 0;
    public int Second { get; set; } = 0;
    public int Frame { get; set; } = 0;
    public int FractionalFrame { get; set; } = 0;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta SMTPEOffset");
        output.Add("FrameRate", FrameRate.ToString());
        output.Add("Hour", Hour.ToString());
        output.Add("Minute", Minute.ToString());
        output.Add("Second", Second.ToString());
        output.Add("Frame", Frame.ToString());
        output.Add("FractionalFrame", FractionalFrame.ToString());

        return output;
    }
}
public class TimeSignatureEvent : IMetaEvent
{
    public TimeSignatureEvent(MidiEventPosition pos, int numerator, int denominator, int clocks, int thirtySeconds)
    {
        Position = pos;
        Numerator = numerator;
        Denominator = denominator;
        Clocks = clocks;
        ThirtySecondNotesPerQuaver = thirtySeconds;
    }

    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.TimeSignature;
    public int ByteCount { get; set; } = 0;
    public int Numerator { get; set; } = 4;
    public int Denominator { get; set; } = 2;
    public int Clocks { get; set; } = 0;
    public int ThirtySecondNotesPerQuaver { get; set; } = 8;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta Time Signature");
        output.Add("Numerator", Numerator.ToString());
        output.Add("Denominator", Denominator.ToString());
        output.Add("Clocks", Clocks.ToString());
        output.Add("32nd notes per quaver", ThirtySecondNotesPerQuaver.ToString());

        return output;
    }
}

public class KeySignatureEvent : IMetaEvent
{
    public KeySignatureEvent(MidiEventPosition pos, bool isMinor, KeySignature signature)
    {
        Position = pos;
        Key = signature;
        Minor = isMinor;
    }
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.KeySignature;
    public int ByteCount { get; set; } = 0;
    public KeySignature Key { get; set; } = KeySignature.C;
    public bool Minor { get; set; } = false;

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta Key Signature");
        output.Add("Key", Key.ToString());
        output.Add("Is Minor", Minor ? "Yes" : "No");

        return output;
    }
}

public class SequencerSpecificEvent : IMetaEvent
{
    public SequencerSpecificEvent(MidiEventPosition pos, int byteCount, byte[] data)
    {
        Position = pos;
        ByteCount = byteCount;
        Data = data;
    }

    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.SequenceSpecific;
    public int ByteCount { get; set; } = 0;
    public byte[] Data { get; set; } = [];

    public Dictionary<string, string> Parameters()
    {
        Dictionary<string, string> output = [];
        output.Add("EventType", "Meta Sequencer Specific");
        output.Add("Length", ByteCount.ToString());
        output.Add("Data", String.Join(" ", Data.Select(b => b.ToString())));
        return output;
    }
}