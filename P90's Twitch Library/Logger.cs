using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace P90Ez.Twitch
{
    public interface ILogger
    {
        public  enum Severety
        {
            Message,
            Warning,
            Critical
        }
        /// <summary>
        /// Logs the provided message and severety of the message.
        /// </summary>
        public void Log(string message, Severety severety);
    }

    /// <summary>
    /// Standard Logger. It's possible to build a custom logger using the interface <see cref="ILogger"/>.
    /// </summary>
    public class Logger : ILogger
    {
        private ConcurrentQueue<string> LogQue = new ConcurrentQueue<string>();
        private event EventHandler<string> ItemAddedToQue;

        private void Enqueue(string message)
        {
            LogQue.Enqueue(message);
            ItemAddedToQue?.Invoke(this, message);
        }

        public void Log(string message, ILogger.Severety severety)
        {
            string errormessage = $"{DateTime.Now.ToLongTimeString()} - ";
            string[] stacktrace = Environment.StackTrace.Split('\n'); //Split Stacktrace by line
            if (severety == ILogger.Severety.Critical)
            {
                errormessage += "Error: " + message + $" [{severety.ToString()}]";
                if (stacktrace.Length >= 2)
                    errormessage += "\n" + String.Join("\n", stacktrace, 2, stacktrace.Length - 2); //add Stacktrace to error message (excluding this function)
            }
            else
            {
                if (stacktrace.Length > 2)
                {
                    try
                    {
                        string functionname = stacktrace[2].Split("   at ")[1].Split(" in ")[0]; //get name of function which called Log function
                        errormessage += functionname + ": " + message + $" [{severety.ToString()}]";
                    }
                    catch
                    {
                        return;
                    }
                }
            }

            Enqueue(errormessage);
        }

        #region File logging
        /// <summary>
        /// Writes logs to a file. Clears the internal log queue.
        /// </summary>
        /// <param name="fileName">Path to file.</param>
        /// <param name="continueWriting">Set to true if you want the Logger to continue writing new logs to the file.</param>
        public void WriteToFile(string fileName, bool continueWriting = false)
        {
            try
            {
                if (fileName == null || fileName == "") return;
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    while (LogQue.TryDequeue(out string result))
                    {
                        writer.WriteLine(result);
                    }
                }
            }catch { }
            if(continueWriting)
            {
                this.fileName = fileName;
                if (!LogToFileSubscribed)
                {
                    LogToFileSubscribed = true;
                    ItemAddedToQue += LogToFile;
                }
            }
        }

        private bool LogToFileSubscribed = false;
        private string fileName;
        /// <summary>
        /// Writes new log entries to the specified file.
        /// </summary>
        private void LogToFile(object sender, string e)
        {
            if (fileName == null || fileName == "") return;
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    while (LogQue.TryDequeue(out string result))
                    {
                        writer.WriteLine(result);
                    }
                }
            }catch { }
        }

        /// <summary>
        /// Will write all future logs to the specified file. Can be stopped with <see cref="StopFileLog"/>
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteNewLogsToFile(string fileName)
        {
            if (!LogToFileSubscribed)
            {
                LogToFileSubscribed = true;
                ItemAddedToQue += LogToFile;
            }
            this.fileName = fileName;
        }

        /// <summary>
        /// Stops writing new log to the file.
        /// </summary>
        public void StopFileLog()
        {
            if (LogToFileSubscribed)
            {
                ItemAddedToQue -= LogToFile;
                LogToFileSubscribed = false;
            }
        }
        #endregion

        #region Console logging
        /// <summary>
        /// Writes log to the console. Will not clear the internal queue.
        /// </summary>
        /// <param name="continueWriting">Set to true if you want the Logger to continue writing new logs to the console.</param>
        public void WriteToConsole(bool continueWriting = false)
        {
            var arr = LogQue.ToArray();
            foreach(string l in arr)
            {
                Console.WriteLine(l);
            }
            if (continueWriting && !LogToConsoleSubscribed)
            {
                LogToConsoleSubscribed = true;
                ItemAddedToQue += LogToConsole;
            }
        }

        private bool LogToConsoleSubscribed = false;
        /// <summary>
        /// Writes new log entries to the console.
        /// </summary>
        private void LogToConsole(object sender, string e)
        {
            if(e != null && e != "")
                Console.WriteLine(e);
        }

        /// <summary>
        /// Will write all future logs to the console. Can be stopped with <see cref="StopConsoleLog"/>
        /// </summary>
        public void WriteNewLogsToConsole()
        {
            if (LogToConsoleSubscribed) return;
            LogToConsoleSubscribed = true;
            ItemAddedToQue += LogToConsole;
        }

        /// <summary>
        /// Stops writing new logs to the console.
        /// </summary>
        public void StopConsoleLog()
        {
            if (!LogToConsoleSubscribed) return;
            ItemAddedToQue -= LogToConsole;
            LogToConsoleSubscribed = false;
        }
        #endregion
    }

}
