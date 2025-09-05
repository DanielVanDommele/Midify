public enum MidiEventType
{
    MidiEvent,
    SysExEvent,
    MetaEvent,
    NoteEvent,
    Unspecified = -1
}

public enum MidiEventSubType
{
    NoteOff = 128,
    NoteOn = 144,
    PolyphonicPressure = 160,
    Controller = 176,
    ProgramChange = 192,
    ChannelPressure = 208,
    PitchBend = 224,
    SysExStart = 240,
    SysExContinueOrEndOrEscape = 247,
    Meta = 255
}