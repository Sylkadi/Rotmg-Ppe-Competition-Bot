
namespace DiscordBot
{
    public class Log
    {
        public static void Fatal(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[{DateTime.Now}] [Fatal] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Error(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}] [Error] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Warning(object message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}] [Warning] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Info(object message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now}] [Info] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Debug(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[{DateTime.Now}] [Debug] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Trace(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"[{DateTime.Now}] [Trace] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Success(object message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{DateTime.Now}] [Success] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
