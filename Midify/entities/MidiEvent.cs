using System.Dynamic;
using System.IO.Pipes;

public interface IMidiEvent
{
    public MidiEventType Type { get; }

    public MidiEventPosition Position { get; set; }
}

// public class MidiEvent
// {
//     public MidiEventType Type { get; set; } = MidiEventType.Unspecified;

//     public MidiEventPosition Position { get; set; } = new();
// }

public interface INoteEvent : IMidiEvent
{
    public MidiNote Note { get; set; }
    public int Velocity { get; set; }

    public int Channel { get; set; }
}

public interface IAfterTouchEvent : IMidiEvent
{
    public MidiNote? Note { get; set; }

    public int Pressure { get; set; }

    public int Channel { get; set; }
}

public interface IControllerEvent : IMidiEvent
{
    public int Controller { get; set; }
    public int Value { get; set; }
    public int Channel { get; set; }
}

public interface IProgramChangeEvent : IMidiEvent
{
    public int Channel { get; set; }
    public MidiInstrument Instrument { get; set; }
}

public interface IPitchBendEvent : IMidiEvent
{
    public int Channel { get; set; }
    public int BendValue { get; set; }
}

public class NoteOffEvent : INoteEvent
{
    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote Note { get; set; } = MidiNote.Off;
    public int Velocity { get; set; } = 64;

    public int Channel { get; set; } = 1;
}

public class NoteOnEvent : INoteEvent
{
    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote Note { get; set; } = MidiNote.Off;
    public int Velocity { get; set; } = 64;
    public int Channel { get; set; } = 1;
}


public class PolyphonicPressureEvent : IAfterTouchEvent
{
    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote? Note { get; set; } = MidiNote.Off;
    public int Pressure { get; set; } = 0;
    public int Channel { get; set; } = 1;
}

public class ChannelPressureEvent : IAfterTouchEvent
{
    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiNote? Note { get; set; } = null;
    public int Channel { get; set; } = 1;
    public int Pressure { get; set; } = 0;
}

public class ControllerEvent : IControllerEvent
{
    public MidiEventType Type { get; } = MidiEventType.MidiEvent;
    public MidiEventPosition Position { get; set; } = new();
    public int Controller { get; set; } = 0;
    public int Channel { get; set; } = 1;
    public int Value { get; set; } = 0;
}

public interface ISysExEvent : IMidiEvent
{
    public int Length { get; set; }
    public string Message { get; set; }
}

public interface IMetaEvent : IMidiEvent
{
    public MidiMetaType MetaType { get; }
    public int ByteCount { get; set; }
}

public interface IMetaTextEvent : IMetaEvent
{
    public string Text { get;  set; }
}

public class SequenceNumberEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public MidiMetaType MetaType { get; } = MidiMetaType.SequenceNumber;
    public int ByteCount { get; set; } = 0;
    public int SequenceNumber { get; set; } = 0;
}

public class TextEvent : IMetaTextEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.Text;
    public int ByteCount { get; set; } = 0;
    public string Text { get; set; } = "";
}

public class CopyrightEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.Copyright;
}

public class InstrumentNameEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.InstrumentName;
}

public class SequenceOrTrackNameEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.SequenceOrTrackName;
}

public class LyricEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.Lyric;
}

public class MarkerEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.Marker;
}

public class CuePointEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.CuePoint;
}

public class ProgramNameEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.ProgramName;
}

public class DeviceNameEvent : TextEvent
{
    public override MidiMetaType MetaType => MidiMetaType.DeviceName;
}

public class ChannelPrefixEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.ChannelPrefix;
    public int ByteCount { get; set; } = 0;
    public int Channel { get; set; } = 1;
}

public class PortEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.Port;
    public int ByteCount { get; set; } = 0;
    public int Port { get; set; } = 1;
}

public class EndOfTrackEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.EndOfTrack;
    public int ByteCount { get; set; } = 0;
}
public class TempoEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.Tempo;
    public int ByteCount { get; set; } = 0;
    public long Tempo { get; set; } = 1;
}
public class SMTPEOffsetEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.SMTPEOffset;
    public int ByteCount { get; set; } = 0;
    public int Hour { get; set; } = 0;
    public int Minute { get; set; } = 0;
    public int Second { get; set; } = 0;
    public int Frame { get; set; } = 0;
    public int FractionalFrame { get; set; } = 0;
}
public class TimeSignatureEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.TimeSignature;
    public int ByteCount { get; set; } = 0;
    public int Numerator { get; set; } = 4;
    public int Denominator { get; set; } = 2;
    public int Clocks { get; set; } = 0;
    public int ThirtySecondNotesPerQuaver { get; set; } = 8;
}

public class KeySignatureEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.KeySignature;
    public int ByteCount { get; set; } = 0;
    public int Sharps { get; set; } = 0;
    public int Flats { get; set; } = 0;
    public bool Minor { get; set; } = false;
}

public class SequencerSpecificEvent : IMetaEvent
{
    public MidiEventType Type { get; } = MidiEventType.MetaEvent;
    public MidiEventPosition Position { get; set; } = new();
    public virtual MidiMetaType MetaType { get; } = MidiMetaType.SequenceSpecific;
    public int ByteCount { get; set; } = 0;
    public byte[] Data { get; set; } = [];

}