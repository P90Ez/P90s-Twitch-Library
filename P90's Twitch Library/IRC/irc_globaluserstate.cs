using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static P90Ez.Twitch.IRC.irc_Enums;

namespace P90Ez.Twitch.IRC
{
    public class irc_globaluserstate
    {
        public string Displayname { get; }
        public long UserID { get; }
        /// <summary>
        /// A list of IDs that identify the emote sets that the user has access to.
        /// </summary>
        public List<string> EmoteSets { get; }
        public string ColorCode { get; }
        /// <summary>
        /// Not 100% accurate because Globaluserstate has less tags than PrivMSG.
        /// </summary>
        public Permissionlevels Permissionlevel { get; private set; }
        /// <summary>
        /// Only readable if sub-badge is visible.
        /// </summary>
        public bool IsSubscriber { get; private set; } = false;
        /// <summary>
        /// Only returns a positive value when user is currently a subscriber.
        /// </summary>
        public int SubscribedMonths { get; private set; } = -1;
        /// <summary>
        /// Only readable if mod-badge is visible.
        /// </summary>
        public bool IsMod { get; private set; } = false;
        /// <summary>
        /// Only readable if broadcaster-badge is visible
        /// </summary>
        public bool IsBroadcaster { get; private set; } = false;
        /// <summary>
        /// Only readable if VIP-badge is visible
        /// </summary>
        public bool IsVip { get; private set; } = false;
        /// <summary>
        /// Only readable if Prime-badge is visible
        /// </summary>
        public bool HasTwitchPrime { get; private set; } = false;
        public bool HasTwitchTurbo { get; private set; } = false;
        public UserTypes UserType { get; private set; }
        public List<string> BadgesRaw { get;}
        private string rawdata { get; }
        //@badge-info=subscriber/8;badges=subscriber/6;color=#0D4200;display-name=dallas;emote-sets=0,33,50,237,793,2126,3517,4578,5569,9400,10337,12239;turbo=0;user-id=12345678;user-type=admin :tmi.twitch.tv GLOBALUSERSTATE
        
        public irc_globaluserstate(string rawdata)
        {
            this.rawdata = rawdata;
            string[] tags = rawdata.Split(' ')[0].Remove(0, 1).Split(';');
            foreach(string tag in tags)
            {
                string[] splittag = tag.Split('=');
                if(splittag.Length >= 2)
                    switch (splittag[0].ToLower())
                    {
                        case "badge-info":
                            if (splittag[1].Contains("subscriber"))
                            {
                                string[] dd = splittag[1].Split('/');
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
                            BadgesRaw = splittag[1].Split(',').ToList();
                            foreach (string badgeraw in BadgesRaw)
                            {
                                string badge = badgeraw.Split('/')[0];
                                switch (badge)
                                {
                                    case "broadcaster":
                                        UpdatePermissionlevel(Permissionlevels.Streamer);
                                        IsBroadcaster = true;
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
                            ColorCode = splittag[1];
                            break;
                        case "display-name":
                            Displayname = splittag[1];
                            break;
                        case "user-id":
                            try
                            {
                                UserID = Convert.ToInt64(splittag[1]);
                            }
                            catch { }
                            break;
                        case "subscriber":
                            if (splittag[1] == "1")
                            {
                                IsSubscriber = true;
                                UpdatePermissionlevel(Permissionlevels.Sub);
                            }
                            break;
                        case "turbo":
                            if (splittag[1] == "1")
                                HasTwitchTurbo = true;
                            break;
                        case "emote-sets":
                            EmoteSets = splittag[1].Split(',').ToList();
                            break;
                        case "user-type":
                            switch (splittag[1].ToLower())
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
                    }
            }
        }

        private void UpdatePermissionlevel(Permissionlevels level)
        {
            if (Permissionlevel < level)
                Permissionlevel = level;
        }
    }
}
