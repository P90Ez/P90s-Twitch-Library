using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static P90Ez.Twitch.IRC.irc_Enums;

namespace P90Ez.Twitch.IRC
{
    public class irc_whisper
    {
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
        public string Displayname
        {
            get
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
        public string Message { get; }
        /// <summary>
        /// An ID that uniquely identifies the whisper message.
        /// </summary>
        public string MessageID { get; }
        /// <summary>
        /// An ID that uniquely identifies the whisper thread. The ID is in the form, &lt;smaller-value-user-id&gt;_&lt;larger-value-user-id&gt;.
        /// </summary>
        public string ThreadID { get; }
        public string ColorCode { get; private set; }
        public UserTypes UserType { get; private set; }
        public bool HasTwitchTurbo { get; private set; } = false;
        /// <summary>
        ///  List of chat badges in the form, &lt;badge&gt;/&lt;version&gt;. For example, admin/1.
        /// </summary>
        public List<string> BadgesRaw { get; private set; }
        public List<string> EmotesRaw { get; private set; }
        public List<string> UsedEmotes { get; private set; } = new List<string>();
        public string RawData { get; }
        public irc_whisper(string rawdata)
        {
            //@badges=staff/1,bits-charity/1;color=#8A2BE2;display-name=PetsgomOO;emotes=;message-id=306;thread-id=12345678_87654321;turbo=0;user-id=87654321;user-type=staff :petsgomoo!petsgomoo@petsgomoo.tmi.twitch.tv WHISPER foo :hello
            RawData = rawdata;
            string[] splitdata = rawdata.Split(' ');
            if (splitdata.Length < 4) return;
            string[] tags = splitdata[0].Remove(0, 1).Split(';');
            foreach (string rawtag in tags)
            {
                string[] tag = rawtag.Split('=');
                if (tag.Length <= 2)
                    switch (tag[0].ToLower())
                    {
                        case "badges":
                            BadgesRaw = tag[1].Split(',').ToList();
                            foreach (string badgeraw in BadgesRaw)
                            {
                                string badge = badgeraw.Split('/')[0];
                            }
                            break;
                        case "color":
                            ColorCode = tag[1];
                            break;
                        case "display-name":
                            Displayname = tag[1];
                            break;
                        case "emotes":
                            EmotesRaw = tag[1].Split(',').ToList();
                            foreach (string emoteRaw in EmotesRaw)
                            {
                                string emote = emoteRaw.Split(':')[0];
                                if (!UsedEmotes.Contains(emote))
                                    UsedEmotes.Add(emote);
                            }
                            break;
                        case "turbo":
                            if (tag[1] == "1")
                                HasTwitchTurbo = true;
                            break;
                        case "user-id":
                            try
                            {
                                UserID = Convert.ToInt64(tag[1]);
                            }
                            catch { }
                            break;
                        case "user-type":
                            switch (tag[1].ToLower())
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
                            _Username = tag[1];
                            break;
                        case "message-id":
                            MessageID = tag[1];
                            break;
                        case "thread-id":
                            ThreadID = tag[1];
                            break;
                    }
            }
            for (int i = 4; i < splitdata.Length; i++)
            {
                if (i == 4)
                    Message = splitdata[i].Remove(0, 1);
                else
                    Message += " " + splitdata[i];
            }
        }
    }
}
