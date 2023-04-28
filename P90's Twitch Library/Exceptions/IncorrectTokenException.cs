using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.Exceptions
{
    public class IncorrectTokenException : Exception
    {
        public IncorrectTokenException(ILogger Logger = null) : base("The provided credentials could not be processed corrently. Was there an error in the Token generation?") 
        {
            if (Logger != null)
                Logger.Log(this.Message, ILogger.Severety.Critical);
        }
    }
}
