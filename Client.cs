using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Data.Json;

using Newtonsoft.Json.Linq;

using System.Diagnostics;

namespace HitboxUWP8
{
	public class HitBoxClient
	{
		public event EventHandler<LoginEventArgs> LoggedIn;

		public HitBoxUser User { get; set; }
		
		public string AppKey { get { return _key; } }
		public bool IsLoggedIn { get { return _isLoggedIn; } }

		internal bool _isLoggedIn;

		internal string _authOrAccessToken;

		private string _key;
		private string _secret;

		public static class ChatServerInfo
		{
			public static string ServerIP;
			public static string SocketID;
		}

		public HitBoxClient(string key, string secret)
		{
			_key	= key;
			_secret = secret;
		}

		/// <summary>Login with modal window</summary>
		public void Login(bool force = false)
		{
			if(!_isLoggedIn)
				(Window.Current.Content as Frame).Navigate(typeof(LoginPage), new object[] { force, this });
		}

		/// <summary>Login with auth or access token</summary>
		public async void Login(string authOrAccessToken)
		{
			if (_isLoggedIn)
				return;

			string response = await Web.GET(HitBoxEndpoint.TokenValidation + _key + "?token=" + authOrAccessToken);

			bool error = JObject.Parse(response)["error"].ToObject<bool>();

			if (error)
			{
				OnLoggedIn(new LoginEventArgs() { Method = LoginEventArgs.Methods.Another, State = LoginEventArgs.States.InvalidToken });
				return;
			}

			_authOrAccessToken = authOrAccessToken;

			User = await GetUser(await GetUserFromToken(_authOrAccessToken), true);

			_isLoggedIn = true;

			OnLoggedIn(new LoginEventArgs() { Method = LoginEventArgs.Methods.Another, State = LoginEventArgs.States.OK });
		}

		public void Logout()
		{
			_isLoggedIn = false;
			User = null;
		}

		/// <summary>Throws exception if auth was failed</summary>
		internal async Task<string> GetAccessToken(string requestToken)
		{
			string hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(_key + _secret));

			string response = await Web.POST(HitBoxEndpoint.ExchangeRequest, new JsonObject
			{
				{ "request_token", JsonValue.CreateStringValue(requestToken) },
				{ "app_token", JsonValue.CreateStringValue(_key) },
				{ "hash", JsonValue.CreateStringValue(hash) }
			}.Stringify());

			if (response.Equals("authentication_failed", StringComparison.CurrentCultureIgnoreCase))
				throw new HitBoxException(HitBoxExceptionList.AuthFailed);

			return JObject.Parse(response)["access_token"].ToString();
		}

