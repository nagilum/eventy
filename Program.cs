using System.Globalization;
using Serilog;

namespace Eventy;

internal static class Program
{
    /// <summary>
    /// Logger service.
    /// </summary>
    private static readonly Serilog.Core.Logger Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();
    
    /// <summary>
    /// Init all the things...
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static void Main(string[] args)
    {
        if (args.Any(n => n.ToLower() is "-h" or "--help"))
        {
            ShowProgramUsage();
            return;
        }

        if (!TryParseCmdArgs(args, out var options))
        {
            return;
        }

        var engine = new LogEngine(options);

        engine.Query();
    }

    /// <summary>
    /// Show program usage and options.
    /// </summary>
    private static void ShowProgramUsage()
    {
        Console.WriteLine(
            """
            Eventy v0.3-beta
            Windows event log viewer
            
            Usage:
              eventy [log-name] [record-id] [options]
            
            Options:
              -m|--max <number>    Set max number of log entries to list. Defaults to 10.
              -r|--reverse         Whether to read events from oldest to newest. Defaults to newest to oldest.
              -f|--from <date>     List log entries from (including) given date/time.
              -t|--to <date>       List log entries to (including) given date/time.
              -l|--level <name>    Only list log entries matching given log level name. Can be repeated.
              -s|--search <term>   Search in any field for the given text. Can be repeated.
              -a|--all             Set whether all search terms must be found or just one for a match.
              -x|--export <path>   Set path to export result as JSON to.
              
            Examples:
              eventy Application -m 50 -l info -s foo -s bar
              eventy Application 123456
              eventy Application -m -1 -f 2024-12-01 -l info -l error -l crit -s foo -s bar
              
            Use -h or --help to view this screen.
            
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
                        Logger.Error("Argument {argv} must be followed by a number.", argv);
                        return false;
                    }

                    if (!int.TryParse(args[i + 1], out var max))
                    {
                        Logger.Error("Unable to parse {argv} to a valid number.", args[i + 1]);
                        return false;
                    }

                    options.MaxEntries = max > 0 ? max : null;
                    skip = true;
                    break;
                
                case "-r":
                case "--reverse":
                    options.ReverseDirection = true;
                    break;
                
                case "-f":
                case "--from":
                    if (i == args.Length - 1)
                    {
                        Logger.Error("Argument {argv} must be followed by a date/time.", argv);
                        return false;
                    }

                    argv = args[i + 1];

                    switch (argv.Length)
                    {
                        case 10 when
                            DateTime.TryParseExact(
                                argv, 
                                "yyyy-MM-dd", 
                                CultureInfo.InvariantCulture, 
                                DateTimeStyles.None,
                                out var from8):
                            options.QueryFrom = from8;
                            break;
                        
                        case 19 when
                            DateTime.TryParseExact(
                                argv,
                                "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var from19):
                            options.QueryFrom = from19;
                            break;
                        
                        default:
                            Logger.Error("Unable to parse {argv} to a valid date/time. Must be in format \"yyyy-MM-dd\" or \"yyyy-MM-dd HH:mm:ss\"", argv);
                            return false;
                    }
                    
                    skip = true;
                    break;
                
                case "-t":
                case "--to":
                    if (i == args.Length - 1)
                    {
                        Logger.Error("Argument {argv} must be followed by a date/time.", argv);
                        return false;
                    }
                    
                    argv = args[i + 1];

                    switch (argv.Length)
                    {
                        case 10 when
                            DateTime.TryParseExact(
                                argv, 
                                "yyyy-MM-dd", 
                                CultureInfo.InvariantCulture, 
                                DateTimeStyles.None,
                                out var to8):

                            options.QueryTo = new DateTime(to8.Year, to8.Month, to8.Day, 23, 59, 59);
                            break;
                        
                        case 19 when
                            DateTime.TryParseExact(
                                argv,
                                "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var to19):
                            
                            options.QueryFrom = to19;
                            break;
                        
                        default:
                            Logger.Error("Unable to parse {argv} to a valid date/time. Must be in format \"yyyy-MM-dd\" or \"yyyy-MM-dd HH:mm:ss\"", argv);
                            return false;
                    }
                    
                    skip = true;
                    break;
                
                case "-l":
                case "--level":
                    if (i == args.Length - 1)
                    {
                        Logger.Error("Argument {argv} must be followed by a level name or index number.", argv);
                        return false;
                    }

                    argv = args[i + 1].ToLower();

                    switch (argv)
                    {
                        case "1" or "cri" or "crit" or "critical":
                            options.LogLevels.Add(1);
                            break;
                        
                        case "2" or "err" or "error":
                            options.LogLevels.Add(2);
                            break;
                        
                        case "3" or "war" or "warn" or "warning":
                            options.LogLevels.Add(3);
                            break;
                        
                        case "4" or "inf" or "info" or "information":
                            options.LogLevels.Add(0);
                            options.LogLevels.Add(4);
                            break;
                        
                        case "5" or "ver" or "verb" or "verbose":
                            options.LogLevels.Add(5);
                            break;
                        
                        default:
                            Logger.Error("Unable to parse {argv} to a valid log level.", argv);
                            return false;
                    }

                    skip = true;
                    break;
                
                case "-s":
                case "--search":
                    if (i == args.Length - 1)
                    {
                        Logger.Error("Argument {argv} must be followed by a search term.", argv);
                        return false;
                    }

                    options.SearchTerms.Add(args[i + 1]);
                    skip = true;
                    break;
                
                case "-a":
                case "--all":
                    options.SearchMustMatchAll = true;
                    break;
                
                case "-x":
                case "--export":
                    if (i == args.Length - 1)
                    {
                        Logger.Error("Argument {argv} must be followed by a file path.", argv);
                        return false;
                    }

                    options.ExportPath = args[i + 1];
                    skip = true;
                    break;
                
                default:
                    if (long.TryParse(argv, out var recordId))
                    {
                        options.RecordId = recordId;
                    }
                    else
                    {
                        if (options.LogName is not null)
                        {
                            Logger.Error("You have already defined a log name to search within.");
                            return false;
                        }

                        options.LogName = argv;
                    }
                    
                    break;
            }
        }

        return true;
    }
}