namespace Eventy;

public interface ILogEngine
{
    /// <summary>
    /// List all available log names. 
    /// </summary>
    void ListLogNames();

    /// <summary>
    /// Query for log entries.
    /// </summary>
    /// <param name="options">Parsed options.</param>
    void QueryLogEntries(Options options);

    /// <summary>
    /// Realtime logging of log entries.
    /// </summary>
    /// <param name="options">Parsed options.</param>
    void TailLogEntries(Options options);

    /// <summary>
    /// View log entry.
    /// </summary>
    /// <param name="options">Parsed options.</param>
    void ViewLogEntry(Options options);
}