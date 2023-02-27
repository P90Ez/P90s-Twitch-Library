using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static P90Ez.Twitch.IRC.irc_Enums;
using static P90Ez.Twitch.IRC.irc_globaluserstate;

namespace P90Ez.Twitch.IRC
{
    public class irc_privsmg
    {
        /// <summary>
        /// Chat Message
        /// </summary>
        public string Message { get; }
        public string MessageID { get; private set; }
        /// <summary>
        /// Only set if this message is a reply to another message
        /// </summary>
        public string ParentMessageID { get; private set; }
        /// <summary>
        /// Only set if this message is a reply to another message
        /// </summary>
        public string ParentMessageUsername { get; private set; }
        private string _ParentMessageDisplayName = "";
        /// <summary>
        /// Only set if this message is a reply to another message
        /// </summary>
        public string ParentMessageDisplayName
        {
            get
            {
                if (Displayname == "")
                    return ParentMessageUsername;
                else
                    return _ParentMessageDisplayName;
            }
            private set
            {
                _ParentMessageDisplayName = value;
            }
        }
        /// <summary>
        /// Only set if this message is a reply to another message
        /// </summary>
        public long ParentMessageUserID { get; private set; }
        /// <summary>
        /// Only set if this message is a reply to another message
        /// </summary>
        public string ParentMessage { get; private set; }
        public long UserID { get; private set; }
        private string _Username = "";
        /// <summary>
        /// Account name, can differ from Displayname (most of the time Username is Displayname in lowercase)
        /// </summary>
        public string Username { get { if (_Username != "") return _Username.ToLower(); else return _Displayname.ToLower(); } }

        private string _Displayname = "";
        /// <summary>
        /// Displayed name, can differ from Username
        /// </summary>
        public string Displayname { get
            {
                if (_Displayname == "")
                    return _Username;
                else
                    return _Displayname;
            }
            private set 
            {
                _Displayname = value;
            }
        }
        public string ColorCode { get; private set; }
        private string[] _Command;
        /// <summary>
        /// 'Command', split every ' ' (Space)
        /// </summary>
        public string[] Command { get 
            {
                if(_Command != null)
                    return _Command;
                if (!Message.Contains(" "))
                    return (_Command = new string[1] {Message});
                return _Command = Message.Split(' ');
            } }
        public Permissionlevels Permissionlevel { get; private set; }
        public bool IsSubscriber { get; private set; } = false;
        /// <summary>
        /// Only returns a positive value when user is currently a subscriber.
        /// </summary>
        public int SubscribedMonths { get; private set; } = -1;
        public bool IsMod { get; private set; } = false;
        /// <summary>
        /// Only readable if broadcaster badge is visible
        /// </summary>
        public bool IsBroadcaster { get; private set; } = false;
        /// <summary>
        /// Only readable if VIP badge is visible
        /// </summary>
        public bool IsVip { get; private set; } = false; 
        /// <summary>
        /// Only readable if Prime badge is visible
        /// </summary>
        public bool HasTwitchPrime { get; private set; } = false;
        public bool HasTwitchTurbo { get; private set; } = false;
        public string ChannelName { get; }
        public long ChannelID { get; private set; }
        public DateTime MessageSentTimeStamp { get; private set; }
        /// <summary>
        /// Raw IRC data
        /// </summary>
        public string RawData { get; }
        /// <summary>
        ///  List of chat badges in the form, &lt;badge&gt;/&lt;version&gt;. For example, admin/1.
        /// </summary>
        public List<string> BadgesRaw { get; private set; }
        public List<string> EmotesRaw { get; private set; }
        public List<string> UsedEmotes { get; private set; } = new List<string>();
        public int BitsAmount { get; private set; } = 0;
        public UserTypes UserType { get; private set; }
        //MessageType
        public bool IsBitMessage { get; private set; } = false;
        public bool IsChatMessage { get; private set; } = true;
        public bool IsChatMessageReply { get; private set; } = false;   
        private Controller parentController { get; }

