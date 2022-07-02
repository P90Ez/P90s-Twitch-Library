using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static P90Ez.Twitch.IRC.irc_Enums;
using static P90Ez.Twitch.IRC.irc_globaluserstate;

namespace P90Ez.Twitch.IRC
{
    public class irc_usernotice
    {
        public string Message { get; }
        public string MessageID { get; }
        /// <summary>
        /// The message Twitch shows in the chat room for this notice.
        /// </summary>
        public string SystemMessage { get; }
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
        public string ColorCode { get; private set; }
        public Permissionlevels Permissionlevel { get; private set; }
        public UserTypes UserType { get; private set; }
        public List<string> UsedEmotes { get; private set; } = new List<string>();
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
        /// <summary>
        /// Included only with sub and resub notices.
        /// </summary>
        public SubInfos SubInfo { get; private set; } = new SubInfos();
        /// <summary>
        /// Included only with subgift notices.
        /// </summary>
        public SubgiftInfos SubgiftInfo { get; private set; } = new SubgiftInfos();
        /// <summary>
        /// Included only with raid notices.
        /// </summary>
        public RaidInfos RaidInfo { get; private set; } = new RaidInfos();
        /// <summary>
        /// Included only with ritual notices.
        /// </summary>
        public RitualInfos RitualInfo { get; private set; } = new RitualInfos();
        /// <summary>
        /// Included only with bitbadgetier notices.
        /// </summary>
        public BitsBadgeTierInfos BitsBadgeTierInfo { get; private set; } = new BitsBadgeTierInfos();
        /// <summary>
        /// Included only with anongiftpaidupgrade and giftpaidupgrade notices.
        /// </summary>
        public GiftPaidUpgradeInfos GiftPaidUpgradeInfo { get; private set; } = new GiftPaidUpgradeInfos();
        public NoticeTypes NoticeType { get; set; }
        public enum NoticeTypes
        {
            sub,
            resub,
            subgift,
            giftpaidupgrade,
            rewardgift,
            anongiftpaidupgrade,
            raid,
            unraid,
            ritual,
            bitsbadgetier,
            announcement
        }

