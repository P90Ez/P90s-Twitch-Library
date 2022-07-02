using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.IRC
{
    public class irc_notice
    {
        public string RawData { get; }
        /// <summary>
        /// You can look up the specifics here: https://dev.twitch.tv/docs/irc/msg-id
        /// </summary>
        public string msg_id { get; }
        /// <summary>
        /// Only returns a positive number if set. Some msg-id's don't provide a target-user-id.
        /// </summary>
        public long TargetUserID { get; } = -1;
        public string ChannelName { get; }
        /// <summary>
        /// Additional information in different formats. You can look up the specifics here: https://dev.twitch.tv/docs/irc/msg-id
        /// </summary>
        public string NoticeMessage { get; }
        public irc_notice(string rawdata)
        {   //                  0                                   1               2   3       4+
            //@msg-id=whisper_restricted;target-user-id=12345678 :tmi.twitch.tv NOTICE #bar :Your settings prevent you from sending this whisper.
            //@msg-id=delete_message_success :tmi.twitch.tv NOTICE #bar :The message from foo is now deleted.
            RawData = rawdata;
            string[] splitdata = rawdata.Split(' ');
            if (splitdata.Length < 4) return;
            string[] tags = splitdata[0].Remove(0, 1).Split(';');
            foreach(string rawtag in tags)
            {
                string[] tag = rawtag.Split('=');
                if(tag.Length >= 2)
                    switch (tag[0].ToLower())
                    {
                        case "msg-id":
                            msg_id = tag[1];
                            break;
                        case "target-user-id":
                            try
                            {
                                TargetUserID = Convert.ToInt64(tag[1]);
                            }
                            catch { }
                            break;
                    }
            }
            ChannelName = splitdata[3].Remove(0,1);
            for(int i = 4; i<splitdata.Length; i++)
            {
                if (i == 4)
                    NoticeMessage = splitdata[i].Remove(0, 1);
                else
                    NoticeMessage += " " + splitdata[i];
            }
        }
    }
}
