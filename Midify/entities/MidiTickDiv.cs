public class MidiTickDiv
{
    public MidiTiming Timing { get; set; } = MidiTiming.UnSpecified;

    public int? PulsePerQuarterNote { get; set; } = null; // 96

    public int? FramesPerSecond { get; set; } = null; // 25

    public int? SubFrames { get; set; } = null; // 40

    public void SetAsMetric(int ppqn)
    {
        Timing = MidiTiming.Metric;
        PulsePerQuarterNote = ppqn;
        FramesPerSecond = null;
        SubFrames = null;
    }

    public void SetAsTimeCode(int fps, int sf)
    {
        Timing = MidiTiming.TimeCode;
        PulsePerQuarterNote = null;
        FramesPerSecond = fps;
        SubFrames = sf;
    }
}