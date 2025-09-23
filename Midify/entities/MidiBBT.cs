namespace Midify.entities
{
    public class MidiBBT
    {
        public int Bar { get; set; } = 1;
        public int Beat { get; set; } = 1;
        public int Tick { get; set; } = 0;
        public double WithinBeat { get; set; } = 0.0;
    }
}