public static class MidiDescriber
{
    public static List<string> GetSummary(MidiFile mf)
    {
        List<string> summary = new();
        summary.Add($"Summary for MIDI File: {mf.FileName}");
        summary.Add("-------------------------------");
        summary.Add($"Format: {mf.Header.Format}");
        summary.Add($"Number of Tracks: {mf.Header.NumberOfTracks}");
        summary.Add($"Tick Division: {(mf.Header.TickDiv.Timing === MidiTiming.TimeCode 
                            ? ($"TimeCode ({mf.Header.TickDiv.FramesPerSecond} frames per second, {mf.Header.TickDiv.SubFrames} subframes)")
                            : ($"Metric ({mf.Header.TickDiv.PulsePerQuarterNote} pulses per quarter note)")
                        )}");
        summary.Add("");
        // summary.Add("TrackList (track number, track name, channel, instrument, number of events):");
        // foreach (var track in mf.Tracks)
        // {
        //     summary.Add($"Track {track.TrackNumber}: {track.TrackName}, channel {track.Channel}, {track.Instrument}, {track.Events.Count} events");
        // }
        // return summary;
    }

    public static List<string> GetDetails(MidiFile mf)
    {
        List<string> details = new();
        return details;
    }

}
