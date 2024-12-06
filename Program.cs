namespace Eventy;

internal static class Program
{
    /// <summary>
    /// Logger service.
    /// </summary>
    private static readonly ConsoleLogger Logger = new();
    
    /// <summary>
    /// Init all the things...
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static void Main(string[] args)
    {
        if (args.Length is 0 ||
            args.Any(n => n is "-h" or "--help"))
        {
            ShowProgramUsage();
            return;
        }

        if (!TryParseCmdArgs(args, out var options))
        {
            return;
        }

        var engine = new LogEngine();

        switch (options.Command)
        {
            case Command.ListLogNames:
                engine.ListLogNames();
                break;
            
            case Command.QueryLogEntries:
                engine.QueryLogEntries(options);
                break;
            
            case Command.TailLogEntries:
                engine.TailLogEntries(options);
                break;
            
            case Command.ViewLogEntry:
                engine.ViewLogEntry(options);
                break;
            
            default: return;
        }
    }

    /// <summary>
    /// Show program usage and options.
    /// </summary>
    private static void ShowProgramUsage()
    {
        Console.WriteLine(
            """
            Eventy v0.1-alpha
            Windows event log viewer
            
            Usage:
              eventy <command> <options>
              
            Commands:
              list    List all available log names.
              query   Query for log entries under log name.
              view    View a single log entry.
            
            Options:
              -m|--max [<number>]   Set max records to query. Defaults to 10. Omit value for all.
              -r|--reverse          Whether to read events from the newest or oldest event.
              
            Examples:
              eventy list
              eventy query Application
              eventy view Application 123456
            
            Source and documentation at https://github.com/nagilum/eventy
            """);
    }

    /// <summary>
    /// Attempt to parse the command line arguments.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="options">Parsed options.</param>
    /// <returns>Success.</returns>
    private static bool TryParseCmdArgs(string[] args, out Options options)
    {
        options = new();

        options.Command = args[0] switch
        {
            "list" => Command.ListLogNames,
            "query" => Command.QueryLogEntries,
            "view" => Command.ViewLogEntry,
            _ => options.Command
        };

        if (options.Command is null)
        {
            Logger.Write(
                ConsoleColor.DarkRed,
                "Error: ",
                (byte)0x00,
                "You must specify a command.",
                Environment.NewLine);

            return false;
        }

        if (options.Command is Command.QueryLogEntries)
        {
            if (args.Length < 2)
            {
                Logger.Write(
                    ConsoleColor.DarkRed,
                    "Error: ",
                    (byte)0x00,
                    "You must specify a log name to query.",
                    Environment.NewLine);

                return false;
            }

            options.LogName = args[1];
        }

        if (options.Command is Command.ViewLogEntry)
        {
            if (args.Length < 3)
            {
                Logger.Write(
                    ConsoleColor.DarkRed,
                    "Error: ",
                    (byte)0x00,
                    "You must specify a log name and log id to view.",
                    Environment.NewLine);

                return false;
            }

            options.LogName = args[1];
            options.LogId = args[2];
        }

        if (args.Length < 2)
        {
            return true;
        }

        var skipArgs = options.Command switch
        {
            Command.ListLogNames => 1,
            Command.QueryLogEntries => 1,
            Command.TailLogEntries => 1,
            Command.ViewLogEntry => 2,
            _ => 0
        };

        args = args
            .Skip(skipArgs)
            .ToArray();

        var skip = false;

        for (var i = 0; i < args.Length; i++)
        {
            if (skip)
            {
                skip = false;
                continue;
            }

            var argv = args[i];

            switch (argv)
            {
                case "-m":
                case "--max":
                    if (i == args.Length - 1)
                    {
                        options.MaxEntries = default;
                    }
                    else if (int.TryParse(args[i + 1], out var maxEntries))
                    {
                        options.MaxEntries = maxEntries > 0
                            ? maxEntries
                            : default;

                        skip = true;
                    }

                    break;
                
                case "-r":
                case "--reverse":
                    options.ReverseDirection = true;
                    break;
            }
        }

        return true;
    }
}