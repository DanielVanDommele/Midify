public class MidiEventCollection
{
    private List<IMidiEvent> events = new();

    public void AddEvent(IMidiEvent midiEvent) {
        events.Add(midiEvent);
    }

    public void RemoveEvent(IMidiEvent midiEvent)
    {
        events.Remove(midiEvent);
    }
    
    public int Count()
    {
        return events.Count;
    }
