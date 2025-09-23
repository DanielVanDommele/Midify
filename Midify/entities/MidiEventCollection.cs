public class MidiEventCollection
{
    private List<IEventBase> events = new();

    public void AddEvent(IEventBase midiEvent)
    {
        events.Add(midiEvent);
    }

    public void RemoveEvent(IEventBase midiEvent)
    {
        events.Remove(midiEvent);
    }

    public int Count
    {
        get { return events.Count; }
    }

    public List<IEventBase> List
    { 
        get { return events; } 
    }
}