        public irc_usernotice(string rawdata)
        {
            //@badge-info=;badges=staff/1,broadcaster/1,turbo/1;color=#008000;display-name=ronni;emotes=;id=db25007f-7a18-43eb-9379-80131e44d633;login=ronni;mod=0;msg-id=resub;msg-param-cumulative-months=6;msg-param-streak-months=2;msg-param-should-share-streak=1;msg-param-sub-plan=Prime;msg-param-sub-plan-name=Prime;room-id=12345678;subscriber=1;system-msg=ronni\shas\ssubscribed\sfor\s6\smonths!;tmi-sent-ts=1507246572675;turbo=1;user-id=87654321;user-type=staff :tmi.twitch.tv USERNOTICE #dallas :Great stream -- keep it up!
            //@badge-info=;badges=staff/1,premium/1;color=#0000FF;display-name=TWW2;emotes=;id=e9176cd8-5e22-4684-ad40-ce53c2561c5e;login=tww2;mod=0;msg-id=subgift;msg-param-months=1;msg-param-recipient-display-name=Mr_Woodchuck;msg-param-recipient-id=55554444;msg-param-recipient-name=mr_woodchuck;msg-param-sub-plan-name=House\sof\sNyoro~n;msg-param-sub-plan=1000;room-id=12345678;subscriber=0;system-msg=TWW2\sgifted\sa\sTier\s1\ssub\sto\sMr_Woodchuck!;tmi-sent-ts=1521159445153;turbo=0;user-id=87654321;user-type=staff :tmi.twitch.tv USERNOTICE #forstycup
            RawData = rawdata;
            string[] splitdata = rawdata.Split(' ');
            if (splitdata.Length < 4) return;
            string[] tags = splitdata[0].Remove(0,1).Split(';');
            foreach(string rawtag in tags)
            {
                string[] tag = rawtag.Split('=');
                if(tag.Length <= 2)
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
                        case "emotes":
                            EmotesRaw = tag[1].Split(',').ToList();
                            foreach (string emoteRaw in EmotesRaw)
                            {
                                string emote = emoteRaw.Split(':')[0];
                                if (!UsedEmotes.Contains(emote))
                                    UsedEmotes.Add(emote);
                            }
                            break;
                        case "id":
                            MessageID = tag[1];
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
                        case "tmi-sent-ts":
                            MessageSentTimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(tag[1])).ToLocalTime().DateTime;
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
                        case "msg-id":
                            switch (tag[1].ToLower())
                            {
                                case "sub":
                                    NoticeType = NoticeTypes.sub;
                                    break;
                                case "resub":
                                    NoticeType = NoticeTypes.resub;
                                    break;
                                case "subgift":
                                    NoticeType = NoticeTypes.subgift;
                                    break;
                                case "giftpaidupgrade":
                                    NoticeType = NoticeTypes.giftpaidupgrade;
                                    break;
                                case "rewardgift":
                                    NoticeType = NoticeTypes.rewardgift;
                                    break;
                                case "anongiftpaidupgrade":
                                    NoticeType = NoticeTypes.anongiftpaidupgrade;
                                    break;
                                case "raid":
                                    NoticeType = NoticeTypes.raid;
                                    break;
                                case "unraid":
                                    NoticeType = NoticeTypes.unraid;
                                    break;
                                case "ritual":
                                    NoticeType = NoticeTypes.ritual;
                                    break;
                                case "bitsbadgetier":
                                    NoticeType = NoticeTypes.bitsbadgetier;
                                    break;
                                case "announcement":
                                    NoticeType = NoticeTypes.announcement;
                                    break;
                            }
                            break;
                        case "system-msg":
                            SystemMessage = tag[1];
                            break;
                        case "msg-param-cumulative-months":
                            try
                            {
                                SubInfo.CumulativeMonths = Convert.ToInt32(tag[1]);
                            }
                            catch { }
                            break;
                        case "msg-param-displayName":
                            RaidInfo.RaiderDisplayName = tag[1];
                            break;
                        case "msg-param-login":
                            RaidInfo.RaiderLogin = tag[1];
                            break;
                        case "msg-param-months":
                            try
                            {
                                SubgiftInfo.CumulativeMonths = Convert.ToInt32(tag[1]);
                            }
                            catch { }
                            break;
                        case "msg-param-promo-gift-total":
                            try
                            {
                                GiftPaidUpgradeInfo.PromoGiftTotal = Convert.ToInt32(tag[1]);
                            }
                            catch { }
                            break;
                        case "msg-param-promo-name":
                            GiftPaidUpgradeInfo.PromoName = tag[1];
                            break;
                        case "msg-param-recipient-display-name":
                            SubgiftInfo.RecipientDisplayName = tag[1];
                            break;
                        case "msg-param-recipient-id":
                            try
                            {
                                SubgiftInfo.RecipientUserID = Convert.ToInt32(tag[1]);
                            }
                            catch { }
                            break;
                        case "msg-param-recipient-user-name":
                            SubgiftInfo.RecipientUserName = tag[1];
                            break;
                        case "msg-param-sender-login":
                            GiftPaidUpgradeInfo.SenderLogin = tag[1];
                            break;
                        case "msg-param-sender-name":
                            GiftPaidUpgradeInfo.SenderName = tag[1];
                            break;
                        case "msg-param-should-share-streak":
                            if (tag[1] == "1")
                                SubInfo.ShouldShareStreak = true;
                            break;
                        case "msg-param-streak-months":
                            if(tag[1] != "0")
                                try
                                {
                                    SubInfo.StreakMonths = Convert.ToInt32(tag[1]);
                                }
                                catch { }
                            break;
                        case "msg-param-sub-plan":
                            SubInfo.SubPlan = tag[1];
                            SubgiftInfo.SubPlan = tag[1];
                            break;
                        case "msg-param-sub-plan-name":
                            SubInfo.SubPlanName = tag[1];
                            SubgiftInfo.SubPlanName = tag[1];
                            break;
                        case "msg-param-viewerCount":
                            try
                            {
                                RaidInfo.ViewCount = Convert.ToInt32(tag[1]);
                            }
                            catch { }
                            break;
                        case "msg-param-ritual-name":
                            RitualInfo.RitualName = tag[1];
                            break;
                        case "msg-param-threshold":
                            try
                            {
                                BitsBadgeTierInfo.Tier = Convert.ToInt32(tag[1]);
                            }
                            catch { }
                            break;
                        case "msg-param-gift-months":
                            try
                            {
                                SubgiftInfo.GiftMonths = Convert.ToInt32(tag[1]);
                            }
                            catch { }
                            break;
                    }
            }
            ChannelName = splitdata[3].Remove(0, 1);
            for (int i = 4; i < splitdata.Length; i++)
            {
                if (i == 4)
                    Message = splitdata[i].Remove(0, 1);
                else
                    Message += " " + splitdata[i];
            }
        }
        /// <summary>
        /// Included only with raid notices.
        /// </summary>
        public class RaidInfos
        {
            /// <summary>
            /// The number of viewers raiding this channel from the broadcaster’s channel.
            /// </summary>
            public int ViewCount { get; internal set; }
            /// <summary>
            /// The login name of the broadcaster raiding this channel.
            /// </summary>
            public string RaiderLogin { get; internal set; }
            /// <summary>
            /// The display name of the broadcaster raiding this channel.
            /// </summary>
            public string RaiderDisplayName { get; internal set; }
        }
        /// <summary>
        /// Included only with sub and resub notices.
        /// </summary>
        public class SubInfos
        {
            /// <summary>
            /// The display name of the subscription plan.This may be a default name or one created by the channel owner.
            /// </summary>
            public string SubPlanName { get; internal set; }
            /// <summary>
            /// The type of subscription plan being used. Possible values are: Prime — Amazon Prime subscription, 1000 — First level of paid subscription, 2000 — Second level of paid subscription, 3000 — Third level of paid subscription
            /// </summary>
            public string SubPlan { get; internal set; }
            /// <summary>
            /// A Boolean value that indicates whether the user wants their streaks shared.
            /// </summary>
            public bool ShouldShareStreak { get; internal set; } = false;
            /// <summary>
            /// The number of consecutive months the user has subscribed. Only returns a positive value if 'ShouldShareStreak' is true.
            /// </summary>
            public int StreakMonths { get; internal set; } = -1;
            /// <summary>
            /// The total number of months the user has subscribed.
            /// </summary>
            public int CumulativeMonths { get; internal set; }
        }
        /// <summary>
        /// Included only with subgift notices.
        /// </summary>
        public class SubgiftInfos
        {
            /// <summary>
            /// The number of months gifted as part of a single, multi-month gift.
            /// </summary>
            public int GiftMonths { get; internal set; }
            /// <summary>
            /// The display name of the subscription plan.This may be a default name or one created by the channel owner.
            /// </summary>
            public string SubPlanName { get; internal set; }
            /// <summary>
            /// The type of subscription plan being used. Possible values are: Prime — Amazon Prime subscription, 1000 — First level of paid subscription, 2000 — Second level of paid subscription, 3000 — Third level of paid subscription
            /// </summary>
            public string SubPlan { get; internal set; }
            /// <summary>
            /// The user name of the subscription gift recipient.
            /// </summary>
            public string RecipientUserName { get; internal set; }
            /// <summary>
            /// The user ID of the subscription gift recipient.
            /// </summary>
            public long RecipientUserID { get; internal set; }
            /// <summary>
            /// The display name of the subscription gift recipient.
            /// </summary>
            public string RecipientDisplayName { get; internal set; }
            /// <summary>
            /// The total number of months the user has subscribed. 
            /// /// </summary>
            public int CumulativeMonths { get; internal set; }
        }
        /// <summary>
        /// Included only with ritual notices.
        /// </summary>
        public class RitualInfos
        {
            /// <summary>
            /// The name of the ritual being celebrated. Possible values are (for now): new_chatter
            /// </summary>
            public string RitualName { get; internal set; }
        }
        /// <summary>
        /// Included only with bitsbadgetier notices.
        /// </summary>
        public class BitsBadgeTierInfos
        {
            /// <summary>
            /// The tier of the Bits badge the user just earned. For example, 100, 1000, or 10000.
            /// </summary>
            public int Tier { get; internal set; }  
        }
        /// <summary>
        /// Included only with anongiftpaidupgrade and giftpaidupgrade notices.
        /// </summary>
        public class GiftPaidUpgradeInfos
        {
            /// <summary>
            /// The display name of the user who gifted the subscription.
            /// </summary>
            public string SenderName { get; internal set; }
            /// <summary>
            /// The login name of the user who gifted the subscription.
            /// </summary>
            public string SenderLogin { get; internal set; }
            /// <summary>
            /// The subscriptions promo, if any, that is ongoing (for example, Subtember 2018).
            /// </summary>
            public string PromoName { get; internal set; }
            /// <summary>
            /// The number of gifts the gifter has given during the promo indicated by 'PromoName'.
            /// </summary>
            public int PromoGiftTotal { get; internal set; }
        }
        private void UpdatePermissionlevel(Permissionlevels level)
        {
            if (Permissionlevel < level)
                Permissionlevel = level;
        }
    }
}
