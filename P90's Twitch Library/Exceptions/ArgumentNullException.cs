using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.Exceptions
{
    public class ArgumentNullException : System.ArgumentNullException
    {
        public ArgumentNullException(string? paramName, ILogger Logger) : base(paramName)
        {
            if(Logger != null)
                Logger.Log(this.Message, ILogger.Severety.Critical);
        }
    }
}
