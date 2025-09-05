public class MidiTrack
{
    public string TrackName { get; set; } = "";

    public int Pan { get; set; } = 64;

    public int Velocity { get; set; } = 64;

    public MidiInstrument Instrument { get; set; } = MidiInstrument.AccousticGrandPiano;

    MidiEventCollection Events { get; set; } = new();
}