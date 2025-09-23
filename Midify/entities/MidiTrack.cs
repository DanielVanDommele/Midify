public class MidiTrack
{
    public int TrackNumber { get; set; } = 0;

    public string TrackName { get; set; } = "";

    public MidiInstrument Instrument { get; set; } = MidiInstrument.NotSet;

    public MidiEventCollection Events { get; set; } = new();
}