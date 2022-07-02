using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.IRC
{
    public class irc_clearmsg
    {
        public string Message { get; }
        /// <summary>
        /// Username in lowercase.
        /// </summary>
        public string TargetUsername { get; }
        public string Channel { get; }
        /// <summary>
        /// OPTIONAL! Use "Channel" instead. (Will only return a positive value if set)
        /// </summary>
        public long ChannelID { get; } = -1;
        public string MessageID { get; }
        public DateTime TimeStamp { get; } 
        private string rawdata { get; }
        public irc_clearmsg(string rawdata)
        {
            this.rawdata = rawdata;
            //          0                                                                   1           2           3       4
            //@login=ronni;room-id=;target-msg-id=abc-123-def;tmi-sent-ts=1642720582342 :tmi.twitch.tv CLEARMSG #dallas :HeyGuys
            string[] splitdata = rawdata.Split(' ');
            Channel = splitdata[3].Remove(0,1);
            for(int i = 4; i < splitdata.Length; i++)
            {
                if (i == 4)
                    Message = splitdata[i].Remove(0,1);
                else
                    Message += " " + splitdata[i];
            }
            string[] tags = splitdata[0].Remove(0, 1).Split(';');
            foreach(string tag in tags)
            {
                string[] splittag = tag.Split('=');
                if(splittag.Length >= 2)
                    switch (splittag[0].ToLower())
                    {
                        case "login":
                            TargetUsername = splittag[1].ToLower();
                            break;
                        case "room-id":
                            try
                            {
                                ChannelID = Convert.ToInt64(splittag[1]);
                            }
                            catch { }
                            break;
                        case "target-msg-id":
                            MessageID = splittag[1];
                            break;
                        case "tmi-sent-ts":
                            try
                            {
                                TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(splittag[1])).ToLocalTime().DateTime;
                            }catch { }
                            break;
                        default: break;
                    }
            }
        }
    }
}
