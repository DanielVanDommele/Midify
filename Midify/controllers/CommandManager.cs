public static class CommandManager
{
    public static void Help()
    {
        Console.WriteLine("Commands");
        Console.WriteLine("");
        Console.WriteLine(" - help : this help screen");
        Console.WriteLine(" - about : gives information about Midify");
        Console.WriteLine(" - summary [in:[filename]] [out:[filename]] : gives a summary overview of the midifile. If an output filename is provided, the content is saved as a text file, otherwise it is displayed on screen");
        Console.WriteLine(" - details [in:[filename]] [out:[filename]] : gives a detailed overview of the midifile including a table with all events per track. If an output filename is provided, the content is saved as a text file, otherwise it is displayed on screen");
        Console.WriteLine("");
    }

    public static void About()
    {
        Console.WriteLine("About Midify");
        Console.WriteLine("");
        Console.WriteLine("Midify is a CLI midi tool for listening and composing midi by programming music together");
        Console.WriteLine("");
    }

    public static void Summary(string inputFile, string outputFile = "")
    {
        MidiFile mf = new MidiReader(new FileInfo(inputFile)).Read();
        List<string> summary = MidiDescriber.GetSummary(mf);

        if (string.IsNullOrEmpty(outputFile))
        {
            foreach (var line in summary)
            {
                Console.WriteLine(line);
            }
        }
        else
        {
            File.WriteAllText(outputFile, string.Join(Environment.NewLine, summary));
            Console.WriteLine($"MidiFile Summary written to {outputFile}");
        }
    }

    public static void Summary(string inputFile, string outputFile = "")
    {
        MidiFile mf = new MidiReader(new FileInfo(inputFile)).Read();
        List<string> details = MidiDescriber.GetDetails(mf);

        if (string.IsNullOrEmpty(outputFile))
        {
            foreach (var line in details)
            {
                Console.WriteLine(line);
            }
        }
        else
        {
            File.WriteAllText(outputFile, string.Join(Environment.NewLine, details));
            Console.WriteLine($"MidiFile Details written to {outputFile}");
        }
    }
}