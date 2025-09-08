public class MidiTrack
{
    public int TrackNumber { get; set; } = 0;

    public string TrackName { get; set; } = "";

    public int Channel { get; set; } = 0;

    public int Pan { get; set; } = 64;

    public int Velocity { get; set; } = 64;

    public MidiInstrument Instrument { get; set; } = MidiInstrument.AccousticGrandPiano;

    MidiEventCollection Events { get; set; } = new();
}