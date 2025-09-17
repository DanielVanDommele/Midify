public static class MidiDescriber
{
    public static List<string> GetSummary(MidiFile mf)
    {
        List<string> summary = [];
        summary.Add($"Summary for MIDI File: {mf.FileName}");
        summary.Add("-------------------------------");
        summary.Add($"Format: {mf.Header.Format}");
        summary.Add($"Number of Tracks: {mf.Header.TrackCount}");
        summary.Add($"Tick Division: {(mf.Header.TickDiv.Timing == MidiTiming.TimeCode 
                            ? ($"TimeCode ({mf.Header.TickDiv.FramesPerSecond} frames per second, {mf.Header.TickDiv.SubFrames} subframes)")
                            : ($"Metric ({mf.Header.TickDiv.PulsePerQuarterNote} pulses per quarter note)")
                        )}");
        summary.Add("");
        summary.Add("TrackList (track number, track name, channel, instrument, number of events):");
        foreach (var track in mf.Tracks.List)
        {
             summary.Add($"Track {track.TrackNumber}: {track.TrackName}, {track.Instrument}, {track.Events.Count} events");
        }
        return summary;
    }

    public static List<string> GetDetails(MidiFile mf)
    {
        List<string> details = [];
        details.AddRange(MidiDescriber.GetSummary(mf));
        details.Add("");
        details.Add("Events per Track:");
        details.Add("");
        foreach (var track in mf.Tracks.List)
        {
            details.Add($"Track #{track.TrackNumber} {track.TrackName} Event List ");
            details.Add("-------------------------------");
            foreach (var mEvent in track.Events.List)
            {
                string outStr = "";

                Dictionary<string, string> eventParameters = mEvent.Parameters();
                foreach (var kv in eventParameters.ToList())
                {
                    switch (kv.Key)
                    {
                        case "Numerator":
                            outStr = "Time Signature: " + kv.Value + "\t\t";
                            break;
                        case "Denominator":
                            outStr = $"/{MidiDescriber.GetDenominator(kv.Value)}\t\t";
                            break;
                        case "Key":
                            outStr = "Key Signature: " + kv.Value + "\t\t";
                            break;
                        case "Channel":
                            outStr += $"#{kv.Value}\t\t";
                            break;
                        default:
                            outStr += $"{kv.Key}: {kv.Value}\t\t";
                            break;
                    }
                }
                outStr += $"@{mEvent.Position.Absolute} (D: {mEvent.Position.Delta}):";

                details.Add(outStr);
            }
            details.Add("-------------------------------");
        }
        return details;
    }

    private static string GetDenominator (string inValue)
    {
        return (2 ^ int.Parse(inValue)).ToString();
    }
}
