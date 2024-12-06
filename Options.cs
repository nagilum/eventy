namespace Eventy;

public class Options : IOptions
{
    /// <summary>
    /// <inheritdoc cref="IOptions.Command"/>
    /// </summary>
    public Command? Command { get; set; }

    /// <summary>
    /// <inheritdoc cref="IOptions.LogId"/>
    /// </summary>
    public string? LogId { get; set; }

    /// <summary>
    /// <inheritdoc cref="IOptions.LogName"/>
    /// </summary>
    public string? LogName { get; set; }

    /// <summary>
    /// <inheritdoc cref="IOptions.MaxEntries"/>
    /// </summary>
    public int? MaxEntries { get; set; } = 10;

    /// <summary>
    /// <inheritdoc cref="IOptions.ReverseDirection"/>
    /// </summary>
    public bool ReverseDirection { get; set; }
}