using P90Ez.Extensions;
using P90Ez.Twitch.API.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;
using static P90Ez.Twitch.EventSub.JsonStructure;

namespace P90Ez.Twitch.EventSub
{
    public class IStandardEvent
    {
        /// <summary>
        /// Creates a new EventSub Subscription.
        /// </summary>
        /// <param name="Parent">Parent EventSubInstance Object</param>
        /// <param name="subscriptionType">Type of the event.</param>
        /// <param name="condition">The condition provided for this event.</param>
        /// <param name="version">The subscription type version.</param>
        internal IStandardEvent(EventSubInstance Parent, string subscriptionType, List<KeyValuePair<string,string>> condition, ushort version, ILogger Logger)
        {
            this.Parent = Parent;
            SubscriptionType = subscriptionType;
            Condition = condition;
            Version = version;
            this.Logger = Logger;
            if(Parent.HasRecievedWelcome)
                CreateSubscription();
            else
                Parent.RecievedWelcome += Parent_RecievedWelcome;
        }

        /// <summary>
        /// The (provided) logger for this EventSub instance.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// This function is needed to subscribe to the event after the welcome message was recieved from Twitch's servers.
        /// </summary>
        private void Parent_RecievedWelcome(object sender, EventArgs e)
        {
            Parent.RecievedWelcome -= Parent_RecievedWelcome; //unsubscribes itself. Must only be called once.
            if (SubscriptionID == "")
                CreateSubscription();
        }

        /// <summary>
        /// Parent EventSubInstance Object
        /// </summary>
        internal EventSubInstance Parent { get; }

        /// <summary>
        /// Type of the event.
        /// </summary>
        public string SubscriptionType { get; }
        /// <summary>
        /// The condition provided for this event.
        /// </summary>
        internal List<KeyValuePair<string,string>> Condition { get; }

        /// <summary>
        /// The subscription type version.
        /// </summary>
        internal ushort Version { get; }

        /// <summary>
        /// An ID that identifies this subscription.
        /// </summary>
        internal string SubscriptionID { get; set; } = "";

        /// <summary>
        /// Triggers this event.
        /// </summary>
        /// <param name="payload">The payload from the notification message.</param>
        internal virtual void TriggerEvent(_Payload payload) { }

        /// <summary>
        /// Creates the subscription using the API endpoint.
        /// </summary>
        /// <returns>Return true if event was successfuly created.</returns>
        internal bool CreateSubscription() 
        {
            //call API
            var data = CreateEventSubSubscription.ForWebsocket(Parent._Creds, Parent.Session_ID, SubscriptionType, Version, Condition, out bool isSuccess, out int httpStatuscode);
            
            if(isSuccess && data != null && data.data != null && data.data.Count > 0)
            {
                if (data.data[0].ID != null && data.data[0].ID != "")
                {
                    SubscriptionID = data.data[0].ID; //set subscription id
                    if (Parent.SubscribedEvents.ContainsKey(SubscriptionID))
                    {
                        Parent.SubscribedEvents.Remove(SubscriptionID, out IStandardEvent value); //SubscriptionIDs should be unique, if they are not -> previous subscription will be removed
                        value.RevokeSubscription();
                    }
                    Parent.SubscribedEvents.TryAdd(SubscriptionID, this); //add subscription to internal subscription collection
                }
            }

            if(httpStatuscode != 202 ||!isSuccess)
            {
                Logger.Log($"Event subscription failed! (code: {httpStatuscode}) Type: {SubscriptionType}, Condition: {Condition.ToJsonString()}", ILogger.Severety.Warning);
            }
            return isSuccess && httpStatuscode == 202;
        }

        /// <summary>
        /// Restores the subscribtion after a reconnect.
        /// </summary>
        internal void Resubscribe()
        {
            SubscriptionID = "";
            if (Parent.HasRecievedWelcome)
                CreateSubscription();
            else
                Parent.RecievedWelcome += Parent_RecievedWelcome;
        }

        /// <summary>
        /// Removes this event from the EventSub subscription list. This event will not be triggered again.
        /// </summary>
        /// <returns>Returns true if event was successfuly removed.</returns>
        public bool RevokeSubscription() 
        {
            if (SubscriptionID == null || SubscriptionID == "") return false;
            //call API to revoke subscribtion
            DeleteEventSubSubscription.Go(Parent._Creds, SubscriptionID, out bool isSuccess, out int httpStatuscode);

            Parent.SubscribedEvents.Remove(SubscriptionID, out IStandardEvent value); //remove from internal subscription collection
            SubscriptionID = "";

            if (httpStatuscode != 204 || !isSuccess)
            {
                Logger.Log($"Event subscription revokation failed! (code: {httpStatuscode}) Type: {SubscriptionType}, Condition: {Condition.ToJsonString()}", ILogger.Severety.Warning);
            }
            return isSuccess && httpStatuscode == 204;
        }
    }
}
