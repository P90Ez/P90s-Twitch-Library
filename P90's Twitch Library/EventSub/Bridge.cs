using P90Ez.Twitch.EventSub.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub
{
    public partial class EventSubInstance
    {
        /// <summary>
        /// The channel.update subscription type sends notifications when a broadcaster updates the category, title, mature flag, or broadcast language for their channel.
        /// <para>Requiered scope: <em>-</em></para>
        /// </summary>
        /// <param name="Broadcaster_ID">The broadcaster user ID for the channel you want to get updates for.</param>
        /// <returns>An object of Channel_Update. Use this object to access the EventHandler.</returns>
        public Channel_Update Add_Channel_Updates(string Broadcaster_ID)
        {
            var tmp = new Channel_Update(this, Broadcaster_ID, Logger);
            WS_Start();
            return tmp;
        }

        /// <summary>
        /// The channel.follow subscription type sends a notification when a specified channel receives a follow.
        /// <para>Requiered scope: <em>moderator:read:followers</em></para>
        /// </summary>
        /// <param name="Broadcaster_ID">The broadcaster user ID for the channel you want to get follow notifications for.</param>
        /// <returns>An object of Channel_Follow. Use this object to access the EventHandler.</returns>
        public Channel_Follow Add_Follows(string Broadcaster_ID)
        {
            var tmp = new Channel_Follow(this, Broadcaster_ID, Logger);
            WS_Start();
            return tmp;
        }

        /// <summary>
        /// Contains all events regarding subcriptions to the specified broadcaster.
        /// <para>Requiered scope: <em>channel:read:subscriptions</em></para>
        /// </summary>
        /// <param name="Broadcaster_ID">The broadcaster user ID for the channel you want to get subscription notifications for.</param>
        /// <returns>An object of Channel_Subscriptions. Use this object to access the EventHandlers.</returns>
        public Channel_Subscriptions Add_Subscriptions(string Broadcaster_ID)
        {
            var tmp = new Channel_Subscriptions(this, Broadcaster_ID, Logger);
            WS_Start();
            return tmp;
        }

        /// <summary>
        /// The channel.cheer subscription type sends a notification when a user cheers on the specified channel.
        /// <para>Requiered scope: <em>bits:read</em></para>
        /// </summary>
        /// <param name="Broadcaster_ID">The broadcaster user ID for the channel you want to get cheer notifications for.</param>
        /// <returns>An object of Channel_Cheer. Use this object to access the EventHandler.</returns>
        public Channel_Cheer Add_Cheers(string Broadcaster_ID)
        {
            var tmp = new Channel_Cheer(this, Broadcaster_ID, Logger);
            WS_Start();
            return tmp;
        }

        /// <summary>
        /// The channel.raid subscription type sends a notification when a broadcaster raids another broadcaster’s channel.
        /// <para>Requiered scope: <em>-</em></para>
        /// </summary>
        /// <param name="Broadcaster_ID">The broadcaster user ID that created (or recieved) the raid you want to get notifications for.</param>
        /// <returns>An object of Channel_Raid. Use this object to access the EventHandlers.</returns>
        public Channel_Raid Add_Raids(string Broadcaster_ID) 
        {
            var tmp = new Channel_Raid(this, Broadcaster_ID, Logger);
            WS_Start();
            return tmp;
        }

        /// <summary>
        /// The channel.ban subscription type sends a notification when a viewer is timed out or banned from the specified channel.
        /// <para>Requiered scope: <em>channel:moderate</em></para>
        /// </summary>
        /// <param name="Broadcaster_ID">The broadcaster user ID for the channel you want to get ban notifications for.</param>
        /// <returns>An object of Channel_Ban. Use this object to access the EventHandler.</returns>
        public Channel_Ban Add_Banns(string Broadcaster_ID)
        {
            var tmp = new Channel_Ban(this, Broadcaster_ID, Logger);
            WS_Start();
            return tmp;
        }

        /// <summary>
        /// The channel.unban subscription type sends a notification when a viewer is unbanned from the specified channel.
        /// <para>Requiered scope: <em>channel:moderate</em></para>
        /// </summary>
        /// <param name="Broadcaster_ID">The broadcaster user ID for the channel you want to get unban notifications for.</param>
        /// <returns>An object of Channel_Unban. Use this object to access the EventHandler.</returns>
        public Channel_Unban Add_Unbanns(string Broadcaster_ID)
        {
            var tmp = new Channel_Unban(this, Broadcaster_ID, Logger);
            WS_Start();
            return tmp;
        }
    }
}
