using P90Ez.Twitch.API.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.API
{
    /// <summary>
    /// <strong>Contains commonly used methods for quicker library and api usage.</strong> Use the Endpoints namespace to get access to all endpoints and features.
    /// <para/><strong>Usage:</strong> Create on object of this class and provide your generated credentials.
    /// </summary>
    public class SimplifiedRequests
    {
        private protected Login.Credentials credentials;
        /// <summary>
        /// Create on object of this class to get access to the simplified and comonly used request functions.
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        public SimplifiedRequests(Login.Credentials credentials)
        {
            this.credentials = credentials;
        }

        #region Get User ID
        /// <summary>
        /// Uses GetStreams and GetUsers endpoints to get the id of the specified broadcaster.
        /// <para/>Don't worry about calling this function multiple times. Requested IDs will be cached!
        /// <para/>Note: A broadcaster ID is the same as the user ID.
        /// </summary>
        /// <param name="BroadcasterName">Display name or preferebly login name.</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>The Id of the specified broadcaster. Returns -1 if the broadcaster could not be found.</returns>
        public long GetBroadcasterID(string BroadcasterName, bool skipCache = false)
        {
            return GetUserID(BroadcasterName, skipCache);
        }
        /// <summary>
        /// Uses GetStreams and GetUsers endpoints to get the id of the specified user.
        /// <para/>Don't worry about calling this function multiple times. Requested IDs will be cached!
        /// </summary>
        /// <param name="UserName">Display name or preferebly login name.</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>The Id of the specified user. Returns -1 if the user could not be found.</returns>
        public long GetUserID(string UserName, bool skipCache = false)
        {
            UserName = UserName.ToLower();
            if(!skipCache && UserIDCache.ContainsKey(UserName))
            {
                return UserIDCache[UserName]; //Read ID from cache.
            }
            long UserID = GetUserIDCacheless(UserName);
            if(UserID != -1)
                UserIDCache[UserName] = UserID; //Save ID to cache only if request was successful.
            return UserID;
        }
        /// <summary>
        /// Only used by GetUserID function!! Key: username (lowercase), Value: UserID
        /// </summary>
        private Dictionary<string, long> UserIDCache = new Dictionary<string, long>();
        private long GetUserIDCacheless(string UserName)
        {
            UserName = UserName.ToLower();
            bool isSuccessful = false; int httpStatuscode = -1;

            //Try to get user via GetStreams -> may not return data when user is not live.
            var response = GetStreams.Go(credentials, new GetStreams.QueryParams() { user_login = new List<string>() { UserName } }, out isSuccessful, out httpStatuscode);
            if(isSuccessful && (httpStatuscode == 200 || httpStatuscode == -200) && response != null && response.data != null)
            {
                foreach(var data in response.data)
                {
                    if (data.user_login.ToLower() == UserName)
                        return Convert.ToInt64(data.user_id); //return id when exact match was found
                    if (data.user_name.ToLower() == UserName)
                        return Convert.ToInt64(data.user_id); //return id when exact match was found
                }
            }

            isSuccessful = false; httpStatuscode = -1;

            //Try to get user via GetUsers
            var anotherresponse = GetUsers.Go(credentials, new GetUsers.QueryParams() { login = new List<string>() { UserName } },out isSuccessful, out httpStatuscode);
            if (isSuccessful && (httpStatuscode == 200 || httpStatuscode == -200) && anotherresponse != null && anotherresponse.data != null)
            {
                foreach (var data in anotherresponse.data)
                {
                    if (data.login.ToLower() == UserName)
                        return Convert.ToInt64(data.id); //return id when exact match was found
                    if (data.display_name.ToLower() == UserName)
                        return Convert.ToInt64(data.id); //return id when exact match was found
                }
            }

            //return -1 when user could not be found.
            return -1;
        }
        #endregion
        #region Chat Moderation
        #region Delete Chat Message
        /// <summary>
        /// Uses the DeleteChatMessage endpoint to delete a chat message.
        /// <para>Required scope: <em>moderator:manage:chat_messages</em></para>
        /// </summary>
        /// <returns>True if message was successfully deleted.</returns>
        public bool DeleteChatMessage(long BroadcasterID, string MessageID)
        {
            Endpoints.DeleteChatMessage.DeleteMessage(credentials, MessageID, BroadcasterID.ToString(), out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 204;
        }
        #endregion
        #region Timeout User
        /// <summary>
        /// Uses the BanUser endpoint to timeout a user.
        /// <para>Required scope: <em>moderator:manage:banned_users</em></para>
        /// </summary>
        /// <returns>True if user was successfully timeouted. False if the request was not successful, or the user was already timeouted/banned.</returns>
        public bool TimeoutUser(long BroadcasterID, long UserID, int Duration, string reason = "")
        {
            Endpoints.BanUser.TimeOut(credentials, BroadcasterID.ToString(), UserID, Duration, reason, out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        /// <summary>
        /// Uses the UnbanUser endpoint to untimeout a user.
        /// <para>Required scope: <em>moderator:manage:banned_users</em></para>
        /// </summary>
        /// <returns>True if user was successfully untimeouted. False if the request was not successful, or the user was not timeouted/banned.</returns>
        public bool UntimeoutUser(long BroadcasterID, long UserID)
        {
            return UnbanUser(BroadcasterID, UserID);
        }
        #endregion
        #region Ban User
        /// <summary>
        /// Uses the BanUser endpoint to ban a user.
        /// <para>Required scope: <em>moderator:manage:banned_users</em></para>
        /// </summary>
        /// <returns>True if user was successfully banned. False if the request was not successful, or the user was already banned.</returns>
        public bool BanUser(long BroadcasterID, long UserID, string reason = "")
        {
            Endpoints.BanUser.Ban(credentials, BroadcasterID.ToString(), UserID, reason, out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        /// <summary>
        /// Uses the UnbanUser endpoint to untimeout a user.
        /// <para>Required scope: <em>moderator:manage:banned_users</em></para>
        /// </summary>
        /// <returns>True if user was successfully untimeouted. False if the request was not successful, or the user was not timeouted/banned.</returns>
        public bool UnbanUser(long BroadcasterID, long UserID)
        {
            Endpoints.UnbanUser.Go(credentials, BroadcasterID.ToString(), UserID, out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 204;
        }
        #endregion
        #region Clear Chat
        /// <summary>
        /// Uses the DeleteChatMessage endpoint to clear the chat.
        /// <para>Required scope: <em>moderator:manage:chat_messages</em></para>
        /// </summary>
        /// <returns>True if chat was successfully cleared.</returns>
        public bool ClearChat(long BroadcasterID) 
        {
            Endpoints.DeleteChatMessage.ClearChat(credentials, BroadcasterID.ToString(), out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 204;
        }
        #endregion
        #region Chat Settings
        /// <summary>
        /// Uses the UpdateChatSettings endpoint to enable/disable emote only chat mode.
        /// <para>Required scope: <em>moderator:manage:chat_settings</em></para>
        /// </summary>
        /// <returns>True if chat mode was successfully updated.</returns>
        public bool EmoteChat(long BroadcasterID, bool Enabled)
        {
            UpdateChatSettings.EmoteChat(credentials, BroadcasterID.ToString(), Enabled, out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        /// <summary>
        /// Uses the UpdateChatSettings endpoint to enable/disable follower only chat mode.
        /// <para>Required scope: <em>moderator:manage:chat_settings</em></para>
        /// </summary>
        /// <returns>True if chat mode was successfully updated.</returns>
        public bool FollowerChat(long BroadcasterID, bool Enabled, int MinFollowTime = -1)
        {
            bool isSuccess = false; int httpStatuscode = -1;
            if (MinFollowTime == -1)
                UpdateChatSettings.FollowerMode(credentials, BroadcasterID.ToString(), Enabled, out isSuccess, out httpStatuscode);
            else
                UpdateChatSettings.FollowerMode(credentials, BroadcasterID.ToString(), Enabled, MinFollowTime, out isSuccess, out httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        /// <summary>
        /// Uses the UpdateChatSettings endpoint to enable/disable subscriber only chat mode.
        /// <para>Required scope: <em>moderator:manage:chat_settings</em></para>
        /// </summary>
        /// <returns>True if chat mode was successfully updated.</returns>
        public bool SubscriberChat(long BroadcasterID, bool Enabled)
        {
            UpdateChatSettings.SubscriberMode(credentials, BroadcasterID.ToString(), Enabled, out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        /// <summary>
        /// Uses the UpdateChatSettings endpoint to enable/disable slow chat mode.
        /// <para>Required scope: <em>moderator:manage:chat_settings</em></para>
        /// </summary>
        /// <returns>True if chat mode was successfully updated.</returns>
        public bool SlowMode(long BroadcasterID, bool Enabled, int WaitTime = -1)
        {
            bool isSuccess = false; int httpStatuscode = -1;
            if (WaitTime == -1)
                UpdateChatSettings.SlowMode(credentials, BroadcasterID.ToString(), Enabled, out isSuccess, out httpStatuscode);
            else
                UpdateChatSettings.SlowMode(credentials, BroadcasterID.ToString(), Enabled, WaitTime, out isSuccess, out httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        /// <summary>
        /// Uses the UpdateChatSettings endpoint to enable/disable unique chat mode.
        /// <para>Required scope: <em>moderator:manage:chat_settings</em></para>
        /// </summary>
        /// <returns>True if chat mode was successfully updated.</returns>
        public bool UniqueMode(long BroadcasterID, bool Enabled)
        {
            UpdateChatSettings.UniqueMode(credentials, BroadcasterID.ToString(), Enabled, out bool isSuccess, out int httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        /// <summary>
        /// Uses the UpdateChatSettings endpoint to enable/disable non mod chat delay.
        /// <para>Required scope: <em>moderator:manage:chat_settings</em></para>
        /// </summary>
        /// <param name="delay">The amount of time, in seconds, that messages are delayed before appearing in chat. Possible values are: 2, 4, 6 (seconds)</param>
        /// <returns>True if chat mode was successfully updated.</returns>
        public bool ModerationChatDelay(long BroadcasterID, bool Enabled, int delay = -1)
        {
            bool isSuccess = false; int httpStatuscode = -1;
            if (delay == -1)
                UpdateChatSettings.ModerationChatDelay(credentials, BroadcasterID.ToString(), Enabled, out isSuccess, out httpStatuscode);
            else
                UpdateChatSettings.ModerationChatDelay(credentials, BroadcasterID.ToString(), Enabled, delay, out isSuccess, out httpStatuscode);
            return isSuccess && httpStatuscode == 200;
        }
        #endregion
        #endregion
    }
}
