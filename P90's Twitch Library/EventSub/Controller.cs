using Newtonsoft.Json;
using P90Ez.Extensions;
using P90Ez.Twitch.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using WebSocketSharp;
using static P90Ez.Twitch.Login;

namespace P90Ez.Twitch.EventSub
{
    public partial class EventSubInstance
    {
        #region Variables
        /// <summary>
        /// <strong>The required scopes depend on the events you want to subscribe to!</strong> <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/">Learn more</see>
        /// </summary>
        public static string RequieredScopes { get; } = "";
        /// <summary>
        /// Requiered Tokentype to use this module.
        /// </summary>
        public static TokenType RequieredTokenType { get; } = TokenType.UserAccessToken;
        /// <summary>
        /// Websocket EventSub Session ID
        /// </summary>
        internal string Session_ID { get { return _Controller.Session_ID; } }
        /// <summary>
        /// True if the welcome message from the Twitch server was recieved.
        /// </summary>
        internal bool HasRecievedWelcome { get { return _Controller.WelcomeRecieved; } }
        /// <summary>
        /// Will be invoked whenever the Websocket recives a welcome message from Twitch's server.
        /// </summary>
        internal event EventHandler RecievedWelcome;
        /// <summary>
        /// The user's generated credentials.
        /// </summary>
        internal Credentials _Creds { get; }
        /// <summary>
        /// Websocket Controller
        /// </summary>
        private Controller _Controller { get; }
        /// <summary>
        /// Contains all events this instance recieves notifications from.
        /// </summary>
        internal ConcurrentDictionary<string, IStandardEvent> SubscribedEvents { get; }
        /// <summary>
        /// The (provided) logger for this EventSub instance.
        /// </summary>
        internal ILogger Logger { get; }
        #endregion

        /// <summary>
        /// Creates a new EventSub instance. Only create multiple instances with the same credentials if you excactly know what you are doing. <see href="https://dev.twitch.tv/docs/eventsub/handling-websocket-events/#subscription-limits">Learn more</see>
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <exception cref="WrongTokenTypeException">Throws a WrongTokenTypeException if the provided token does not match the <seealso cref="RequieredTokenType"/></exception>
        public EventSubInstance(Credentials credentials, ILogger Logger = null)
        {
            if (Logger == null) 
                this.Logger = new Logger();
            else
                this.Logger = Logger;

            if(credentials == null) throw new ArgumentNullException(nameof(credentials));
            if (!credentials.IsSuccess) throw new IncorrectTokenException(Logger);
            if (!credentials.IsCorrectTokenType(RequieredTokenType)) throw new WrongTokenTypeException(RequieredTokenType, credentials.TokenType, "EventSub", Logger);
            _Creds = credentials;
            
            SubscribedEvents = new ConcurrentDictionary<string, IStandardEvent>();
            _Controller = new Controller(this);
        }

        #region Start, disconnect
        /// <summary>
        /// Terminates the Websocket connection.
        /// </summary>
        public void Disconnect()
        {
            _Controller.Disconnect();
        }
        /// <summary>
        /// Connect to Twitch's servers. Will do nothing when the Websocket is already active.
        /// </summary>
        internal void WS_Start()
        {
            _Controller.Connect();
        }
        #endregion

        internal class Controller
        {
            #region Variables
            private WebSocket _ws;
            /// <summary>
            /// Parent object to access <see cref="SubscribedEvents"/> and more.
            /// </summary>
            private EventSubInstance Parent { get; }

            const string URL = "wss://eventsub.wss.twitch.tv/ws";
            private string alt_url = "";
            /// <summary>
            /// Websocket EventSub Session ID
            /// </summary>
            internal string Session_ID = "";
            /// <summary>
            /// If Twitch's servers don't send a message in this time (in seconds) the connection will be terminated.
            /// </summary>
            private int KeepAliveTime = 10;
            /// <summary>
            /// True if the websocket was started.
            /// </summary>
            private bool StartedWS = false;
            /// <summary>
            /// True if the welcome message from the Twitch server was recieved.
            /// </summary>
            internal bool WelcomeRecieved = false;
            /// <summary>
            /// True if the Websocket disconnect was initiated by the caller.
            /// </summary>
            private bool IntentionalDisconnect = false;
            /// <summary>
            /// The (provided) logger for this EventSub instance.
            /// </summary>
            private ILogger Logger { get { return Parent.Logger; } }
            #endregion

            #region Setup
            internal Controller(EventSubInstance Parent)
            {
                this.Parent = Parent;
                WS_Setup(URL);
            }

            /// <summary>
            /// Websocket setup. Creates a new instance with the provided url and subscribes to events.
            /// </summary>
            /// <param name="url"></param>
            private void WS_Setup(string url)
            {
                _ws = new WebSocket(url);
                _ws.OnMessage += OnMessage;
                _ws.OnClose += OnClose;
                _ws.OnError += OnError;
            }
            #endregion

            #region Websocket events
            /// <summary>
            /// Will be called by the Websocket when the connection was closed.
            /// </summary>
            private void OnClose(object sender, CloseEventArgs e)
            {
                WelcomeRecieved = false;
                if(e != null && !IntentionalDisconnect && !e.WasClean)
                {
                    Logger.Log($"Connection closed: {e.Code} {e.Reason}", ILogger.Severety.Warning);
                }
                if (!IntentionalDisconnect && (e != null && !e.WasClean))
                    Reconnect();
                IntentionalDisconnect = false;
            }

