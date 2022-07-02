using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.IRC
{
    public class irc_CostumEventArgs
    {
        public class HostArgs
        {
            /// <summary>
            /// Origin channel
            /// </summary>
            public string ChannelName { get; internal set; }
            /// <summary>
            /// Host target
            /// </summary>
            public string TargetChannelName { get; internal set; }
        }
        public class JoinPartArgs
        {
            public string ChannelName { get; internal set; }
            public string Username { get; internal set; }
            public string Type { get; internal set; }
            public List<string> AllUsers { get; internal set; }
        }
        public class NameListArgs
        {
            public string ChannelName { get; internal set; }
            public List<string> UsersInChat { get; internal set; }
        }
    }
}
