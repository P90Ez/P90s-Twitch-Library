using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.IRC
{
    public class irc_roomstate
    {
        /// <summary>
        /// A Boolean value that determines whether the chat room allows only messages with emotes.
        /// </summary>
        public bool EmoteOnly { get; } = false;
        /// <summary>
        /// A Boolean value that determines whether only followers can post messages in the chat room.
        /// </summary>
        public bool FollowerOnly { get; } = false;
        /// <summary>
        /// If FollowerOnly is activated, this int contains the time (in min) how long you have to be follower before you'll be able to chat.
        /// </summary>
        public int RequieredFollowDuration { get; } = -1;
        /// <summary>
        /// A Boolean value that determines whether only subscribers and moderators can chat in the chat room.
        /// </summary>
        public bool SubOnly { get; } = false;
        /// <summary>
        /// A Boolean value that determines wheter users must wait between sending messages.
        /// </summary>
        public bool SlowMode { get; } = false;
        /// <summary>
        /// If SlowMode is activated, this in contains the time (in sec) before you can send another message.
        /// </summary>
        public int SlowDuration { get; } = -1;
        /// <summary>
        /// A Boolean value that determines whether a user’s messages must be unique. Applies only to messages with more than 9 characters.
        /// </summary>
        public bool UniqueChatMode { get; } = false;
        public long ChannelID { get; }
        public string ChannelName { get; }
        public string RawData { get; }
        private Controller parentController { get; }
        public irc_roomstate(string rawdata, Controller parentController)
        {   //                          0                                   1           2           3
            //@emote-only=0;followers-only=0;r9k=0;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #dallas
            this.parentController = parentController;
            RawData = rawdata;  
            string[] splitdata = rawdata.Split(' ');
            if (splitdata.Length < 4) return;
            string[] tags = splitdata[0].Remove(0,1).Split(';');
            foreach(string rawtag in tags)
            {
                string[] tag = rawtag.Split('=');
                if(tag.Length >= 2)
                    switch (tag[0].ToLower())
                    {
                        case "emote-only":
                            if(tag[1] == "1")
                                EmoteOnly = true;
                            break;
                        case "followers-only":
                            if(tag[1] != "-1")
                                try
                                {
                                    RequieredFollowDuration = Convert.ToInt32(tag[1]);
                                    if(RequieredFollowDuration > 0)
                                        FollowerOnly = true;
                                }
                                catch { }
                            break;
                        case "r9k":
                            if(tag[1] == "1")
                                UniqueChatMode = true;
                            break;
                        case "room-id":
                            try
                            {
                                ChannelID = Convert.ToInt64(tag[1]);
                            }
                            catch { }
                            break;
                        case "slow":
                            try
                            {
                                SlowDuration = Convert.ToInt32(tag[1]);
                                if (SlowDuration > 1)
                                    SlowMode = true;
                            }
                            catch { }
                            break;
                        case "subs-only":
                            if(tag[1] == "1")
                                SubOnly = true;
                            break;
                    }
            }
            ChannelName = splitdata[3].Remove(0, 1);
        }

        /// <summary>
        /// Restricts users to posting chat messages that contain only emoticons. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetEmoteOnly()
        {
            parentController.EmoteOnly(ChannelName);
        }
        /// <summary>
        /// Removes EmoteOnly restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetEmoteOnlyOff()
        {
            parentController.EmoteOnlyOff(ChannelName);
        }
        /// <summary>
        /// Restricts who can post chat messages to followers only. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="FollowTime">minimum length of time following channel in minutes</param>
        public void SetFollowersOnly(int FollowTime = 0)
        {
            parentController.FollowersOnly(ChannelName, FollowTime);
        }
        /// <summary>
        /// Removes FollowersOnly restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetFollowersOnlyOff()
        {
            parentController.FollowersOnlyOff(ChannelName);
        }
        /// <summary>
        /// Restricts how often users can post messages. This sets the minimum time, in seconds, that a user must wait before being allowed to post another message. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="waittime">minimum wait time in seconds</param>
        public void SetSlowMode(int waittime)
        {
            parentController.SlowMode(ChannelName, waittime);
        }
        /// <summary>
        /// Removes SlowMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetSlowModeOff()
        {
            parentController.SlowModeOff(ChannelName);
        }
        /// <summary>
        /// Restricts who can post chat messages to subscribers only. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetSubOnlyMode()
        {
            parentController.SubOnlyMode(ChannelName);
        }
        /// <summary>
        /// Removes SubOnlyMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetSubOnlyModeOff()
        {
            parentController.SubOnlyModeOff(ChannelName);
        }
        /// <summary>
        /// Restricts a user’s chat messages to unique messages only; a user cannot send duplicate chat messages. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetUniqueChatMode()
        {
            parentController.UniqueChatMode(ChannelName);
        }
        /// <summary>
        /// Removes UniqueChatMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetUniqueChatModeOff()
        {
            parentController.UniqueChatModeOff(ChannelName);
        }
    }
}
