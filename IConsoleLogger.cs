namespace Eventy;

public interface IConsoleLogger
{
    /// <summary>
    /// Write objects to console.
    /// </summary>
    /// <param name="objects">Objects.</param>
    void Write(params object[] objects);
}