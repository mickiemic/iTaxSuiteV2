using Serilog;

namespace iTaxSuite.Library.Extensions
{
    public class UI
    {
        public static void Trace(string message)
        {
            Log.Verbose(message);
        }
        public static void Trace(string template, params object[] args)
        {
            Log.Verbose(template, args);
        }
        public static void Debug(string message)
        {
            Log.Debug(message);
        }
        public static void Debug(string template, params object[] args)
        {
            Log.Debug(template, args);
        }
        public static void Info(string message)
        {
            Log.Information(message);
        }
        public static void Info(string template, params object[] args)
        {
            Log.Information(template, args);
        }
        public static void Warn(string message)
        {
            Log.Warning(message);
        }
        public static void Warn(string template, params object[] args)
        {
            Log.Warning(template, args);
        }
        public static void Error(string message)
        {
            Log.Error(message);
        }
        public static void Error(Exception exception, string message)
        {
            Log.Error(exception, message);
        }
        public static void Error(string template, params object[] args)
        {
            Log.Error(template, args);
        }

        public static void Fatal(string message)
        {
            Log.Fatal(message);
        }
        public static void Fatal(Exception exception, string message)
        {
            Log.Fatal(exception, message);
        }
        public static void Fatal(string template, params object[] args)
        {
            Log.Fatal(template, args);
        }
    }
}
