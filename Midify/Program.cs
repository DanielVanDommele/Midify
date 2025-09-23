class Program
{
    static void Main(string[] args)
    {
        // See https://aka.ms/new-console-template for more information
        Console.Clear();
        Console.WriteLine("Midify v1.0.0 by D-Lite");
        Console.WriteLine("");
        if (args.Length > 0)
        {
            string firstCommand = args[0];
            switch (firstCommand.ToLower())
            {
                case "help":
                    CommandManager.Help();
                    break;
                case "about":
                    CommandManager.About();
                    break;
                case "summary":
                case "details":
                    if (args.Length >= 2)
                    {
                        string inputFileArg = args[1];
                        if (inputFileArg.StartsWith("in:") && File.Exists(inputFileArg.Replace("in:", "")))
                        {
                            FileInfo inputFile = new FileInfo(inputFileArg.Replace("in:", ""));
                            if (inputFile.Extension == ".mid" || inputFile.Extension == ".midi")
                            {
                                string outputFile = args.Length >= 3 ? args[2] : "";
                                if (outputFile != "" && outputFile.StartsWith("out:"))
                                {
                                    outputFile = outputFile.Replace("out:", "");
                                }
                                else
                                {
                                    Console.WriteLine("incorrect outfile parameter, reverting to console output");
                                    outputFile = "";
                                }

                                if (firstCommand.ToLower() == "details")
                                {
                                    CommandManager.Details(inputFile, outputFile);
                                }
                                else
                                {
                                    CommandManager.Summary(inputFile, outputFile);
                                }
                            }
                            else
                            { 
                                Console.WriteLine("Please provide a valid MIDI file as input (with .mid or .midi extension)");
                                return;
                            }

                        }
                        else if (!File.Exists(inputFileArg.Replace("in:", "")))
                        {
                            Console.WriteLine($"The provided input file '{inputFileArg.Replace("in:", "")}' does not exist");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"The provided input file '{inputFileArg}' does not exist");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please provide an input file. Usage: Midify summary <inputFile> [outputFile]");
                    }
                    break;
                default:
                    Console.WriteLine($"'{firstCommand}' is not a command");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Please provide arguments. Enter the 'help' command to learn about what you can do with Midify!");
        }
    } 
}