		/// <summary>Returns null if no user was found</summary>
		internal static async Task<string> GetUserFromToken(string token)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.UserFromToken + token));

			return (jmessage["user_name"].IsNull() ? null : jmessage["user_name"].ToString());
		}

		/// <summary>Returns user's stream key. Returns null on error</summary>
		public async Task<string> GetStreamKey()
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.StreamKey + User.Username + "?authToken=" + _authOrAccessToken));

			if (!jmessage["error"].IsNull())
				return null;

			return User.Username + "?key=" + jmessage["streamKey"].ToString();
		}

		/// <summary>Returns access levels for specified channel</summary>
		public async Task<HitBoxAccessLevels> GetAccessLevels(string channel)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.AccessLevels + channel + "/" + _authOrAccessToken));

			if (jmessage["user_id"].IsNull())
				return new HitBoxAccessLevels()
				{
					IsFollower   = jmessage["isFollower"].ToObject<bool>(),
					IsSubscriber = jmessage["isSubscriber"].ToObject<bool>()
				};

			return new HitBoxAccessLevels()
			{
				UserID			= jmessage["user_id"].ToObject<int>(),
				AccessUserID	= jmessage["access_user_id"].ToObject<int>(),
				Settings		= jmessage["settings"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Account			= jmessage["account"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Livestreams		= jmessage["livestreams"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Broadcast		= jmessage["broadcast"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Videos			= jmessage["videos"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Recordings		= jmessage["recordings"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Statistics		= jmessage["statistics"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Inbox			= jmessage["inbox"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Revenues		= jmessage["revenues"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Chat			= jmessage["chat"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Following		= jmessage["following"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Teams			= jmessage["teams"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Subscriptions	= jmessage["subscriptions"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				Payments		= jmessage["payments"].ToString() == "admin" ? HitBoxAccessLevels.AccessLevel.Admin : HitBoxAccessLevels.AccessLevel.Anon,
				IsFollower		= jmessage["isFollower"].ToObject<bool>(),
				IsSubscriber	= jmessage["isSubscriber"].ToObject<bool>()
			};
		}

		/// <summary>Editors can run it</summary>
		public async void RunCommercialBreak(string channel, int count = 1) // TODO: THIS
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.POST(HitBoxEndpoint.CommercialBreak + channel + "/" + count, new JsonObject()
			{
				{ "user_name", JsonValue.CreateStringValue(User.Username) },
				{ "authToken", JsonValue.CreateStringValue(_authOrAccessToken) }
			}.Stringify()));

			if (!jmessage["error"].IsNull())
				return;

			/*
			Response:
			{
			  "method":"commercialBreak",
			  "params":{
				"channel":"masta",
				"count":"1",
				"delay":"0",
				"token":"SuperSecret",
				"url":"http://hitbox.tv",
				"timestamp":1428528241
			  }
			}
			*/
		}

		/// <summary>Returns last commecrial break on given channel. Returns null if channel never run ads</summary>
		public static async Task<HitBoxLastCommBreak> GetLastCommercialBreak(string channel)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.CommercialBreak + channel));

			if (jmessage["seconds_ago"] == null)
				return null;

			return new HitBoxLastCommBreak()
			{
				Count		= jmessage["ad_count"].ToObject<int>(),
				SecondsAgo	= jmessage["seconds_ago"].ToObject<int>(),
				Timeout		= jmessage["timeout"].ToObject<int>()
			};
		}

		/// <summary>Returns true on success, false on error</summary>
		public async Task<bool> TwitterPost(string message)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.POST(HitBoxEndpoint.TwitterPost + "?authToken=" + _authOrAccessToken + "&user_name=" + User.Username, new JsonObject()
			{
				{ "user_name", JsonValue.CreateStringValue(User.Username) },
				{ "authToken", JsonValue.CreateStringValue(_authOrAccessToken) },
				{ "message",   JsonValue.CreateStringValue(message) }
			}.Stringify()));

			if (!jmessage["error"].IsNull())
				return false;

			return true;
		}

		/// <summary>Returns true on success, false on error</summary>
		public async Task<bool> FacebookPost(string message)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.POST(HitBoxEndpoint.FacebookPost + "?authToken=" + _authOrAccessToken + "&user_name=" + User.Username, new JsonObject()
			{
				{ "user_name", JsonValue.CreateStringValue(User.Username) },
				{ "authToken", JsonValue.CreateStringValue(_authOrAccessToken) },
				{ "message",   JsonValue.CreateStringValue(message) }
			}.Stringify()));

			if (!jmessage["error"].IsNull())
				return false;

			return true;
		}

		/// <summary>Returns user with given username. Null if user was not found</summary>
		public async Task<HitBoxUser> GetUser(string username, bool useToken = false)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.User + username + (!useToken ? "" : "?authToken=" + _authOrAccessToken)));

			if (jmessage["user_name"].IsNull())
				return null;

			return new HitBoxUser()
			{
				Username		= jmessage["user_name"].ToString(),
				CoverUrl		= jmessage["user_cover"].ToString(),
				AvatarUrl		= jmessage["user_logo_small"].ToString(),
				IsBroadcaster	= jmessage["user_is_broadcaster"].IsNull() ? false : jmessage["user_is_broadcaster"].ToObject<bool>(),
				Followers		= jmessage["followers"].ToObject<int>(),
				ID				= jmessage["user_id"].ToObject<int>(),
				MediaID			= jmessage["user_media_id"].IsNull() ? 0 : jmessage["user_media_id"].ToObject<int>(),
				IsLive			= !jmessage["is_live"].IsNull() ? (jmessage["is_live"].ToString() == "0" ? false : true) : (jmessage["media_is_live"].ToString() == "0" ? false : true),
				LiveSince		= !jmessage["live_since"].IsNull() ? DateTime.Parse(jmessage["live_since"].ToString()) : DateTime.Parse(jmessage["media_live_since"].ToString()),
				Twitter			= jmessage["twitter_account"].IsNull() ? null : jmessage["twitter_account"].ToString(),
				Email			= jmessage["user_email"].IsNull() ? null : jmessage["user_email"].ToString()
			};
		}

		/// <summary>Returns null if channel has no followers</summary>
		public static async Task<IList<HitBoxFollower>> GetFollowers(string channel, int offset = 0, int limit = 10)
		{
			List<HitBoxFollower> followers = new List<HitBoxFollower>();

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Followers + channel + "?offset=" + offset + "&limit=" + limit));

			if (!jmessage["error"].IsNull())
				return null;

			foreach(JObject jfollower in jmessage["followers"])
			{
				followers.Add(new HitBoxFollower()
				{
					UserID = jmessage["user_id"].ToObject<int>(),
					Username = jmessage["user_name"].ToString(),
					Followers = jmessage["followers"].ToObject<int>(),
					DateAdded = DateTime.Parse(jmessage["date_added"].ToString()),
					AvatarUrl = jmessage["user_logo_small"].ToString()
				});
			}

			return followers;
		}

		/// <summary>Returns null if user not follow anyone</summary>
		public static async Task<IList<HitBoxFollower>> GetFollowing(string user, int offset = 0, int limit = 10)
		{
			List<HitBoxFollower> following = new List<HitBoxFollower>();

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Following + "?user_name=" + user + "&offset=" + offset + "&limit=" + limit));

			if (!jmessage["error"].IsNull())
				return null;

			foreach (JObject jfollower in jmessage["following"])
			{
				following.Add(new HitBoxFollower()
				{
					UserID = jmessage["user_id"].ToObject<int>(),
					Username = jmessage["user_name"].ToString(),
					Followers = jmessage["followers"].ToObject<int>(),
					DateAdded = DateTime.Parse(jmessage["date_added"].ToString()),
					AvatarUrl = jmessage["user_logo_small"].ToString()
				});
			}

			return following;
		}

		/// <summary>True if user is following channel</summary>
		public static async Task<bool> CheckFollowingStatus(string channel, string user)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Following + channel + "?user_name=" + user));

			if (!jmessage["error"].IsNull())
				return false;

			return true;
		}

		/// <summary>False if user is already following channel</summary>
		public async Task<bool> Follow(string usernameOrUserID)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.POST(HitBoxEndpoint.Follow + "?authToken=" + _authOrAccessToken, new JsonObject()
			{
				{ "type", JsonValue.CreateStringValue("user") },
				{ "follow_id", JsonValue.CreateStringValue(usernameOrUserID) }
			}.Stringify()));

			if (jmessage["error"].ToObject<bool>())
				return false;
			else
				return true;
		}

		/// <summary>Unfollow a user</summary>
		public async Task<bool> Unfollow(int userID)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.DELETE(HitBoxEndpoint.Follow + "?type=user&authToken=" + _authOrAccessToken + "&follow_id=" + userID));

			if (!jmessage["error"].IsNull())
				return false;

			return true;
		}

		/// <summary>If subscriber returns true</summary>
		public async Task<bool> CheckSubscriptionStatus(string channel)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Subscription + channel + "/" + _authOrAccessToken));

			return jmessage["isSubscriber"].ToObject<bool>();
		}

		/// <summary>Returns Total Media Views for :channel</summary>
		public static async Task<int> GetTotalViews(string channel)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.TotalViews + channel));

			if (jmessage["total_live_views"].ToString() == "false")
				return 0;

			return jmessage["total_live_views"].ToObject<int>();
		}

		/// <summary>Returns live games (max = 100)</summary>
		public static async Task<IList<HitBoxGame>> GetGames(string searchQuery = null)
		{
			List<HitBoxGame> games = new List<HitBoxGame>();

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Games + "?liveonly=true&limit=100" + (searchQuery == null ? "" : "&q=" + searchQuery)));
			
			foreach (JObject jgame in jmessage["categories"])
			{
				games.Add(new HitBoxGame()
				{
					ID			 = jgame["category_id"].ToObject<int>(),
					Name		 = jgame["category_name"].ToString(),
					Viewers		 = jgame["category_viewers"].ToObject<int>(),
					ChannelCount = jgame["category_media_count"].ToObject<int>(),
					LogoUrl		 = jgame["category_logo_large"].ToString(),
					SeoKey		 = jgame["category_seo_key"].ToString()
				});
			}

			return games;
		}

		/// <summary>Returns game with given id. Returns null if game is not found</summary>
		public static async Task<HitBoxGame> GetGame(string id)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Game + id));

			if (jmessage["category"].IsNull())
				return null;

			return new HitBoxGame()
			{
				ID			 = jmessage["category"]["category_id"].ToObject<int>(),
				Name		 = jmessage["category"]["category_name"].ToString(),
				Viewers		 = jmessage["category"]["category_viewers"].ToObject<int>(),
				ChannelCount = jmessage["category"]["category_media_count"].ToObject<int>(),
				LogoUrl		 = jmessage["category"]["category_logo_large"].ToString()
			};
		}

		/// <summary>Returns null if no livestreams was found</summary>
		public async Task<List<HitBoxLivestream>> GetLivestreams(int gameID = 0, int start = 0, int limit = 10, bool useToken = false)
		{
			List<HitBoxLivestream> livestreams = new List<HitBoxLivestream>(); 

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Livestreams + "?fast=true&liveonly=true&game=" + gameID + "&start=" + start + "&limit=" + limit));

			if (!jmessage["error"].IsNull())
				return null;

			foreach(JObject jlive in jmessage["livestream"])
			{
				livestreams.Add(new HitBoxLivestream()
				{
					Username		= jlive["media_name"].ToString(),
					UsernameDisplay = jlive["media_display_name"].ToString(),
					Title			= jlive["media_status"].ToString(),
					Game			= jlive["category_name"].ToString(),
					Viewers			= jlive["media_views"].ToObject<int>(),
					Countries		= jlive["media_countries"].ToObject<List<string>>(),
					LiveSince		= DateTime.Parse(jlive["media_live_since"].ToString()),
					AvatarUrl		= jlive["user_logo_small"].ToString(),
					ID				= jlive["media_id"].ToObject<int>(),
					CoverUrl		= jlive["media_thumbnail"].ToString()
				});
			}

			return livestreams;
		}

		/// <summary>Returns null if no videos was found</summary>
		public static async Task<IList<HitBoxVideo>> GetVideos(int gameID = 0, int start = 0, int limit = 10, bool weekly = true)
		{
			List<HitBoxVideo> videos = new List<HitBoxVideo>();

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Videos + "?fast=true&filter=" + (weekly ? "weekly" : "popular") + "&game=" + gameID + "&start=" + start + "&limit=" + limit));

			if (!jmessage["error"].IsNull())
				return null;

			foreach(JObject jvideo in jmessage["video"])
			{
				videos.Add(new HitBoxVideo()
				{
					ID			= jvideo["media_id"].ToObject<int>(),
					Title		= jvideo["media_title"].ToString(),
					Description = jvideo["media_description"].ToString(),
					DateAdded	= DateTime.Parse(jvideo["media_date_added"].ToString()),
					Duration	= jvideo["media_duration_format"].ToString(),
					Views		= jvideo["media_views"].ToObject<int>(),
					CoverUrl	= jvideo["media_thumbnail"].ToString(),
					Game = new HitBoxGame()
					{
						Name = jvideo["category_name"].ToString(),
						ID	 = jvideo["category_id"].ToString() == "" ? 0 : jvideo["category_id"].ToObject<int>()
					},
					User = new HitBoxUser()
					{
						Username  = jvideo["media_user_name"].ToString(),
						ID		  = jvideo["media_user_id"].ToObject<int>(),
						AvatarUrl = jvideo["channel"]["user_logo_small"].ToString()
					}
				});
			}

			return videos;
		}

		public async Task<JsonHelper.LivestreamMediaObject> GetLivestreamMediaObject(string media_id)
		{
			return JsonHelper.GetLivestreamMediaObject(await Web.GET("http://www.hitbox.tv/api/player/config/live/" + media_id + "?redis=true&authToken=" + _authOrAccessToken + "&embed=false&qos=false&showHidden=true"));
		}

		/// <summary>Returns media status and viewer count for channel</summary>
		public static async Task<HitBoxMediaStatus> MediaStatus(string channel)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.MediaStatus + channel));

			if (jmessage["media_is_live"] == null)
				return null;

			return new HitBoxMediaStatus
			{
				IsLive  = jmessage["media_is_live"].ToString() == "1" ? true : false,
				Viewers = jmessage["media_views"].ToObject<int>()
			};
		}

		/// <summary>Returns a random chat server</summary>
		public static async Task<string> GetChatServer()
		{
			string response = await Web.GET(HitBoxEndpoint.ChatServers);
			
			try
			{
				JArray servers = JArray.Parse(response);
				return servers[new Random().Next(0, servers.Count - 1)]["server_ip"].ToString();
			}
			catch(Newtonsoft.Json.JsonReaderException) // Because hitbox api sometimes returns html instead of json
			{

				await Task.Delay(100);
				return await GetChatServer();
			}
		}

		/// <summary>Returns a chat server id</summary>
		public static async Task<string> GetChatServerSocketID(string serverIP)
		{
			return (await Web.GET("http://" + (await GetChatServer()) + "/socket.io/1/")).Split(':')[0];
		}

#region Handlers

		protected internal virtual void OnLoggedIn(LoginEventArgs e)
		{
			if (LoggedIn != null)
				LoggedIn(this, e);
		}

#endregion
	}
}
