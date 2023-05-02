using P90Ez.Twitch.API;
using P90Ez.Twitch.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static P90Ez.Twitch.IRC.irc_usernotice;
using static P90Ez.Twitch.Login;

namespace P90Ez.Twitch.IRC
{
    /// <summary>
    /// <strong>The IRC-Brain.
    /// <para>Usage:</para></strong>
    /// <list type="number">
    /// <item>Create a new instance</item>
    /// <item>Use Start()-function to start communication with Twitch irc-servers</item>
    /// </list>
    /// <para>Requiered Scopes: <em>chat:read, chat:edit (channel:moderate, whispers:read, whispers:edit)</em> - <seealso href="https://dev.twitch.tv/docs/authentication/scopes#chat-and-pubsub-scopes">A list of scopes for IRC</seealso></para>
    /// <para>Requiered TokenType: <em>User Access Token</em></para>
    /// </summary>
    public class Controller
    {
        #region Basic vars

        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "chat:read chat:edit";
        /// <summary>
        /// Requiered Scopes to use chat moderation tools. <em>(Note: most of them won't work after february 2023 - use API Endpoints instead)</em>
        /// </summary>
        public static string ModerationScopes { get; } = "channel:moderate";
        /// <summary>
        /// Requiered Scopes to use to read &amp; send whispers. <em>(Note: DEPRECATED - use API Endpoints instead)</em>
        /// </summary>
        public static string WhisperScopes { get; } = "whispers:read whispers:edit";
        /// <summary>                                    
        /// Requiered Tokentype to use this module.                                
        /// </summary>
        public static TokenType RequieredTokenType { get; } = TokenType.UserAccessToken;
        /// <summary>
        /// Bot's username.
        /// </summary>
        public string Nick { get; }
        /// <summary>
        /// Bot's OAUTH token.
        /// </summary>
        private protected string OAuth { get; }
        private protected Credentials credentials { get; }
        private SimplifiedRequests SimplifiedRequests { get; }
        /// <summary>
        /// Channelname, which the irc stream is attached to.
        /// </summary>
        public string Channel { get; }
        /// <summary>
        /// ID of the channel, which the irc stream is attached to.
        /// </summary>
        public long Channel_ID { get; }

