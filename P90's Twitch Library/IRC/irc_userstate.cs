using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static P90Ez.Twitch.IRC.irc_Enums;

namespace P90Ez.Twitch.IRC
{
    public class irc_userstate
    {
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
        public string ColorCode { get; private set; }
        public Permissionlevels Permissionlevel { get; private set; }
        public UserTypes UserType { get; private set; }
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
        /// <summary>
        /// Raw IRC data
        /// </summary>
        public string RawData { get; }
        /// <summary>
        ///  List of chat badges in the form, &lt;badge&gt;/&lt;version&gt;. For example, admin/1.
        /// </summary>
        public List<string> BadgesRaw { get; private set; }
        /// <summary>
        /// A list of IDs that identify the emote sets that the user has access to.
        /// </summary>
        public List<string> EmoteSets { get; }
        public irc_userstate(string rawdata)
        {
            //@badge-info=;badges=staff/1;color=#0D4200;display-name=ronni;emote-sets=0,33,50,237,793,2126,3517,4578,5569,9400,10337,12239;mod=1;subscriber=1;turbo=1;user-type=staff :tmi.twitch.tv USERSTATE #dallas
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
                        case "badge-info":
                            if (tag[1].Contains("subscriber"))
                            {
                                string[] dd = tag[1].Split('/');
                                if (dd.Length >= 2)
                                    for (int i = 0; i < dd.Length - 1; i++)
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
                            BadgesRaw = tag[1].Split(',').ToList();
                            foreach (string badgeraw in BadgesRaw)
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
                            ColorCode = tag[1];
                            break;
                        case "display-name":
                            Displayname = tag[1];
                            break;
                        case "mod":
                            if (tag[1] == "1")
                            {
                                IsMod = true;
                                UpdatePermissionlevel(Permissionlevels.Mod);
                            }
                            break;
                        case "room-id":
                            ChannelID = Convert.ToInt64(tag[1]);
                            break;
                        case "subscriber":
                            if (tag[1] == "1")
                            {
                                IsSubscriber = true;
                                UpdatePermissionlevel(Permissionlevels.Sub);
                            }
                            break;
                        case "turbo":
                            if (tag[1] == "1")
                                HasTwitchTurbo = true;
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
                        case "emote-sets":
                            EmoteSets = tag[1].Split(',').ToList();
                            break;
                    }
            }
            ChannelName = splitdata[3].Remove(0, 1);
        }
        private void UpdatePermissionlevel(Permissionlevels level)
        {
            if (Permissionlevel < level)
                Permissionlevel = level;
        }
    }
}
