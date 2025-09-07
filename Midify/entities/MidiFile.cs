public class MidiFile
{
    public MidiFile(string fileName, long fileSize)
    {
        FileName = fileName;
        FileSize = fileSize;
    }

    public string FileName { get; set; } = "";

    public long FileSize { get; set; } = 0;

    public MidiHeader Header { get; set; } = new();

    public MidiTrackCollection Tracks { get; set; } = new();
}