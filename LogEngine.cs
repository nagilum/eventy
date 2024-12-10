using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace Eventy;

public class LogEngine(Options options)
{
    /// <summary>
    /// Console logger.
    /// </summary>
    private readonly ConsoleLogger _logger = new();

    /// <summary>
    /// Query for log names or entries matching the parsed options.
    /// </summary>
    public void Query()
    {
        // Query and list all log names the user have access to.
        if (options.LogName is null &&
            options.RecordId is null)
        {
            this.QueryLogNames();
        }
        
        // Query and list log entries from the given log name.
        else if (options.LogName is not null &&
                 options.RecordId is null)
        {
            this.QueryLogEntries();
        }
        
        // Query log names and display the first record that matches the ID.
        else if (options.RecordId is not null)
        {
            this.QueryLogEntry();
        }
    }

    /// <summary>
    /// Check event record created time to be within set dates.
    /// </summary>
    /// <param name="record">Event record.</param>
    /// <returns>Success.</returns>
    private bool CheckCreatedDateTime(EventRecord record)
    {
        var fromOk = false;
        var toOk = false;

        if (options.QueryFrom is null ||
            record.TimeCreated is null)
        {
            fromOk = true;
        }
        else if (options.QueryFrom < record.TimeCreated)
        {
            fromOk = true;
        }

        if (options.QueryTo is null ||
            record.TimeCreated is null)
        {
            toOk = true;
        }
        else if (options.QueryTo > record.TimeCreated)
        {
            toOk = true;
        }

        return fromOk && toOk;
    }

    /// <summary>
    /// Check event record for matching log level.
    /// </summary>
    /// <param name="record">Event record.</param>
    /// <returns>Success.</returns>
    private bool CheckForMatchingLogLevel(EventRecord record)
    {
        if (options.LogLevels.Count is 0 ||
            record.Level is null)
        {
            return true;
        }

        return options.LogLevels.Contains(record.Level.Value);
    }

