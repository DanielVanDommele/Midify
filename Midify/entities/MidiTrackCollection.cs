public class MidiTrackCollection
{
    private List<MidiTrack> tracks = new();

    public MidiTrack GetTrackByIndex(int index)
    {
        return tracks[index];
    }

    public MidiTrack GetTrackByName(string name)
    {
        return tracks[0];
    }

    public void AddTrack(MidiTrack track)
    {
        tracks.Add(track);
    }

    public void RemoveTrack(MidiTrack track)
    {
        tracks.Remove(track);
    }
}