namespace Eventy;

public class ConsoleLogger : IConsoleLogger
{
    /// <summary>
    /// <inheritdoc cref="IConsoleLogger.Write"/>
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
}