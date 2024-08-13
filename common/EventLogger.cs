using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace PNCommon
{
    internal static class EventLogger
    {
        internal static void LogEvent(string message, string source, EventLogEntryType type)
        {
            try
            {
                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, "Application");
                EventLog.WriteEntry(source, message, type);
            }
            catch 
            {
                //
            }
        }

        internal static string GetExceptionDescription(Exception ex)
        {
            try
            {
                //get exception type
                var type = ex.GetType();

                //create stack trace based on exception
                var stack = new StackTrace(ex, true);
                //get last frame from stack trace
                var frame = stack.GetFrame(stack.FrameCount - 1);

                var sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                sb.Append("\tType: ");
                sb.Append(type);
                sb.Append("\tMessage: ");
                sb.Append(ex.Message);
                sb.Append("\tIn: ");
                sb.Append(frame.GetFileName());
                sb.Append("; at: ");
                sb.Append(frame.GetMethod().Name);
                var line = frame.GetFileLineNumber();
                var column = frame.GetFileColumnNumber();
                if (line != 0 || column != 0)
                {
                    sb.Append("; line: ");
                    sb.Append(line);
                    sb.Append("; column: ");
                    sb.Append(column);
                }
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
