using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace Eventy;

public class LogEngine : ILogEngine
{
    /// <summary>
    /// Logger service.
    /// </summary>
    private readonly ConsoleLogger _logger = new();

    /// <summary>
    /// <inheritdoc cref="ILogEngine.ListLogNames"/>
    /// </summary>
    public void ListLogNames()
    {
        try
        {
            var eventLogs = EventLog.GetEventLogs()
                .Where(HasEntries)
                .ToArray();

            var names = eventLogs
                .Select(n => n.LogDisplayName)
                .ToArray();

            foreach (var name in names)
            {
                _logger.Write(
                    name,
                    Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            _logger.Write(
                ConsoleColor.DarkRed,
                "Error: ",
                (byte)0x00,
                ex.Message,
                Environment.NewLine);
        }
    }

    /// <summary>
    /// <inheritdoc cref="ILogEngine.QueryLogEntries"/>
    /// </summary>
    public void QueryLogEntries(Options options)
    {
        try
        {
            var query = new EventLogQuery(options.LogName, PathType.LogName)
            {
                ReverseDirection = options.ReverseDirection
            };
            
            using var reader = new EventLogReader(query);
            
            var count = 0;
            var records = new List<EventRecord>();

            while (reader.ReadEvent() is { } record)
            {
                records.Add(record);
                
                count++;

                if (count == options.MaxEntries)
                {
                    break;
                }
            }

            var longestId = 0;

            foreach (var record in records)
            {
                var id = record.RecordId?.ToString() ?? "-";

                if (id.Length > longestId)
                {
                    longestId = id.Length;
                }
            }

            longestId += 1;

            foreach (var record in records)
            {
                var id = record.RecordId?.ToString() ?? "-";
                var created = record.TimeCreated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                var source = record.ProviderName ?? "-";

                var levelColor = record.Level switch
                {
                    0 => ConsoleColor.White, // LogAlways
                    1 => ConsoleColor.Red, // Critical
                    2 => ConsoleColor.Red, // Error
                    3 => ConsoleColor.Yellow, // Warning,
                    4 => ConsoleColor.White, // Informational
                    5 => ConsoleColor.Magenta, // Verbose
                    _ => ConsoleColor.Blue
                };

                var levelName = record.Level switch
                {
                    0 => "INF",
                    1 => "CRI",
                    2 => "ERR",
                    3 => "WAR",
                    4 => "INF",
                    5 => "VER",
                    _ => "UNK"
                };
                
                _logger.Write(
                    levelColor,
                    $"[{levelName}] ",
                    ConsoleColor.Blue,
                    $"#{id}",
                    new string(' ', longestId - id.Length),
                    ConsoleColor.Green,
                    created,
                    new string(' ', 20 - created.Length),
                    (byte)0x00,
                    source,
                    Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            _logger.Write(
                ConsoleColor.DarkRed,
                "Error: ",
                (byte)0x00,
                ex.Message,
                Environment.NewLine);
        }
    }

    /// <summary>
    /// <inheritdoc cref="ILogEngine.TailLogEntries"/>
    /// </summary>
    public void TailLogEntries(Options options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="ILogEngine.ViewLogEntry"/>
    /// </summary>
    public void ViewLogEntry(Options options)
    {
        try
        {
            var query = new EventLogQuery(options.LogName, PathType.LogName)
            {
                ReverseDirection = options.ReverseDirection
            };
            
            using var reader = new EventLogReader(query);
            
            EventRecord? record = null;

            while (reader.ReadEvent() is { } temp)
            {
                if (temp.RecordId?.ToString().Equals(options.LogId ?? string.Empty) is not true)
                {
                    continue;
                }

                record = temp;
                break;
            }

            if (record is null)
            {
                throw new Exception($"Record {options.LogId} in {options.LogName} not found!");
            }

            string userName;

            try
            {
                userName = new System.Security.Principal.SecurityIdentifier(record.UserId.Value)
                    .Translate(typeof(System.Security.Principal.NTAccount))
                    .ToString();
            }
            catch
            {
                userName = "-";
            }

            var keywords = record.KeywordsDisplayNames.ToList();

            var dict = new Dictionary<string, string>
            {
                { "Event Id", record.Id.ToString() },
                { "Activity Id", record.ActivityId?.ToString() ?? "-" },
                { "Process Id", record.ProcessId?.ToString() ?? "-" },
                { "Record Id", record.RecordId?.ToString() ?? "-" },
                { "Thread Id", record.ThreadId?.ToString() ?? "-" },
                { "Provider Id", record.ProviderId?.ToString() ?? "-" },
                
                { "Source", record.ProviderName ?? "-" },
                { "Level", record.LevelDisplayName ?? "-" },
                { "Log Name", record.LogName ?? "-" },
                { "Machine Name", record.MachineName ?? "-" },
                { "User", userName },
                { "OpCode", record.OpcodeDisplayName ?? "-" },
                { "Task", record.TaskDisplayName ?? "-" },
                { "Keywords", keywords.Count > 0 ? string.Join(", ", keywords) : "-" },
                
                { "Logged", record.TimeCreated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-" }
            };

            var longest = 0;

            foreach (var (key, _) in dict)
            {
                if (key.Length > longest)
                {
                    longest = key.Length;
                }
            }

            foreach (var (key, value) in dict)
            {
                _logger.Write(
                    key,
                    ": ",
                    new string(' ', longest - key.Length),
                    ConsoleColor.Blue,
                    value,
                    Environment.NewLine);
            }
            
            _logger.Write(record.FormatDescription());
        }
        catch (Exception ex)
        {
            _logger.Write(
                ConsoleColor.DarkRed,
                "Error: ",
                (byte)0x00,
                ex.Message,
                Environment.NewLine);
        }
    }

    /// <summary>
    /// Check if given event log has any entries.
    /// </summary>
    /// <param name="eventLog">Event log.</param>
    /// <returns>Success.</returns>
    private bool HasEntries(EventLog eventLog)
    {
        try
        {
            return eventLog.Entries.Count > 0;
        }
        catch
        {
            return false;
        }
    }
}