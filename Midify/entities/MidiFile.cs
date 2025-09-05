public class MidiFile
{
    public MidiFile(string fileName, int fileSize)
    {
        FileName = fileName;
        FileSize = fileSize;
    }

    public string FileName { get; set; } = "";

    public int FileSize { get; set; } = 0;

    public MidiHeader Header { get; set; } = new();

    public MidiTrackCollection Tracks { get; set; } = new();
}