    /// <summary>
    /// Check event record for matches in search terms.
    /// </summary>
    /// <param name="record">Event record.</param>
    /// <returns>Success.</returns>
    private bool CheckForMatchingSearchTerms(EventRecord record)
    {
        if (options.SearchTerms.Count is 0)
        {
            return true;
        }

        var data = new List<string?>
        {
            record.Id.ToString(),
            record.ActivityId.ToString(),
            record.ProcessId.ToString(),
            record.RecordId.ToString(),
            record.ThreadId.ToString(),
            record.ProviderId.ToString(),
            record.ProviderName,
            record.MachineName,
            record.UserId?.ToString()
        };
        
        try
        {
            var description = record.FormatDescription();

            if (description is not null)
            {
                data.Add(description);
            }
        }
        catch
        {
            // Do nothing.
        }
        
        try
        {
            if (record.UserId is not null)
            {
                var userName = new System.Security.Principal.SecurityIdentifier(record.UserId.Value)
                    .Translate(typeof(System.Security.Principal.NTAccount))
                    .ToString();

                data.Add(userName);
            }
        }
        catch
        {
            // Do nothing.
        }

        try
        {
            data.Add(record.OpcodeDisplayName);
            data.Add(record.TaskDisplayName);
        }
        catch
        {
            // Do nothing.
        }

        try
        {
            data.AddRange(record.KeywordsDisplayNames);
        }
        catch
        {
            // Do nothing.
        }

        var matches =
            (
                from term in options.SearchTerms
                from value in data.Where(n => n is not null)
                where value!.IndexOf(term, StringComparison.InvariantCulture) > -1
                select term
            )
            .Count();

        switch (options.SearchMustMatchAll)
        {
            case true when matches == options.SearchTerms.Count:
            case false when matches > 0:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Check if the current user has access to query entries.
    /// </summary>
    /// <param name="eventLog">Event log.</param>
    /// <returns>Success.</returns>
    private bool UserHasAccess(EventLog eventLog)
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

    /// <summary>
    /// Query and list all log names the user have access to.
    /// </summary>
    private void QueryLogNames()
    {
        try
        {
            var names = EventLog.GetEventLogs()
                .Where(this.UserHasAccess)
                .Select(n => n.LogDisplayName)
                .OrderBy(n => n)
                .ToArray();

            _logger.Write(
                "Found ",
                ConsoleColor.Blue,
                names.Length,
                (byte)0x00,
                " accessible log name",
                names.Length is 1 ? ":" : "s:",
                Environment.NewLine);

            foreach (var name in names)
            {
                _logger.Write(
                    ConsoleColor.White,
                    name,
                    Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            _logger.WriteException(ex);
        }
    }

    /// <summary>
    /// Query and list log entries from the given log name.
    /// </summary>
    private void QueryLogEntries()
    {
        _logger.Write(
            "Querying for log entries under the ",
            ConsoleColor.White,
            options.LogName!,
            (byte)0x00,
            " log name.",
            Environment.NewLine,
            Environment.NewLine);

        EventLogReader? reader = null;

        try
        {
            var query = new EventLogQuery(options.LogName, PathType.LogName)
            {
                ReverseDirection = !options.ReverseDirection
            };

            reader = new EventLogReader(query);
        }
        catch (Exception ex)
        {
            _logger.WriteException(ex);
            reader?.Dispose();
            return;
        }

        var count = 0;

        while (true)
        {
            EventRecord? record;

            try
            {
                record = reader.ReadEvent();
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
                continue;
            }

            if (record is null)
            {
                break;
            }

            var view =
                this.CheckForMatchingLogLevel(record) &&
                this.CheckCreatedDateTime(record) &&
                this.CheckForMatchingSearchTerms(record);

            if (!view)
            {
                continue;
            }

            this.ViewEventRecordRow(record);

            count++;

            if (count == options.MaxEntries)
            {
                break;
            }
        }

        if (count is 0)
        {
            _logger.Write(
                Environment.NewLine,
                "Found no log entries matching query under the ",
                ConsoleColor.White,
                options.LogName!,
                (byte)0x00,
                " log name.",
                Environment.NewLine);
        }
        else
        {
            _logger.Write(
                Environment.NewLine,
                "Found ",
                ConsoleColor.Blue,
                count,
                (byte)0x00,
                " log entr",
                count is 1 ? "y" : "ies",
                " matching query under the ",
                ConsoleColor.White,
                options.LogName!,
                (byte)0x00,
                " log name.",
                Environment.NewLine);
        }

        reader.Dispose();
    }

    /// <summary>
    /// Query log names and display the first record that matches the ID.
    /// </summary>
    private void QueryLogEntry()
    {
        string[] names;

        if (options.LogName is not null)
        {
            names = [options.LogName];
        }
        else
        {
            try
            {
                names = EventLog.GetEventLogs()
                    .Where(this.UserHasAccess)
                    .Select(n => n.LogDisplayName)
                    .OrderBy(n => n)
                    .ToArray();

                if (names.Length is 0)
                {
                    throw new Exception("No log names found.");
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
                return;
            }
        }

        var found = 0;

        foreach (var name in names)
        {
            _logger.Write(
                "Querying for log entry ",
                ConsoleColor.Blue,
                options.RecordId!,
                (byte)0x00,
                " under the ",
                ConsoleColor.White,
                name,
                (byte)0x00,
                " log name.",
                Environment.NewLine);
            
            EventRecord? record = null;

            try
            {
                var query = new EventLogQuery(name, PathType.LogName)
                {
                    ReverseDirection = !options.ReverseDirection
                };

                using var reader = new EventLogReader(query);

                while (reader.ReadEvent() is { } eventRecord)
                {
                    if (eventRecord.RecordId?.Equals(options.RecordId) is not true)
                    {
                        continue;
                    }

                    record = eventRecord;
                    break;
                }
            }
            catch (EventLogNotFoundException)
            {
                _logger.WriteError($"Unable to access event log {name}");
                continue;
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
                continue;
            }

            if (record is null)
            {
                if (options.LogName is null)
                {
                    _logger.WriteError("Record not found!");
                }
                
                continue;
            }

            found++;

            this.ViewEventRecordFull(record);
        }

        switch (found)
        {
            case > 1:
                _logger.Write(
                    "Found ",
                    ConsoleColor.Blue,
                    found,
                    (byte)0x00,
                    " log entries.",
                    Environment.NewLine);
                break;
            
            case 0:
                _logger.WriteError("Record not found!");
                break;
        }
    }

    /// <summary>
    /// Print out all info about the event record.
    /// </summary>
    /// <param name="record">Event record.</param>
    private void ViewEventRecordFull(EventRecord record)
    {
        string? description = default;
        string? opcodeDisplayName = default;
        string? userName = default;
        string? taskDisplayName = default;
        string[]? keywords = default;

        try
        {
            description = record.FormatDescription();
        }
        catch
        {
            // Do nothing.
        }

        try
        {
            userName = new System.Security.Principal.SecurityIdentifier(record.UserId.Value)
                .Translate(typeof(System.Security.Principal.NTAccount))
                .ToString();
        }
        catch
        {
            // Do nothing.
        }

        try
        {
            keywords = record.KeywordsDisplayNames.ToArray();
        }
        catch
        {
            // Do nothing.
        }
        
        try
        {
            opcodeDisplayName = record.OpcodeDisplayName;
            taskDisplayName = record.TaskDisplayName;
        }
        catch
        {
            // Do nothing.
        }
        
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
            { "User", userName ?? "-" },
            { "OpCode", opcodeDisplayName ?? "-" },
            { "Task", taskDisplayName ?? "-" },
            { "Keywords", keywords?.Length > 0 ? string.Join(", ", keywords) : "-" },
                
            { "Logged", record.TimeCreated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-" },
            { "Description", string.Empty }
        };
        
        var longest = 0;

        foreach (var (key, _) in dict)
        {
            if (key.Length > longest)
            {
                longest = key.Length;
            }
        }

        _logger.Write(Environment.NewLine);

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

        if (description is not null)
        {
            _logger.Write(
                description,
                Environment.NewLine);
        }
        
        _logger.Write(Environment.NewLine);
    }

    /// <summary>
    /// View a condensed row-version of the event record.
    /// </summary>
    /// <param name="record">Event record.</param>
    private void ViewEventRecordRow(EventRecord record)
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
            0 => "Information",
            1 => "Critical",
            2 => "Error",
            3 => "Warning",
            4 => "Information",
            5 => "Verbose",
            _ => "Unknown"
        };

        _logger.Write(
            levelColor,
            levelName,
            new string(' ', 13 - levelName.Length),
            ConsoleColor.Blue,
            id,
            new string(' ', 10 - id.Length),
            ConsoleColor.Green,
            created,
            new string(' ', 21 - created.Length),
            ConsoleColor.White,
            source,
            Environment.NewLine);
    }
}