        public irc_privsmg(string rawdata, Controller parentController)
        {
            RawData = rawdata;
            this.parentController = parentController;
            string[] splitdata = rawdata.Split(' ');
            if (splitdata.Length < 5) return;
            ParseUsefulData(splitdata[0]);
            //              0                                                                                                                                                                                                                                                               
            //@badge-info=subscriber/30;badges=broadcaster/1,subscriber/0,premium/1;color=#FF9138;display-name=P90Ez;emotes=;first-msg=0;flags=;id=cdbc5699-6080-4787-a11d-18503d8318ba;mod=0;room-id=196174120;subscriber=1;tmi-sent-ts=1655204676025;turbo=0;user-id=196174120;user-type=
            //        1                           2        3      4       
            //:p90ez!p90ez@p90ez.tmi.twitch.tv PRIVMSG #p90ez :test 123
            ChannelName = splitdata[3].Remove(0, 1).ToLower();
            for(int i = 4;i<splitdata.Length;i++)
            {
                if (i == 4)
                    Message = splitdata[i].Remove(0, 1);
                else
                    Message += " " + splitdata[i];
            }
        }
        private void ParseUsefulData(string data)
        {   //input example: (more here: https://dev.twitch.tv/docs/irc/tags - PRIVMSG)
            //@badge-info=subscriber/30;badges=broadcaster/1,subscriber/0,premium/1;color=#FF9138;display-name=P90Ez;emotes=;first-msg=0;flags=;id=cdbc5699-6080-4787-a11d-18503d8318ba;mod=0;room-id=196174120;subscriber=1;tmi-sent-ts=1655204676025;turbo=0;user-id=196174120;user-type=
            data = data.Remove(0, 1);
            foreach(string d in data.Split(';'))
            {
                string[] splitd = d.Split('=');
                if(splitd.Length >= 2)
                {
                    switch (splitd[0].ToLower())
                    {
                        case "badge-info":
                            if (splitd[1].Contains("subscriber"))
                            {
                                string[] dd = splitd[1].Split('/');
                                if(dd.Length >= 2)
                                    for(int i = 0; i < dd.Length-1; i++)
                                    {
                                        if (dd[i] == "subscriber")
                                            try
                                            {
                                                SubscribedMonths = Convert.ToInt32(dd[i + 1]);
                                            }
                                            catch { }
                                    }
                            }
                            break;
                        case "badges":
                            BadgesRaw = splitd[1].Split(',').ToList();
                            foreach(string badgeraw in BadgesRaw)
                            {
                                string badge = badgeraw.Split('/')[0];
                                switch (badge)
                                {
                                    case "broadcaster":
                                        UpdatePermissionlevel(Permissionlevels.Streamer);
                                        IsBroadcaster = true;
                                        IsMod = true;
                                        break;
                                    case "moderator":
                                        UpdatePermissionlevel(Permissionlevels.Mod);
                                        IsMod = true;
                                        break;
                                    case "subscriber":
                                        IsSubscriber = true;
                                        UpdatePermissionlevel(Permissionlevels.Sub);
                                        break;
                                    case "vip":
                                        IsVip = true;
                                        UpdatePermissionlevel(Permissionlevels.Vip);
                                        break;
                                    case "premium":
                                        HasTwitchPrime = true;
                                        break;
                                }
                            }
                            break;
                        case "color":
                            ColorCode = splitd[1];
                            break;
                        case "display-name":
                            Displayname = splitd[1];
                            break;
                        case "emotes":
                            EmotesRaw = splitd[1].Split(',').ToList();
                            foreach(string emoteRaw in EmotesRaw)
                            {
                                string emote = emoteRaw.Split(':')[0];
                                if (!UsedEmotes.Contains(emote))
                                    UsedEmotes.Add(emote);
                            }
                            break;
                        case "first-msg": //not implemented
                            break;
                        case "flags": //not implemented
                            break;
                        case "id":
                            MessageID = splitd[1];
                            break;
                        case "mod":
                            if (splitd[1] == "1")
                            {
                                IsMod = true;
                                UpdatePermissionlevel(Permissionlevels.Mod);
                            }
                            break;
                        case "room-id":
                            ChannelID = Convert.ToInt64(splitd[1]);
                            break;
                        case "subscriber":
                            if(splitd[1] == "1")
                            {
                                IsSubscriber = true;
                                UpdatePermissionlevel(Permissionlevels.Sub);
                            }
                            break;
                        case "tmi-sent-ts":
                            MessageSentTimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(splitd[1])).ToLocalTime().DateTime;
                            break;
                        case "turbo":
                            if (splitd[1] == "1")
                                HasTwitchTurbo = true;
                            break;
                        case "user-id":
                            try
                            {
                                UserID = Convert.ToInt64(splitd[1]);
                            }
                            catch { }
                            break;
                        case "user-type":
                            switch (splitd[1].ToLower())
                            {
                                case "admin":
                                    UserType = UserTypes.TwitchAdmin;
                                    break;
                                case "global_mod":
                                    UserType = UserTypes.GlobalMod;
                                    break;
                                case "staff":
                                    UserType = UserTypes.TwitchStaff;
                                    break;
                                default:
                                    UserType = UserTypes.NormalUser;
                                    break;
                            }
                            break;
                        case "login":
                            _Username = splitd[1];
                            break;
                        case "bits":
                            IsBitMessage = true;
                            try
                            {
                                BitsAmount = Convert.ToInt32(splitd[1]);
                            }
                            catch { }
                            break;
                        case "reply-parent-msg-id":
                            IsChatMessageReply = true;
                            ParentMessageID = splitd[1];
                            break;
                        case "reply-parent-user-id":
                            try
                            {
                                ParentMessageUserID = Convert.ToInt64(splitd[1]);
                            }catch{ }
                            break;
                        case "reply-parent-user-login":
                            ParentMessageUsername = splitd[1];
                            break;
                        case "reply-parent-display-name":
                            ParentMessageDisplayName = splitd[1];
                            break;
                        case "reply-parent-msg-body":
                            ParentMessage = splitd[1];
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Deletes this message from chat. The bot needs permission to perform this action! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void DeleteMessageFromChat()
        {
            if (Permissionlevel < Permissionlevels.Mod)
                parentController.DeleteMessageFromChat(MessageID); //dev note: replaced deprecated functions with new (API endpoint) functions - no channelname required -> might be a problem in the future (multiple chats with 1 controller)
        }
        /// <summary>
        /// Timeouts a user from this chat. The bot needs permission to perform this action! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void TimeOutMessageSender(int seconds, string reason = "")
        {
            if (Permissionlevel < Permissionlevels.Mod && UserType == UserTypes.NormalUser)
                parentController.TimeoutUser(UserID, seconds, reason);
        }
        /// <summary>
        /// Bans a user from this chat. The bot needs permission to perform this action! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void BanMessageSender(string reason = "")
        {
            if (Permissionlevel < Permissionlevels.Mod && UserType == UserTypes.NormalUser)
                parentController.BanUser(UserID, reason);
        }
        /// <summary>
        /// Replys directly to this message.
        /// </summary>
        /// <param name="message"></param>
        public void Reply(string message)
        {
            if (ParentMessageID == null)
                parentController.ReplyChatWriter(message, MessageID, ChannelName);
            else
                parentController.ReplyChatWriter(message, ParentMessageID, ChannelName);
        }
        /// <summary>
        /// Send a message to the chat without replying directly to the original message.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            parentController.ChatWriter(message, ChannelName);
        }
        private void UpdatePermissionlevel(Permissionlevels level)
        {
            if (Permissionlevel < level)
                Permissionlevel = level;
        }

        
    }
}
