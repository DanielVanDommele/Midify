class Program
{
    static void Main(string[] args)
    {
        // See https://aka.ms/new-console-template for more information
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
                case "About":
                    CommandManager.About();
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