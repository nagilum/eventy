namespace Eventy;

public class ConsoleLogger
{
    /// <summary>
    /// Write objects to console.
    /// </summary>
    public void Write(params object[] objects)
    {
        foreach (var obj in objects)
        {
            switch (obj)
            {
                case ConsoleColor color:
                    Console.ForegroundColor = color;
                    break;
                
                case byte and 0x00:
                    Console.ResetColor();
                    break;
                
                default:
                    Console.Write(obj);
                    break;
            }
        }
        
        Console.ResetColor();
    }

    /// <summary>
    /// Write error message to console.
    /// </summary>
    public void WriteError(string message)
    {
        this.Write(
            ConsoleColor.Red,
            "Error: ",
            (byte)0x00,
            message,
            Environment.NewLine);
    }

    /// <summary>
    /// Write exception message to console as error.
    /// </summary>
    public void WriteException(Exception exception)
    {
        this.WriteError(exception.Message);
    }
}