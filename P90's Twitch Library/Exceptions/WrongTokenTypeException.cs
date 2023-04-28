using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.Exceptions
{
    public class WrongTokenTypeException : Exception
    {
        public WrongTokenTypeException(Login.TokenType RequieredTokenType, Login.TokenType ProvidedTokenType, string where, ILogger Logger = null)
            : base($"Wrong Token Type! Where: {where}, " +
                  $"provided token from type {ProvidedTokenType.ToString()} - " +
                  $"requires a token from type {RequieredTokenType.ToString()}") 
        {
            if(Logger != null)
                Logger.Log(this.Message, ILogger.Severety.Critical);
        }
    }
}
