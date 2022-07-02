using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.IRC
{
    public class irc_clearchat
    {
        public long ChannelID { get; }
        public string ChannelName { get; }
        /// <summary>
        /// Only returns a non-empty-string when a user was timeouted/banned.
        /// </summary>
        public string TargetUsername { get; } = "";
        /// <summary>
        /// Only returns a positive value when a user was timeouted/banned.
        /// </summary>
        public long TargetUserID { get; } = -1;
        public DateTime TimeStamp { get; }
        /// <summary>
        /// Unix Timestamp in Milliseconds
        /// </summary>
        public long UnixTimestampMill { get; }

        /// <summary>
        /// Only returns a positive value when a user was timeouted.
        /// </summary>
        public long TimeoutDuration { get; } = -1;

        public ClearChatTypes ClearChatType { get; private set; } = ClearChatTypes.RemovedAllMessages;

        public enum ClearChatTypes
        {
            RemovedAllMessages,
            UserBan,
            UserTimeout,
        }
        public string RawData { get; }
        public irc_clearchat(string rawdata)
        {
            this.RawData = rawdata;
            string[] splitdata = rawdata.Split(' ');
            string[] tags = splitdata[0].Remove(0, 1).Split(';');
            foreach (string tagraw in tags)
            {
                string[] tag = tagraw.Split('=');
                if (tag.Length >= 2)
                    switch (tag[0].ToLower())
                    {
                        case "ban-duration":
                            UpdateType(ClearChatTypes.UserTimeout);
                            try
                            {
                                TimeoutDuration = Convert.ToInt64(tag[1]);
                            }
                            catch { }
                            break;
                        case "room-id":
                            try
                            {
                                ChannelID = Convert.ToInt64(tag[1]);
                            }
                            catch { }
                            break;
                        case "target-user-id":
                            UpdateType(ClearChatTypes.UserBan);
                            try
                            {
                                TargetUserID = Convert.ToInt64(tag[1]);
                            }
                            catch { }
                            break;
                        case "tmi-sent-ts":
                            try
                            {
                                UnixTimestampMill = Convert.ToInt64(tag[1]);
                                TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(UnixTimestampMill).ToLocalTime().DateTime;
                            }
                            catch { }
                            break;
                    }
            }
            ChannelName = splitdata[3].Remove(0, 1);
            if(splitdata.Length >= 5)
                TargetUsername = splitdata[4].Remove(0,1);
        }
        private void UpdateType(ClearChatTypes type)
        {
            if (ClearChatType < type)
                ClearChatType = type;
        }
    }
}
