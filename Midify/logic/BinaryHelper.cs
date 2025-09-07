public static class BinaryHelper
{
    public static byte[] ReadNext(FileStream fs, int offset, int count)
    {
        byte[] buffer = [];
        fs.ReadExactly(buffer, offset, count);
        return buffer;
    }
}