        private const string _server = "irc.chat.twitch.tv";
        private const int _port = 6667;
        private NetworkStream _stream;
        private TcpClient _irc;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cancelToken;
        /// <summary>
        /// Set to True to Display IRC Message in the Console [Deprecated - will be removed in the future - please use the Logger!]
        /// </summary>
        public bool Debug = false;
        internal ILogger Logger { get; }
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
        /// <param name="credentials"></param>
        /// <param name="channel"></param>
        public Controller(Login.Credentials credentials, string channel, ILogger Logger = null)
        {
            if (Logger == null) this.Logger = new Logger();
            else this.Logger = Logger;

            if (credentials == null) throw new Exceptions.ArgumentNullException(nameof(credentials), Logger);
            if (!credentials.IsSuccess) throw new IncorrectTokenException(Logger);
            if (!credentials.IsCorrectTokenType(RequieredTokenType)) throw new WrongTokenTypeException(RequieredTokenType, credentials.TokenType, "IRC controller", Logger);

            Nick = credentials.UserLogin.ToLower();
            Channel = channel.ToLower();
            if(!credentials.AuthToken.ToLower().Contains("oauth:"))
                OAuth = "oauth:" + credentials.AuthToken;
            else
                OAuth = credentials.AuthToken;

            this.credentials = credentials;
            SimplifiedRequests = new API.SimplifiedRequests(credentials, Logger);

            Channel_ID = SimplifiedRequests.GetBroadcasterID(channel);
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
                        IRCWriter("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership"); //request tags, commands, membership
                        string inputline = "";
                        while (!_cancelToken.IsCancellationRequested && (inputline = _reader.ReadLine()) != null)
                        {
                            if(Debug)
                                Console.WriteLine("-> " + inputline);
                            string[] splitinput = inputline.Split(' '); //split string on each space character
                            if (splitinput[0] == "PING")
                                IRCWriter("PONG :tmi.twitch.tv");
                            else if (splitinput.Length > 1 && splitinput[1] == "001") //001 = successfully connected -> join chat
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
                    if (ex != null && ex.Message != null)
                        Logger.Log(ex.Message, ILogger.Severety.Critical);

                    if (Debug && ex?.Message != null)
                        Console.WriteLine(ex.Message);
                    Thread.Sleep(5000); //When an error has occurred, wait 5 seconds and try again
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
                    Events.OnHost?.Invoke(this, new irc_CostumEventArgs.HostArgs() { ChannelName = notice.ChannelName, TargetChannelName = targetchannel });
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
            switch (clearchat.ClearChatType)
            {
                case irc_clearchat.ClearChatTypes.UserBan:
                    Events.UserBanned?.Invoke(this, clearchat);
                    break;
                case irc_clearchat.ClearChatTypes.UserTimeout:
                    Events.UserTimeOut?.Invoke(this, clearchat);
                    break;
                case irc_clearchat.ClearChatTypes.RemovedAllMessages:
                    Events.ChatCleared?.Invoke(this, clearchat);
                    break;
            }
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
        /// [DEPRECATED] Deletes this message from chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void DeleteMessageFromChat(string ChannelName, string MessageID)
        {
                IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/delete {MessageID}");
        }
        /// <summary>
        /// [DEPRECATED] Timeouts a user from this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void TimeOutUser(string Username, string ChannelName, int seconds)
        {
                IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/timeout {Username} {seconds}");
        }
        /// <summary>
        /// [DEPRECATED] Removes a timeout from a user in this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnTimeOutUser(string Username, string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/untimeout {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Bans a user from this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void BanUser(string Username, string ChannelName, string reason = "")
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/ban {Username} {reason}");
        }
        /// <summary>
        /// [DEPRECATED] Unbans a user from this chat. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnbanUser(string Username, string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unban {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Clears all messages from the chat room. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void ClearChat(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/clear");
        }
        /// <summary>
        /// [DEPRECATED] Changes the color used for the bot’s username. The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Color">A color string like 'red' or a hex color code like '#0D4200'</param>
        public void Color(string ChannelName, string Color)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/color {Color}");
        }
        /// <summary>
        /// [DEPRECATED] Runs a commercial. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="length">Supported lengts are: 30, 60, 90, 120, 150, 180 seconds.</param>
        public void StartCommercial(string ChannelName, int length = 30)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/commercial {length}");
        }
        /// <summary>
        /// [DEPRECATED] Restricts users to posting chat messages that contain only emoticons. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void EmoteOnly(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/emoteonly");
        }
        /// <summary>
        /// [DEPRECATED] Removes EmoteOnly restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void EmoteOnlyOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/emoteonlyoff");
        }
        /// <summary>
        /// [DEPRECATED] Restricts who can post chat messages to followers only. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="FollowTime">minimum length of time following channel in minutes</param>
        public void FollowersOnly(string ChannelName, int FollowTime = 0)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/followers {FollowTime}");
        }
        /// <summary>
        /// [DEPRECATED] Removes FollowersOnly restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void FollowersOnlyOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/followersoff");
        }
        /// <summary>
        /// [DEPRECATED] Hosts another channel in this channel. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="TargetChannelName"></param>
        public void Host(string ChannelName, string TargetChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/host {TargetChannelName}");
        }
        /// <summary>
        /// [DEPRECATED] Stops hosting the other channel. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnHost(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unhost");
        }
        /// <summary>
        /// [DEPRECATED] Marks a section of the broadcast to highlight later. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SetMarker(string ChannelName, string description = "")
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/marker {description}");
        }
        /// <summary>
        /// [DEPRECATED] Removes the color that typically appears after your chat name and italicizes the chat message’s text. The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void MeMessage(string ChannelName, string message)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/me {message}");
        }
        /// <summary>
        /// [DEPRECATED] Gives a user moderator privileges. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void Mod(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/mod {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Revokes a users moderator privileges. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnMod(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unmod {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Lists the users that are moderators on the channel. The Twitch IRC server replies with a NOTICE message containing the list of moderator on this channel.
        /// </summary>
        public void Mods(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/mods");
        }
        /// <summary>
        /// [DEPRECATED] Starts a raid. A raid sends your viewers to the specified channel. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void Raid(string ChannelName, string TargetChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/raid {TargetChannelName}");
        }
        /// <summary>
        /// [DEPRECATED] Cancels a raid. The bot needs permission to perform this action (channel_editor)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnRaid(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unraid");
        }
        /// <summary>
        /// [DEPRECATED] Restricts how often users can post messages. This sets the minimum time, in seconds, that a user must wait before being allowed to post another message. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="waittime">minimum wait time in seconds</param>
        public void SlowMode(string ChannelName, int waittime)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/slow {waittime}");
        }
        /// <summary>
        /// [DEPRECATED] Removes SlowMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SlowModeOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/slowoff");
        }
        /// <summary>
        /// [DEPRECATED] Restricts who can post chat messages to subscribers only. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SubOnlyMode(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/subscribers");
        }
        /// <summary>
        /// [DEPRECATED] Removes SubOnlyMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void SubOnlyModeOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/subscribersoff");
        }
        /// <summary>
        /// [DEPRECATED] Restricts a user’s chat messages to unique messages only; a user cannot send duplicate chat messages. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UniqueChatMode(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/uniquechat");
        }
        /// <summary>
        /// [DEPRECATED] Removes UniqueChatMode restriction. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UniqueChatModeOff(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/uniquechatoff");
        }
        /// <summary>
        /// [DEPRECATED] Grants VIP status to a user. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void VIP(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/vip {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Revokes VIP status to a user. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        public void UnVIP(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unvip {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Lists the users with VIP status in the channel. The Twitch IRC server replies with a NOTICE message containing the list of vips on this channel.
        /// </summary>
        public void VIPs(string ChannelName)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/vips");
        }
        /// <summary>
        /// [DEPRECATED] Highlights a message with a color. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
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
        /// [DEPRECATED] Vote in the active poll on the given channel. The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="index"></param>
        public void Vote(string ChannelName, int index)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/vote {index}");
        }
        /// <summary>
        /// [DEPRECATED] Display profile information about a user on this channel. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message containing all information.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Username"></param>
        public void UserInformation(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/user {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Start restricting a user's messages. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Username"></param>
        public void RestrictUser(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/restrict {Username}");
        }
        /// <summary>
        /// [DEPRECATED] Revokes restrictions from RestrictUser. The bot needs permission to perform this action (channel:moderate)! The Twitch IRC server replies with a NOTICE message indicating whether the command succeeded or failed.
        /// </summary>
        /// <param name="ChannelName"></param>
        /// <param name="Username"></param>
        public void UnRestrictUser(string ChannelName, string Username)
        {
            IRCWriter($"PRIVMSG #{ChannelName.ToLower()} :/unrestrict {Username}");
        }
        //[DEPRECATED] Polls, Predictions & Goals still missing (may be added in a future update)
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
        #region Moderation & Chat Settings
        /// <summary>
        /// Timeouts a user from this chat. The bot needs permission to perform this action (channel:moderate/moderator:manage:banned_users)!
        /// </summary>
        public bool TimeoutUser(long UserID, int Seconds, string Reason = "")
        {
            return SimplifiedRequests.TimeoutUser(Channel_ID, UserID, Seconds, Reason);
        }
        /// <summary>
        /// Removes a timeout from a user in this chat. The bot needs permission to perform this action (channel:moderate/moderator:manage:banned_users)!
        /// </summary>
        public bool UntimeoutUser(long UserID)
        {
            return SimplifiedRequests.UntimeoutUser(Channel_ID, UserID);
        }
        /// <summary>
        /// Bans a user from this chat. The bot needs permission to perform this action (channel:moderate/moderator:manage:banned_users)!
        /// </summary>
        public bool BanUser(long UserID, string Reason = "")
        {
            return SimplifiedRequests.BanUser(Channel_ID, UserID, Reason);
        }
        /// <summary>
        /// Unbans a user from this chat. The bot needs permission to perform this action (channel:moderate/moderator:manage:banned_users)!
        /// </summary>
        public bool UnbanUser(long UserID)
        {
            return SimplifiedRequests.UnbanUser(Channel_ID, UserID);
        }
        /// <summary>
        /// Deletes this message from chat. The bot needs permission to perform this action (channel:moderate/channel:moderate:chat_messages)!
        /// </summary>
        /// <returns>True if successful.</returns>
        public bool DeleteMessageFromChat(string MessageID)
        {
            return SimplifiedRequests.DeleteChatMessage(Channel_ID, MessageID);
        }
        /// <summary>
        /// Clears all messages from the chat room. The bot needs permission to perform this action (channel:moderate/moderator:manage:chat_messages)!
        /// </summary>
        public bool ClearChat()
        {
            return SimplifiedRequests.ClearChat(Channel_ID);
        }
        /// <summary>
        /// Restricts users to posting chat messages that contain only emoticons. The bot needs permission to perform this action (channel:moderate/moderator:manage:chat_settings)!
        /// </summary>
        public bool EmoteOnly(bool Enabled)
        {
            return SimplifiedRequests.EmoteChat(Channel_ID, Enabled);
        }
        /// <summary>
        /// Restricts who can post chat messages to followers only (min follow time in minutes). The bot needs permission to perform this action (channel:moderate/moderator:manage:chat_settings)!
        /// </summary>
        public bool FollowersOnly(bool Enabled, int MinFollowTime = -1)
        {
            return SimplifiedRequests.FollowerChat(Channel_ID, Enabled, MinFollowTime);
        }
        /// <summary>
        /// Restricts who can post chat messages to subscribers only. The bot needs permission to perform this action (channel:moderate/moderator:manage:chat_settings)!
        /// </summary>
        public bool SubOnlyMode(bool Enabled)
        {
            return SimplifiedRequests.SubscriberChat(Channel_ID, Enabled);
        }
        /// <summary>
        /// Restricts a user’s chat messages to unique messages only; a user cannot send duplicate chat messages. The bot needs permission to perform this action (channel:moderate/moderator:manage:chat_settings)!
        /// </summary>
        public bool UniqueChatMode(bool Enable)
        {
            return SimplifiedRequests.UniqueMode(Channel_ID, Enable);
        }
        /// <summary>
        /// Restricts how often users can post messages. This sets the minimum time, in seconds, that a user must wait before being allowed to post another message. The bot needs permission to perform this action (channel:moderate/moderator:manage:chat_settings)!
        /// </summary>
        public bool SlowMode(bool Enable, int WaitTime = -1)
        {
            return SimplifiedRequests.SlowMode(Channel_ID, Enable, WaitTime);
        }
        /// <summary>
        /// Adds a short delay before chat messages appear in the chat room. This gives chat moderators and bots a chance to remove them before viewers can see the message. The bot needs permission to perform this action (moderator:manage:chat_settings)!
        /// </summary>
        /// <param name="delay">The amount of time, in seconds, that messages are delayed before appearing in chat. Possible values are: 2, 4, 6 (seconds)</param>
        public bool ModerationChatDelay(bool Enabled, int delay = -1)
        {
            return SimplifiedRequests.ModerationChatDelay(Channel_ID, Enabled, delay);
        }
        #endregion
    }
}
