public class VariableLengthQuantity
{
    public VariableLengthQuantity(int value, int byteCount)
    {
        Value = value;
        ByteCount = byteCount;
    }

    public int Value { get; set; } = 0;
    public int ByteCount { get; set; } = 0;
}