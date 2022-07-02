using System;
using System.Collections.Generic;
using System.Text;
using static P90Ez.Twitch.IRC.irc_CostumEventArgs;

namespace P90Ez.Twitch.IRC
{
    /// <summary>
    /// Contains all IRC/Chat Events.
    /// </summary>
    public class irc_events
    {
        #region IRC Events
        /// <summary>
        /// Will be triggered whenever a user joins a chatroom.
        /// </summary>
        public EventHandler<JoinPartArgs> OnChatJoin;
        /// <summary>
        /// Will be triggered whenever a user leaves a chatroom.
        /// </summary>
        public EventHandler<JoinPartArgs> OnChatLeave;
        /// <summary>
        /// Will be triggered whenever the full namelist is sent via IRC. (as an alternative, you can get the current list of users by calling the method 'GetUsersInChat')
        /// </summary>
        public EventHandler<NameListArgs> OnNameList;
        #endregion
        #region PRIVMSG Events
        /// <summary>
        /// Will be triggered whenever a PRIVSM is sent via IRC.
        /// </summary>
        public EventHandler<irc_privsmg> OnPRIVMSG;
        /// <summary>
        /// Will be triggered when a message is sent in chat.
        /// </summary>
        public EventHandler<irc_privsmg> OnChatMessage;
        /// <summary>
        /// Will be triggered when a user responds to a message in chat.
        /// </summary>
        public EventHandler<irc_privsmg> OnChatMessageReply;
        /// <summary>
        /// Will be triggered when a bitmessage is sent.
        /// </summary>
        public EventHandler<irc_privsmg> OnBitMessage;
        #endregion
        #region CLEARCHAT Events
        /// <summary>
        /// Will be triggered whenever a CLEARCHAT is sent via IRC. Clearchat will give you information about deleted messages (not all - use also CLEARMSG) and banned/timeouted users.
        /// </summary>
        public EventHandler<irc_clearchat> OnCLEARCHAT;
        /// <summary>
        /// Will be triggered when a user was banned from a chat.
        /// </summary>
        public EventHandler<irc_clearchat> UserBanned;
        /// <summary>
        /// Will be triggered when a user was timeouted from a chat.
        /// </summary>
        public EventHandler<irc_clearchat> UserTimeOut;
        /// <summary>
        /// Will be triggered when the whole chat was cleared.
        /// </summary>
        public EventHandler<irc_clearchat> ChatCleared;
        #endregion
        #region CLEARMSG Events
        /// <summary>
        /// Will be triggered whenever a CLEARMSG is sent via IRC. ClearMSG will give you information about single deleted messages.
        /// </summary>
        public EventHandler<irc_clearmsg> OnCLEARMSG;
        #endregion
        #region GLOBALUSERSTATE Events !!!!
        /// <summary>
        /// Will be triggered whenever a GLOBALUSERSTATE is sent via IRC.
        /// </summary>
        internal EventHandler<irc_globaluserstate> OnGLOBALUSERSTATE; //ÜBERARBEITEN!! und evaluieren wann getriggert
        #endregion
        #region NOTICE Events
        /// <summary>
        /// Will be triggered whenever a NOTICE is sent via IRC. This one is pretty raw, more excact events will come in the future. Here are 2 useful links to the documentation: https://dev.twitch.tv/docs/irc/msg-id, https://dev.twitch.tv/docs/irc/tags#globaluserstate-tags .
        /// </summary>
        public EventHandler<irc_notice> OnNOTICE;
        /// <summary>
        /// Will be triggered when this channel is hosting another broadcaster. 'NoticeMessage' will contain following: "Now hosting &lt;ChannelName&gt;."
        /// </summary>
        public EventHandler<HostArgs> OnHost;
        #endregion
        #region ROOMSTATE Events
        /// <summary>
        /// Will be triggered whenever a ROOMSTATE is sent via IRC.
        /// </summary>
        public  EventHandler<irc_roomstate> OnROOMSTATE;
        /// <summary>
        /// Will be triggered whenever EmoteOnly is switched on or off.
        /// </summary>
        public  EventHandler<irc_roomstate> EmoteOnlyTriggered;
        /// <summary>
        /// Will be triggered whenever FollowerOnly is switched on or off.
        /// </summary>
        public  EventHandler<irc_roomstate> FollowerOnlyTriggered;
        /// <summary>
        /// Will be triggered whenever SubOnly is switched on or off.
        /// </summary>
        public  EventHandler<irc_roomstate> SubOnlyTriggered;
        /// <summary>
        /// Will be triggered whenever UniqueMessageMode is switched on or off.
        /// </summary>
        public  EventHandler<irc_roomstate> UniqueMessageModeTriggered;
        /// <summary>
        /// Will be triggered whenever SlowMode is switched on or off.
        /// </summary>
        public  EventHandler<irc_roomstate> SlowModeTriggered;
        #endregion
        #region USERNOTICE Events
        /// <summary>
        /// Will be triggered whenever a USERNOTICE is sent via IRC.
        /// </summary>
        public EventHandler<irc_usernotice> OnUSERNOTICE;
        /// <summary>
        /// Will be triggered whenever a user subscribes for the FIRST time (also check out OnResub and OnSubgift or OnALLSub).
        /// </summary>
        public EventHandler<irc_usernotice> OnSub;
        /// <summary>
        /// Will be triggered whenever a user resubscribes. (also check out OnSub and OnSubgift or OnALLSub).
        /// </summary>
        public EventHandler<irc_usernotice> OnResub;
        /// <summary>
        /// Will be triggered whenever a user gifts (a) sub(s) (also check out OnSub and OnResub or OnALLSub).
        /// </summary>
        public EventHandler<irc_usernotice> OnSubgift;
        /// <summary>
        /// Will be triggered whenever a user subscribes, resubscribes, or gifts (a) sub(s).
        /// </summary>
        public EventHandler<irc_usernotice> OnALLSub;
        /// <summary>
        /// Will be triggered when a user sends his first message to this chat.
        /// </summary>
        public EventHandler<irc_usernotice> OnFirstChatMessage;
        /// <summary>
        /// Will be triggered when another broadcaster is raiding this stream.
        /// </summary>
        public EventHandler<irc_usernotice> OnRaid;
        /// <summary>
        /// Will be triggered when a user unlocks a new Bits Badge (Tier).
        /// </summary>
        public EventHandler<irc_usernotice> OnBitsBadgeTierUpdate;
        /// <summary>
        /// Will be triggered when a moderator/broadcaster announces something. (new feature?)
        /// </summary>
        public EventHandler<irc_usernotice> OnAnnouncement;
        #endregion
        #region USERSTATE Events
        /// <summary>
        /// Will be triggered whenever a USERSTATE is sent via IRC.
        /// </summary>
        public EventHandler<irc_userstate> OnUSERSTATE;
        #endregion
        #region WHISPER Events
        /// <summary>
        /// Will be triggered whenever a WHISPER is sent via IRC.
        /// </summary>
        public EventHandler<irc_whisper> OnWHISPER;
        #endregion
    }
}
