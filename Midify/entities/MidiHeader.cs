public class MidiHeader
{
    public MidiFormat Format { get; set; } = MidiFormat.MultiTrackSimultaneous;

    public int TrackCount { get; set; } = 1;

    public MidiTickDiv TickDiv { get; set; } = new();
}