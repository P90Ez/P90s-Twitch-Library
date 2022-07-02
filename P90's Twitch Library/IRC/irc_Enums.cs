using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.IRC
{
    /// <summary>
    /// Contains useful enums, which are used within this library.
    /// </summary>
    public class irc_Enums
    {
        public enum Permissionlevels
        {
            /// <summary>
            /// A normal user without special permission.
            /// </summary>
            User,
            /// <summary>
            /// A user who is subscribed to the broadcaster.
            /// </summary>
            Sub,
            /// <summary>
            /// A user which is VIP of this channel.
            /// </summary>
            Vip,
            /// <summary>
            /// Channel/Chat moderator
            /// </summary>
            Mod,
            /// <summary>
            /// Broadcaster.
            /// </summary>
            Streamer,
        }
        /// <summary>
        /// Special Twitch usertypes for staff, global mods or admins.
        /// </summary>
        public enum UserTypes
        {
            /// <summary>
            /// Not a special user
            /// </summary>
            NormalUser,
            /// <summary>
            /// Twitch employees
            /// </summary>
            TwitchStaff,
            /// <summary>
            /// Global moderators
            /// </summary>
            GlobalMod,
            /// <summary>
            /// Twitch admins
            /// </summary>
            TwitchAdmin,
        }
    }
}
