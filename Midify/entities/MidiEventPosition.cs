using Midify.entities;

public class MidiEventPosition
{
    public int Absolute { get; set; } = 0;
    public int Delta { get; set; } = 0;

    public MidiBBT BarBeatTick { get; set; } = new MidiBBT();
}