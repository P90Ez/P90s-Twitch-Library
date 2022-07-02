using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static P90Ez.Twitch.IRC.irc_usernotice;

namespace P90Ez.Twitch.IRC
{
    public class Controller
    {
        #region Basic vars
        /// <summary>
        /// Bot's username.
        /// </summary>
        public string Nick { get; }
        /// <summary>
        /// Bot's OAUTH token.
        /// </summary>
        private protected string OAuth { get; }
        /// <summary>
        /// Channelname, which the irc stream will be attached to.
        /// </summary>
        public string Channel { get; }
        private const string _server = "irc.chat.twitch.tv";
        private const int _port = 6667;
        private NetworkStream _stream;
        private TcpClient _irc;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cancelToken;
        /// <summary>
        /// Set to True to Display IRC Message in the Console
        /// </summary>
        public bool Debug = false;
        /// <summary>
        /// Let's the bot ignore it's own messages. (only applies to following events: OnPRIVMSG, OnChatMessage, OnChatMessageReply, OnBitMessage)
        /// </summary>
        public bool IgnoreMessagesFromThisBot = true;
        /// <summary>
        /// Contains all Events from IRC/Chat.
        /// </summary>
        public irc_events Events = new irc_events();
        #endregion
        #region Instanciation
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="nick">Bot Username</param>
        /// <param name="oath">Bot OAuth Token (important: starting with "OAUTH:...")</param>
        /// <param name="channel">Twitch Channelname</param>
        public Controller(string nick, string oath, string channel)
        {
            Nick = nick.ToLower();
            OAuth = oath;
            Channel = channel.ToLower();
        }
        /// <summary>
        /// Starts the IRC session.
        /// </summary>
        public void Start()
        {
            _cancelToken = new CancellationTokenSource();
            Task.Run(() => _Loop());
        }
        /// <summary>
        /// Stops the IRC session. (Note: this stops the controller from recieving messages but the loop will not be canceled before the next message is sent, which is not that big of a deal - might get fixed in the future)
        /// </summary>
        public void Stop()
        {
            _cancelToken.Cancel();
            IRCWriter("PART", true);
        }
#endregion
        private void _Loop()
        {
            do
            {
                try
                {
                    using (_irc = new TcpClient(_server, _port))
                    using (_stream = _irc.GetStream())
                    using (_reader = new StreamReader(_stream))
                    using (_writer = new StreamWriter(_stream))
                    {
                        IRCWriter($"PASS {OAuth}");
                        IRCWriter($"NICK {Nick}");
                        IRCWriter("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership"); //tags, commands, membership anfordern
                        string inputline = "";
                        while (!_cancelToken.IsCancellationRequested && (inputline = _reader.ReadLine()) != null)
                        {
                            if(Debug)
                                Console.WriteLine("-> " + inputline);
                            string[] splitinput = inputline.Split(' '); //Bei jedem Leerzeichen aufsplitten
                            if (splitinput[0] == "PING")
                                IRCWriter("PONG :tmi.twitch.tv");
                            else if (splitinput.Length > 1 && splitinput[1] == "001") //001 = Erfolgreich verbunden -> dem Chat des Kanal joinen
                                IRCWriter($"JOIN #{Channel}");
                            else if(splitinput.Length >= 4 && splitinput[1].Contains("tmi.twitch.tv"))
                                switch(splitinput[2])
                            {
                                    case "CLEARCHAT":
                                        OnCLEARCHAT(inputline);
                                        break;
                                    case "CLEARMSG":
                                        OnCLEARMSG(inputline);
                                        break;
                                    case "GLOBALUSERSTATE":
                                        OnGLOBALUSERSTATE(inputline);
                                        break;
                                    case "NOTICE":
                                        OnNOTICE(inputline);
                                        break;
                                    case "PRIVMSG":
                                        OnPRIVMSG(inputline);
                                        break;
                                    case "RECONNECT":
                                        OnRECONNECT();
                                        break;
                                    case "ROOMSTATE":
                                        OnROOMSTATE(inputline);
                                        break;
                                    case "USERNOTICE":
                                        OnUSERNOTICE(inputline);
                                        break;
                                    case "USERSTATE":
                                        OnUSERSTATE(inputline);
                                        break;
                                    case "WHISPER":
                                        OnWHISPER(inputline);
                                        break;
                            }
                            else
                                switch (splitinput[1])
                                {
                                    case "PART":
                                        OnPart(inputline);
                                        break;
                                    case "JOIN":
                                        OnJoin(inputline);
                                        break;
                                    case "353":
                                        OnNameList(inputline);
                                        break;
                                    case "366":
                                        OnEndOfNames(inputline);
                                        break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Debug && ex?.Message != null)
                        Console.WriteLine(ex.Message);
                    Thread.Sleep(5000); //Bei Fehler nach x Sekunden Reconnect versuchen
                }
            } while (true);
        }

        #region Process IRC Events
        private void OnWHISPER(string inputline)
        {
            irc_whisper whisper = new irc_whisper(inputline);
            if (whisper == null) return;
            Events.OnWHISPER?.Invoke(this, whisper);
        }

        private void OnUSERSTATE(string inputline)
        {
            irc_userstate userstate = new irc_userstate(inputline);
            if (userstate == null) return;
            Events.OnUSERSTATE?.Invoke(this, userstate);
        }

        private void OnUSERNOTICE(string inputline)
        {
            irc_usernotice unotice = new irc_usernotice(inputline);
            if (unotice == null) return;
            Events.OnUSERNOTICE?.Invoke(this, unotice);
            switch (unotice.NoticeType)
            {
                case NoticeTypes.sub:
                    Events.OnALLSub?.Invoke(this, unotice);
                    Events.OnSub?.Invoke(this, unotice);
                    break;
                case NoticeTypes.resub:
                    Events.OnALLSub?.Invoke(this, unotice);
                    Events.OnResub?.Invoke(this, unotice);
                    break;
                case NoticeTypes.subgift:
                    Events.OnALLSub?.Invoke(this, unotice);
                    Events.OnSubgift?.Invoke(this, unotice);
                    break;
                case NoticeTypes.raid:
                    Events.OnRaid?.Invoke(this, unotice);
                    break;
                case NoticeTypes.ritual:
                    if (unotice.RitualInfo.RitualName != null && unotice.RitualInfo.RitualName == "new_chatter")
                        Events.OnFirstChatMessage?.Invoke(this, unotice);
                    break;
                case NoticeTypes.bitsbadgetier:
                    Events.OnBitsBadgeTierUpdate?.Invoke(this, unotice);
                    break;
                case NoticeTypes.announcement:
                    Events.OnAnnouncement?.Invoke(this, unotice);
                    break;
            }
        }

        private void OnRECONNECT()
        {
            this.Stop();
            Thread.Sleep(3000);
            this.Start();
        }
        private Dictionary<string,irc_roomstate> prev_roomstates = new Dictionary<string, irc_roomstate>(); //Key: Channelname, Value: Previos Roomstate
        private void OnROOMSTATE(string inputline)
        {
            irc_roomstate roomstate = new irc_roomstate(inputline, this);
            if (roomstate == null) return;
            Events.OnROOMSTATE?.Invoke(this, roomstate);
            if (prev_roomstates.ContainsKey(roomstate.ChannelName))
            {
                irc_roomstate prev_roomstate = prev_roomstates[roomstate.ChannelName];
                if (prev_roomstate.SlowMode != roomstate.SlowMode)
                    Events.SlowModeTriggered?.Invoke(this, roomstate);
                if (prev_roomstate.UniqueChatMode != roomstate.UniqueChatMode)
                    Events.UniqueMessageModeTriggered?.Invoke(this, roomstate);
                if(prev_roomstate.SubOnly != roomstate.SubOnly)
                    Events.SubOnlyTriggered?.Invoke(this,roomstate);
                if(prev_roomstate.FollowerOnly != roomstate.FollowerOnly)
                    Events.FollowerOnlyTriggered?.Invoke(this,roomstate);
                if(prev_roomstate.EmoteOnly != roomstate.EmoteOnly)
                    Events.EmoteOnlyTriggered?.Invoke(this,roomstate);
            }
            else
            {
                if(roomstate.SlowMode)
                    Events.SlowModeTriggered?.Invoke(this, roomstate);
                if (roomstate.UniqueChatMode)
                    Events.UniqueMessageModeTriggered?.Invoke(this, roomstate);
                if (roomstate.SubOnly)
                    Events.SubOnlyTriggered?.Invoke(this, roomstate);
                if (roomstate.FollowerOnly)
                    Events.FollowerOnlyTriggered?.Invoke(this, roomstate);
                if (roomstate.EmoteOnly)
                    Events.EmoteOnlyTriggered?.Invoke(this, roomstate);
            }
            prev_roomstates[roomstate.ChannelName] = roomstate;
        }

        private void OnNOTICE(string inputline)
        {
            irc_notice notice = new irc_notice(inputline);
            if (notice == null) return;
            Events.OnNOTICE?.Invoke(this, notice);
            switch (notice.msg_id)
            {
                case "host_on":
                    string[] targetchannelraw = notice.NoticeMessage.Split(' ');
                    if (targetchannelraw.Length > 3) return;
                    string targetchannel = targetchannelraw[2].Remove(targetchannelraw[2].Length - 1);
                    Events.OnHost?.Invoke(this, new irc_CostumEventArgs.HostArgs() { ChannelName = notice.Channel, TargetChannelName = targetchannel });
                    break;
            }
        }

        private void OnGLOBALUSERSTATE(string inputline)
        {
            irc_globaluserstate gus = new irc_globaluserstate(inputline);
            if (gus == null) return;
            Events.OnGLOBALUSERSTATE?.Invoke(this, gus);
        }

        private void OnCLEARMSG(string inputline)
        {
            irc_clearmsg clearmsg = new irc_clearmsg(inputline);
            if (clearmsg == null) return;
            Events.OnCLEARMSG?.Invoke(this, clearmsg); 
        }

        private void OnCLEARCHAT(string inputline)
        {
            irc_clearchat clearchat = new irc_clearchat(inputline);
            if (clearchat == null) return;
            Events.OnCLEARCHAT?.Invoke(this, clearchat);
        }

        private void OnPRIVMSG(string dataraw)
        {
            irc_privsmg privmsg = new irc_privsmg(dataraw,this);
            if (privmsg == null) return;
            if (privmsg.Username == this.Nick && IgnoreMessagesFromThisBot) return; //let's the bot ignore it's own messages.
            Events.OnPRIVMSG?.Invoke(this, privmsg);
            if(privmsg.IsChatMessage)
                Events.OnChatMessage?.Invoke(this, privmsg);
            if(privmsg.IsChatMessageReply)
                Events.OnChatMessageReply?.Invoke(this,privmsg);
            if (privmsg.IsBitMessage)
                Events.OnBitMessage?.Invoke(this, privmsg);
        }
        #endregion
        #region IRC Writers
        /// <summary> 
        /// Sends an IRC message to the server. Read the documentation before using this method! https://dev.twitch.tv/docs/irc/send-receive-messages#replying-to-a-chat-message
        /// </summary>
        /// <param name="text">raw IRC message</param>
        public void IRCWriter(string text)
        {
            IRCWriter(text, false);
        }
        /// <summary> 
        /// Sends an IRC message to the server. Read the documentation before using this method! https://dev.twitch.tv/docs/irc/send-receive-messages#replying-to-a-chat-message (note: do NOT set overridecancel to true, unless you know what you are doing)
        /// </summary>
        /// <param name="text">raw IRC message</param>
        internal void IRCWriter(string text, bool overridecancel = false)
        {
            if (text != "" && (overridecancel || !_cancelToken.IsCancellationRequested))
                try
                {
                    _writer.WriteLine(text);
                    _writer.Flush();
                    if(Debug)
                        Console.WriteLine("<- " + text); //Ausgangsnachricht in der Console loggen
                }
                catch (Exception ex)
                {
                    if(Debug)
                        Console.WriteLine(ex.Message);
                }
        }
        /// <summary> 
        /// Sends a message into the chat, correct format already applied.
        /// </summary>
        /// <param name="text">Chatmessage</param>
        /// <param name="ChannelName">Name of the channel</param>
        public void ChatWriter(string text, string ChannelName)
        {
            if (text != "")
                IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :{text}"); //Format laut Twitch Dokumentation
        }
        /// <summary>
        /// Sends a direct reply to a message, correct format already applied.
        /// </summary>
        /// <param name="text">Chatmessage</param>
        /// <param name="parent_message_id">ID of the message you want to reply to.</param>
        /// <param name="ChannelName">Name of the channel</param>
        public void ReplyChatWriter(string text, string parent_message_id, string ChannelName)
        {
            if (text != "" && parent_message_id != "")
                IRCWriter($"@reply-parent-msg-id={parent_message_id} PRIVMSG #{ChannelName} :{text}");
        }
        #region CHATCOMMANDS
        /// <summary>
        /// Deletes this message from chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void DeleteMessageFromChat(string ChannelName, string MessageID)
        {
                IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/delete {MessageID}");
        }
        /// <summary>
        /// Timeouts a user from this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void TimeOutUser(string Username, string ChannelName, int seconds)
        {
                IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/timeout {Username} {seconds}");
        }
        /// <summary>
        /// Removes a timeout from a user in this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnTimeOutUser(string Username, string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/untimeout {Username}");
        }
        /// <summary>
        /// Bans a user from this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void BanUser(string Username, string ChannelName, string reason = "Automod")
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/ban {Username} {reason}");
        }
        /// <summary>
        /// Unbans a user from this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnbanUser(string Username, string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unban {Username}");
        }
        /// <summary>
        /// Clears all messages from the chat room. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void ClearChat(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/clear");
        }
        /// <summary>
        /// Changes the color used for the bot’s username. The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Color">A color string like 'red' or a hex color code like '#0D4200'</param>
        public void Color(string ChannelName, string Color)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/color {Color}");
        }
        /// <summary>
        /// Runs a commercial. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="length">Supported lengts are: 30, 60, 90, 120, 150, 180 seconds.</param>
        public void StartCommercial(string ChannelName, int length = 30)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/commercial {length}");
        }
        /// <summary>
        /// Restricts users to posting chat messages that contain only emoticons. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void EmoteOnly(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/emoteonly");
        }
        /// <summary>
        /// Removes EmoteOnly restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void EmoteOnlyOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/emoteonlyoff");
        }
        /// <summary>
        /// Restricts who can post chat messages to followers only. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="FollowTime">minimum length of time following channel in minutes</param>
        public void FollowersOnly(string ChannelName, int FollowTime = 0)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/followers {FollowTime}");
        }
        /// <summary>
        /// Removes FollowersOnly restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void FollowersOnlyOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/followersoff");
        }
        /// <summary>
        /// Hosts another channel in this channel. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="TargetChannelName"></param>
        public void Host(string ChannelName, string TargetChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/host {TargetChannelName}");
        }
        /// <summary>
        /// Stops hosting the other channel. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnHost(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unhost");
        }
        /// <summary>
        /// Marks a section of the broadcast to highlight later. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetMarker(string ChannelName, string description = "")
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/marker {description}");
        }
        /// <summary>
        /// Removes the colon that typically appears after your chat name and italicizes the chat message’s text. The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void MeMessage(string ChannelName, string message)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/me {message}");
        }
        /// <summary>
        /// Gives a user moderator privileges. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void Mod(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/mod {Username}");
        }
        /// <summary>
        /// Revokes a users moderator privileges. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnMod(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unmod {Username}");
        }
        /// <summary>
        /// Lists the users that are moderators on the channel. The Twitch IRC server replies with a NOTICE message containing the list of moderator on this channel.
        /// </summary>
        public void Mods(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/mods");
        }
        /// <summary>
        /// Starts a raid. A raid sends your viewers to the specified channel. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void Raid(string ChannelName, string TargetChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/raid {TargetChannelName}");
        }
        /// <summary>
        /// Cancels a raid. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnRaid(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unraid");
        }
        /// <summary>
        /// Restricts how often users can post messages. This sets the minimum time, in seconds, that a user must wait before being allowed to post another message. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="waittime">minimum wait time in seconds</param>
        public void SlowMode(string ChannelName, int waittime)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/slow {waittime}");
        }
        /// <summary>
        /// Removes SlowMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SlowModeOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/slowoff");
        }
        /// <summary>
        /// Restricts who can post chat messages to subscribers only. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SubOnlyMode(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/subscribers");
        }
        /// <summary>
        /// Removes SubOnlyMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SubOnlyModeOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/subscribersoff");
        }
        /// <summary>
        /// Restricts a user’s chat messages to unique messages only; a user cannot send duplicate chat messages. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UniqueChatMode(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/uniquechat");
        }
        /// <summary>
        /// Removes UniqueChatMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UniqueChatModeOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/uniquechatoff");
        }
        /// <summary>
        /// Grants VIP status to a user. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void VIP(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/vip {Username}");
        }
        /// <summary>
        /// Revokes VIP status to a user. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnVIP(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unvip {Username}");
        }
        /// <summary>
        /// Lists the users with VIP status in the channel. The Twitch IRC server replies with a NOTICE message containing the list of vips on this channel.
        /// </summary>
        public void VIPs(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/vips");
        }
        /// <summary>
        /// Highlights a message with a color. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="message"></param>
        /// <param name="color">Supported colors: blue, green, orange, purple. Leave blank for the default color</param>
        public void Announce(string ChannelName, string message, string color = "default")
        {
            switch (color)
            {
                case "blue":
                    IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/announceblue {message}");
                    break;
                case "green":
                    IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/announcegreen {message}");
                    break;
                case "orange":
                    IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/announceorange {message}");
                    break;
                case "purple":
                    IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/announcepurple {message}");
                    break;
                default:
                    IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/announce {message}");
                    break;
            }
        }
        /// <summary>
        /// Vote in the active poll on the given channel. The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="index"></param>
        public void Vote(string ChannelName, int index)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/vote {index}");
        }
        /// <summary>
        /// Display profile information about a user on this channel. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message containing all information.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Username"></param>
        public void UserInformation(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/user {Username}");
        }
        /// <summary>
        /// Start restricting a user's messages. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Username"></param>
        public void RestrictUser(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/restrict {Username}");
        }
        /// <summary>
        /// Revokes restrictions from RestrictUser. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Username"></param>
        public void UnRestrictUser(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unrestrict {Username}");
        }
        //Polls, Predictions & Goals still missing (may be added in a future update)
        #endregion
        #endregion
        #region Users in Chat (+ Part, Join, 353, 366)
        /// <summary>
        /// Contains all users currently in Chat. Key: ChannelName, Value: List of users
        /// </summary>
        private Dictionary<string, List<string>> UsersInChat = new Dictionary<string, List<string>>();
        public List<string> GetUsersInChat(string ChannelName)
        {
            if (!UsersInChat.ContainsKey(ChannelName)) return null;
            return UsersInChat[ChannelName];
        }
        private void OnPart(string inputline)
        {
            string[] splitinput = inputline.Split(' ');
            if (splitinput.Length < 3) return;
            string ChannelName = splitinput[2].Remove(0, 1);
            string Username = splitinput[0].Split('!')[0].Remove(0, 1);
            CheckAndRemoveUser(Username, ChannelName);
            Events.OnChatLeave?.Invoke(this, new irc_CostumEventArgs.JoinPartArgs() { ChannelName = ChannelName, Username = Username, Type = "PART", AllUsers = GetUsersInChat(ChannelName) });
        }
        private void OnJoin(string inputline)
        {
            string[] splitinput = inputline.Split(' '); 
            if (splitinput.Length < 3) return;
            string ChannelName = splitinput[2].Remove(0, 1);
            string Username = splitinput[0].Split('!')[0].Remove(0, 1);
            CheckAndAddUser(Username, ChannelName);
            Events.OnChatJoin?.Invoke(this, new irc_CostumEventArgs.JoinPartArgs() { ChannelName = ChannelName, Username = Username, Type = "JOIN", AllUsers = GetUsersInChat(ChannelName) });
        }
        private void OnNameList(string inputline)
        {
            string[] splitinput = inputline.Split(' ');
            string ChannelName = splitinput[4].Remove(0, 1);
            for(int i = 5; i < splitinput.Length; i++)
            {
                if(i==5)
                    CheckAndAddUser(splitinput[i].Remove(0,1),ChannelName);
                else
                    CheckAndAddUser(splitinput[i],ChannelName);
            }
        }
        private void OnEndOfNames(string inputline)
        {
            string[] splitinput = inputline.Split(' ');
            string ChannelName = splitinput[3].Remove(0, 1).ToLower();
            Events.OnNameList?.Invoke(this, new irc_CostumEventArgs.NameListArgs() { ChannelName = ChannelName, UsersInChat = UsersInChat[ChannelName] });
        }
        private void CheckAndAddUser(string username, string channelname)
        {
            username = username.ToLower();
            channelname = channelname.ToLower();
            if (!UsersInChat.ContainsKey(channelname) || UsersInChat[channelname] == null)
                UsersInChat[channelname] = new List<string>();
            if(!UsersInChat[channelname].Contains(username))
                UsersInChat[channelname].Add(username);
        }
        private void CheckAndRemoveUser(string username, string channelname)
        {
            username = username.ToLower();
            channelname = channelname.ToLower();
            if (!UsersInChat.ContainsKey(channelname) || UsersInChat[channelname] == null)
                UsersInChat[channelname] = new List<string>();
            if (UsersInChat[channelname].Contains(username))
                UsersInChat[channelname].Remove(username);
        }
        #endregion
    }
}
