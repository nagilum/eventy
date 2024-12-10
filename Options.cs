namespace Eventy;

public class Options
{
    /// <summary>
    /// Path to export result as JSON to.
    /// </summary>
    public string? ExportPath { get; set; }
    
    /// <summary>
    /// Log levels to list.
    /// </summary>
    public List<byte> LogLevels { get; } = [];
    
    /// <summary>
    /// Log name to list records from.
    /// </summary>
    public string? LogName { get; set; }

    /// <summary>
    /// Max number of entries to list.
    /// </summary>
    public int? MaxEntries { get; set; } = 10;
    
    /// <summary>
    /// List log entries from (including) this date.
    /// </summary>
    public DateTime? QueryFrom { get; set; }
    
    /// <summary>
    /// List log entries to (including) this date.
    /// </summary>
    public DateTime? QueryTo { get; set; }
    
    /// <summary>
    /// Record to view.
    /// </summary>
    public long? RecordId { get; set; }

    /// <summary>
    /// Reverse the order of log entries to oldest to newest.
    /// </summary>
    public bool ReverseDirection { get; set; }

    /// <summary>
    /// Whether all search terms must be found or just one for a match.
    /// </summary>
    public bool SearchMustMatchAll { get; set; }

    /// <summary>
    /// Search terms to match.
    /// </summary>
    public List<string> SearchTerms { get; } = [];
}