namespace Eventy;

public interface IOptions
{
    /// <summary>
    /// Command to execute.
    /// </summary>
    Command? Command { get; set; }
    
    /// <summary>
    /// Log ID.
    /// </summary>
    string? LogId { get; set; }
    
    /// <summary>
    /// Log name to query.
    /// </summary>
    string? LogName { get; set; }
    
    /// <summary>
    /// Max entries to query.
    /// </summary>
    int? MaxEntries { get; set; }
    
    /// <summary>
    /// Whether to read events from the newest event in an event log to the oldest event in the log.
    /// </summary>
    bool ReverseDirection { get; set; }
}