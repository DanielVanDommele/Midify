using Midify.logic;

public static class BinaryHelper
{
    public static byte[] ReadNext(FileStream fs, int count, bool ignoreEndianCheck = false)
    {
        byte[] buffer = new byte[count];
        fs.ReadExactly(buffer, 0, count);
        if (!ignoreEndianCheck && BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer);
        }
        return buffer;
    }

    public static byte[] ReadNext(MidiTrackStream mts, int count, bool ignoreEndianCheck = false)
    {
        byte[] buffer = new byte[count];
        _ = mts.Read(buffer, 0, count);
        if (!ignoreEndianCheck && BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer);
        }
        return buffer;
    }

    public static VariableLengthQuantity ReadVariableLengthQuantity(FileStream fs)
    {
        int value = 0;
        int bytesRead = 0;
        int nextByte;
        do
        {
            nextByte = fs.ReadByte();
            bytesRead++;
            if (nextByte == -1)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading variable-length quantity.");
            }
            value = (value << 7) | (nextByte & 0x7F);
        } while ((nextByte & 0x80) != 0);
        return new VariableLengthQuantity(value, bytesRead);
    }

    public static VariableLengthQuantity ReadVariableLengthQuantity(MidiTrackStream mts)
    {
        int value = 0;
        int bytesRead = 0;
        int nextByte;
        do
        {
            nextByte = mts.ReadByte();
            bytesRead++;
            if (nextByte == -1)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading variable-length quantity.");
            }
            value = (value << 7) | (nextByte & 0x7F);
        } while ((nextByte & 0x80) != 0);
        return new VariableLengthQuantity(value, bytesRead);
    }

    public static string GetChunkIdentifier(byte[] bytes)
    {
        return System.Text.Encoding.ASCII.GetString(bytes);
    }
}