            /// <summary>
            /// Will be called by the Websocket when an error has occured.
            /// </summary>
            private void OnError(object sender, ErrorEventArgs e)
            {
                if (e == null) return;
                if (e.Message == "Error: An error has occurred in connecting.") Logger.Log("EventSub connection lost. Reconnecting...", ILogger.Severety.Warning);
                else Logger.Log(e.Message, ILogger.Severety.Critical);
            }

            /// <summary>
            /// Will be called by the Websocket when a message is recieved.
            /// </summary>
            private void OnMessage(object sender, MessageEventArgs e)
            {
                if (!e.IsText) return;
                if(e.Data == null) return;
                JsonStructure data = JsonConvert.DeserializeObject<JsonStructure>(e.Data);
                MessageHandler(data);
            }
            #endregion

            #region message handling
            void MessageHandler(JsonStructure message)
            {
                if (message == null || message.Metadata == null) return;
                
                switch(message.Metadata.Type)
                {
                    case "session_welcome": //subscribe to topics
                        if (message.Payload != null && message.Payload.Session != null)
                        {
                            //Logger.Log("Recieved a welcome message", ILogger.Severety.Message); //debug
                            if(message.Payload.Session.ID != null)
                                Session_ID = message.Payload.Session.ID;
                            KeepAliveTime = message.Payload.Session.keepalive_timeout_seconds;
                            WelcomeRecieved = true;
                            Parent.RecievedWelcome?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case "notification": //event
                        if(message.Payload != null && message.Payload.Subscription != null && message.Payload.Event != null)
                        {
                            //Logger.Log("Recieved a notification (event) message", ILogger.Severety.Message); //debug
                            if (Parent.SubscribedEvents.ContainsKey(message.Payload.Subscription.ID))
                                Parent.SubscribedEvents[message.Payload.Subscription.ID].TriggerEvent(message.Payload);
                        }
                        break;
                    case "session_reconnect": //connect to reconnect_url
                        if(message.Payload != null && message.Payload.Session != null && message.Payload.Session.ID == Session_ID && message.Payload.Session.Status == "reconnecting")
                        {
                            Logger.Log("Recieved a reconnect message", ILogger.Severety.Message);
                            alt_url = message.Payload.Session.reconnect_url;
                            Reconnect();
                            return;
                        }
                        break;
                    case "revocation":
                        if(message.Payload != null && message.Payload.Subscription != null && message.Payload.Subscription.ID != null)
                        {
                            Logger.Log($"Revocation message recieved! subscription type: {message.Payload.Subscription.Type}, status: {message.Payload.Subscription.Status}, condition: {message.Payload.Subscription.condition.ToJsonString()}", ILogger.Severety.Message);
                            if(Parent.SubscribedEvents.ContainsKey(message.Payload.Subscription.ID))
                                Parent.SubscribedEvents.Remove(message.Payload.Subscription.ID, out IStandardEvent value);
                        }
                        break;
                    default:
                        break;
                }
                KeepAliveRecieved();
            }
            #endregion

            #region Connect, Disconnect, Reconnect
            /// <summary>
            /// Connect to Twitch's servers. Will do nothing when the Websocket is already active.
            /// </summary>
            internal void Connect()
            {
                if (_ws == null || _ws.IsAlive || StartedWS) return;
                _ws.Connect();
                StartedWS = true;
            }

            /// <summary>
            /// Terminates the connection. Will do nothing when the Websocket connection is not active.
            /// </summary>
            public void Disconnect()
            {
                WelcomeRecieved = false;
                StartedWS = false;
                IntentionalDisconnect = true;
                if (_ws != null && _ws.IsAlive)
                    _ws.Close();
            }

            /// <summary>
            /// Reconnects the Websocket to Twitch's servers. Will use <see cref="alt_url"/> when set.
            /// </summary>
            private void Reconnect()
            {
                KeepAlive_Stop();
                WelcomeRecieved = false;
                if(_ws != null && _ws.IsAlive)
                    _ws.Close();
                if (alt_url != "")
                {
                    WS_Setup(alt_url);
                    alt_url = "";
                    _ws.Connect();
                }
                else
                {
                    System.Threading.Thread.Sleep(3000);
                    if (_ws == null) WS_Setup(URL);
                    foreach (var obj in Parent.SubscribedEvents.Values)
                    {
                        obj.Resubscribe();
                    }
                    _ws.Connect();
                }
            }
            #endregion

            #region KeepAlive
            /// <summary>
            /// Keep Alive timer. Will terminate the Websocket connection when timer elapses.
            /// </summary>
            private Timer KeepAliveTimer;
            /// <summary>
            /// Gets called when a message from Twitch's servers was recieved. Resets the <see cref="KeepAliveTimer"/>.
            /// </summary>
            private void KeepAliveRecieved()
            {
                KeepAlive_Stop();
                KeepAliveTimer = new Timer((KeepAliveTime+3) * 1000); //+3 seconds, are needed due to network and processing delays
                KeepAliveTimer.AutoReset = false;
                KeepAliveTimer.Elapsed += KeepAliveTimer_Elapsed;
                KeepAliveTimer.Start();
            }

            /// <summary>
            /// Stops the <see cref="KeepAliveTimer"/>.
            /// </summary>
            private void KeepAlive_Stop()
            {
                if (KeepAliveTimer != null)
                {
                    KeepAliveTimer.Stop();
                }
                KeepAliveTimer = null;
            }
            /// <summary>
            /// Function will be called when the <see cref="KeepAliveTimer"/> elapsed. Terminates the Websockets connection.
            /// </summary>
            private void KeepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                KeepAlive_Stop();
                Logger.Log("Connection terminated! KeepAlive Timout!", ILogger.Severety.Warning);
                Reconnect();
            }
            #endregion
        }

    }
}
