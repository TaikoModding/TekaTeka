using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TekaTeka
{
    public enum LogType
    {
        Info,
        Warning,
        Error,
        Fatal,
        Message,
        Debug
    }

    internal class Logger
    {
        public static void Log(string value, LogType type = LogType.Info)
        {
            switch (type)
            {
            case LogType.Info:
                Plugin.LogSource?.LogInfo(value);
                break;
            case LogType.Warning:
                Plugin.LogSource?.LogWarning(value);
                break;
            case LogType.Error:
                Plugin.LogSource?.LogError(value);
                break;
            case LogType.Fatal:
                Plugin.LogSource?.LogFatal(value);
                break;
            case LogType.Message:
                Plugin.LogSource?.LogMessage(value);
                break;
            case LogType.Debug:
#if DEBUG
                Plugin.LogSource?.LogDebug(value);
#endif
                break;
            }
        }

        public static void Log(List<string> values, LogType type = LogType.Info)
        {
            if (values.Count == 0)
            {
                return;
            }
            string value = values[0];
            int numSpacing = "[Info   :".Length + Math.Max(Plugin.ModName.Length, 10) + 2;
            string spacing = string.Empty;
            for (int i = 0; i < numSpacing; i++)
            {
                spacing += " ";
            }
            for (int i = 1; i < values.Count; i++)
            {
                value += "\n";
                value += spacing;
                value += values[i];
            }
            Log(value, type);
        }
    }
}
