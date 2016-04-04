﻿using System;
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
	/// <summary>Base class that contains all functionality</summary>
	public class HitBoxClientBase
	{
		/// <summary>Occurs when a user has logged in</summary>
		public event EventHandler<LoginEventArgs> LoggedIn;

		/// <summary>Logged in user</summary>
		public HitBoxUser User { get; set; }
		
		public bool EnableFast { get; set; } // TODO: fast option in HitBoxClient
		public bool IsLoggedIn { get { return _isLoggedIn; } }

		internal bool _isLoggedIn;

		internal string _authOrAccessToken;

		internal string _key;
		private string  _secret;

		/// <summary>Initializes class with application key and secret</summary>
		public HitBoxClientBase(string key, string secret)
		{
			_key	= key;
			_secret = secret;
		}
		
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
		internal async Task<string> GetUserFromToken(string token)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.UserFromToken + token));

			return (jmessage["user_name"] == null ? null : jmessage["user_name"].ToString());
		}

		/// <summary>Returns user's stream key. Returns null on error</summary>
		public async Task<string> GetStreamKey()
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.StreamKey + User.Username + "?authToken=" + _authOrAccessToken));

			if (jmessage["error"] != null)
				return null;

			return User.Username + "?key=" + jmessage["streamKey"].ToString();
		}

		/// <summary>Returns user access levels for specified channel</summary>
		public async Task<HitBoxAccessLevels> GetAccessLevels(string channel)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.AccessLevels + channel + "/" + _authOrAccessToken));

			if (jmessage["user_id"] == null)
				return new HitBoxAccessLevels()
				{
					IsFollower   = jmessage["isFollower"].ToObject<bool>(),
					IsSubscriber = jmessage["isSubscriber"].ToObject<bool>()
				};

			return new HitBoxAccessLevels()
			{
				UserID			= jmessage["user_id"].ToObject<int>(),
				AccessUserID	= jmessage["access_user_id"].ToObject<int>(),
				Settings		= jmessage["settings"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Account			= jmessage["account"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Livestreams		= jmessage["livestreams"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Broadcast		= jmessage["broadcast"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Videos			= jmessage["videos"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Recordings		= jmessage["recordings"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Statistics		= jmessage["statistics"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Inbox			= jmessage["inbox"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Revenues		= jmessage["revenues"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Chat			= jmessage["chat"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Following		= jmessage["following"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Teams			= jmessage["teams"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Subscriptions	= jmessage["subscriptions"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				Payments		= jmessage["payments"].ToString() == "admin" ? HitBoxRole.Admin : HitBoxRole.Anon,
				IsFollower		= jmessage["isFollower"].ToObject<bool>(),
				IsSubscriber	= jmessage["isSubscriber"].ToObject<bool>()
			};
		}

		/// <summary>Editors can run it</summary>
		public async Task<HitBoxCommBreak> RunCommercialBreak(string channel, int count = 1)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.POST(HitBoxEndpoint.CommercialBreak + channel + "/" + count, new JsonObject()
			{
				{ "user_name", JsonValue.CreateStringValue(User.Username) },
				{ "authToken", JsonValue.CreateStringValue(_authOrAccessToken) }
			}.Stringify()));

			if (jmessage["error"] != null)
				return null;

			return new HitBoxCommBreak
			{
				Count = jmessage["params"]["count"].ToObject<int>(),
				Delay = jmessage["params"]["delay"].ToString(),
				Timestamp = DateTime.FromFileTime(jmessage["params"]["timestamp"].ToObject<long>())
			};
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

			if (jmessage["error"] != null)
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

			if (jmessage["error"] != null)
				return false;

			return true;
		}

		/// <summary>Returns user with given username. Null if user was not found</summary>
		public async Task<HitBoxUser> GetUser(string username, bool useToken = false)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.User + username + (!useToken ? "" : "?authToken=" + _authOrAccessToken)));

			if (jmessage["user_name"] == null)
				return null;

			return new HitBoxUser()
			{
				Username		= jmessage["user_name"].ToString(),
				CoverUrl		= jmessage["user_cover"].ToString(),
				AvatarUrl		= jmessage["user_logo_small"].ToString(),
				IsBroadcaster	= jmessage["user_is_broadcaster"] == null ? false : jmessage["user_is_broadcaster"].ToObject<bool>(),
				Followers		= jmessage["followers"].ToObject<int>(),
				ID				= jmessage["user_id"].ToObject<int>(),
				MediaID			= jmessage["user_media_id"] == null ? 0 : jmessage["user_media_id"].ToObject<int>(),
				IsLive			= jmessage["is_live"] != null ? (jmessage["is_live"].ToString() == "0" ? false : true) : (jmessage["media_is_live"].ToString() == "0" ? false : true),
				LiveSince		= jmessage["live_since"] != null ? DateTime.Parse(jmessage["live_since"].ToString()) : DateTime.Parse(jmessage["media_live_since"].ToString()),
				Twitter			= jmessage["twitter_account"] == null ? null : jmessage["twitter_account"].ToString(),
				Email			= jmessage["user_email"] == null ? null : jmessage["user_email"].ToString()
			};
		}

		/// <summary>Returns null if channel has no followers</summary>
		public static async Task<IList<HitBoxFollower>> GetFollowers(string channel, int offset = 0, int limit = 10)
		{
			List<HitBoxFollower> followers = new List<HitBoxFollower>();

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Followers + channel + "?offset=" + offset + "&limit=" + limit));

			if (jmessage["error"] != null)
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

			if (jmessage["error"] != null)
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

			if (jmessage["error"] != null)
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

			if (jmessage["error"] != null)
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

		/// <summary>Returns null if user have no panels</summary>
		public static async Task<IList<HitBoxProfilePanel>> GetProfilePanels(string channel) // TODO: profile panels
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.ProfilePanels + channel));

			if (!jmessage["profile"].HasValues)
				return null;

			IList<HitBoxProfilePanel> panels = new List<HitBoxProfilePanel>();

			foreach(JToken jpanel in jmessage["profile"]["panels"])
			{
				panels.Add(new HitBoxProfilePanel
				{
					ID			= jpanel["id"].ToObject<int>(),
					Headline	= jpanel["headline"].ToString(),
					Content		= jpanel["content"].ToString(),
					ImageLink	= jpanel["link"].ToString(),
					ImageUrl	= jpanel["image"].ToString()
				});
			}

			return panels;
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

			if (jmessage["category"] == null)
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

		/// <summary>Returns null if no videos was found</summary>
		public static async Task<IList<HitBoxVideo>> GetVideos(int gameID = 0, int start = 0, int limit = 10, bool weekly = true)
		{
			List<HitBoxVideo> videos = new List<HitBoxVideo>(limit);

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Videos + "?filter=" + (weekly ? "weekly" : "popular") + "&game=" + gameID + "&start=" + start + "&limit=" + limit));

			if (jmessage["error"] != null)
				return null;

			IList<HitBoxMediaProfile> profiles = null;

			foreach (JObject jvideo in jmessage["video"])
			{
				JArray jprofiles = JArray.Parse(System.Text.RegularExpressions.Regex.Unescape(jvideo["media_profiles"].ToString()));

				profiles = new List<HitBoxMediaProfile>(jprofiles.Count);

				foreach (JToken jprofile in jprofiles)
				{
					profiles.Add(new HitBoxMediaProfile
					{
						Url		= jprofile["url"].ToString(),
						Height	= jprofile["height"].ToObject<int>(),
						Bitrate = jprofile["bitrate"].ToObject<int>()
					});
				}

				videos.Add(new HitBoxVideo
				{
					ID			 = jvideo["media_id"].ToObject<int>(),
					Title		 = jvideo["media_title"].ToString(),
					Views		 = jvideo["media_views"].ToObject<int>(),
					Description  = jvideo["media_description"].ToString(),
					DateAdded	 = DateTime.Parse(jvideo["media_date_added"].ToString()),
					Duration	 = jvideo["media_duration_format"].ToString(),
					ThumbnailUrl = jvideo["media_thumbnail"].ToString(),
					MediaFile	 = jvideo["media_file"].ToString(),
					Profiles	 = profiles,
					Game = new HitBoxGame
					{
						ID			 = jvideo["category_id"].ToString() == "" ? 0 : jvideo["category_id"].ToObject<int>(),
						Name		 = jvideo["category_name"].ToString(),
						Viewers		 = jvideo["category_viewers"].ToString() == "" ? 0 : jvideo["category_viewers"].ToObject<int>(),
						ChannelCount = jvideo["category_channels"].ToString() == "" ? 0 : jvideo["category_channels"].ToObject<int>(),
						LogoUrl		 = jvideo["category_logo_large"].ToString(),
						SeoKey		 = jvideo["category_seo_key"].ToString()
					},
					Channel = new HitBoxChannel
					{
						Recordings  = jvideo["channel"]["recordings"].ToObject<int>(),
						Videos		= jvideo["channel"]["videos"].ToObject<int>(),
						Teams		= jvideo["channel"]["teams"].ToObject<int>(),
						User = new HitBoxUser
						{
							ID			= jvideo["channel"]["user_id"].ToObject<int>(),
							Username	= jvideo["channel"]["user_name"].ToString(),
							Followers	= jvideo["channel"]["followers"].ToObject<int>(),
							MediaID		= jvideo["channel"]["user_media_id"].ToObject<int>(),
							IsLive		= jvideo["channel"]["media_is_live"].ToString() == "1",
							LiveSince	= DateTime.Parse(jvideo["channel"]["media_live_since"].ToString()),
							AvatarUrl	= jvideo["channel"]["user_logo_small"].ToString(),
							CoverUrl	= jvideo["channel"]["user_cover"].ToString(),
							Twitter		= jvideo["channel"]["twitter_account"].ToString()
						}
					}
				});
			}

			return videos;
		}
		
		/// <summary>Returns null if no video was found</summary>
		public async Task<HitBoxVideo> GetVideo(int id, bool useToken = false)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Video + id + (useToken ? "?authToken=" + _authOrAccessToken : "")));

			if (jmessage["error"] != null)
				return null;

			JToken jvideo = jmessage["video"][0];

			IList<HitBoxMediaProfile> profiles = new List<HitBoxMediaProfile>();

			JArray jprofiles = JArray.Parse(System.Text.RegularExpressions.Regex.Unescape(jvideo["media_profiles"].ToString()));

			for(int i = 0; i < jprofiles.Count; i++)
			{
				profiles.Add(new HitBoxMediaProfile
				{
					Url		= jprofiles[i]["url"].ToString(),
					Height	= jprofiles[i]["height"].ToObject<int>(),
					Bitrate = jprofiles[i]["bitrate"].ToObject<int>()
				});
			}

			return new HitBoxVideo
			{
				ID			 = jvideo["media_id"].ToObject<int>(),
				Title		 = jvideo["media_title"].ToString(),
				Description  = jvideo["media_description"].ToString(),
				DateAdded	 = DateTime.Parse(jvideo["media_date_added"].ToString()),
				Duration	 = jvideo["media_duration_format"].ToString(),
				Views		 = jvideo["media_views"].ToObject<int>(),
				ThumbnailUrl = jvideo["media_thumbnail"].ToString(),
				MediaFile	 = jvideo["media_file"].ToString(),
				Profiles	 = profiles,
				Game = new HitBoxGame
				{
					Name		 = jvideo["category_name"].ToString(),
					ID			 = jvideo["category_id"].ToString() == "" ? 0 : jvideo["category_id"].ToObject<int>(),
					Viewers		 = jvideo["category_viewers"].ToString() == "" ? 0 : jvideo["category_viewers"].ToObject<int>(),
					ChannelCount = jvideo["category_channels"].ToString() == "" ? 0 : jvideo["category_channels"].ToObject<int>(),
					LogoUrl		 = jvideo["category_logo_large"].ToString(),
					SeoKey		 = jvideo["category_seo_key"].ToString()
				},
				Channel = new HitBoxChannel
				{
					Recordings	= jvideo["channel"]["recordings"].ToObject<int>(),
					Videos		= jvideo["channel"]["videos"].ToObject<int>(),
					Teams		= jvideo["channel"]["teams"].ToObject<int>(),
					User = new HitBoxUser
					{
						ID			= jvideo["channel"]["user_id"].ToObject<int>(),
						Username	= jvideo["channel"]["user_name"].ToString(),
						IsLive		= jvideo["channel"]["media_is_live"].ToString() == "1",
						LiveSince	= DateTime.Parse(jvideo["channel"]["media_live_since"].ToString()),
						MediaID		= jvideo["channel"]["user_media_id"].ToObject<int>(),
						AvatarUrl	= jvideo["channel"]["user_logo_small"].ToString(),
						CoverUrl	= jvideo["channel"]["user_cover"].ToString(),
						Followers	= jvideo["channel"]["followers"].ToObject<int>(),
						Twitter		= jvideo["channel"]["twitter_account"].ToString()
					}
				}
			};
		}

		public async Task<bool> CreateVideo() // TODO: CreateVideo method
		{
			return false;
		}

		public void UpdateVideo() // TODO: UpdateVideo method
		{
			
		}

		/// <summary>Returns null if no livestreams was found</summary>
		public async Task<IList<HitBoxLivestream>> GetLivestreams(int gameID = 0, int start = 0, int limit = 10, bool liveonly = true, bool useToken = false)
		{
			IList<HitBoxLivestream> livestreams = new List<HitBoxLivestream>(limit);

			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Livestreams + "?liveonly=" + (liveonly ? "true" : "false") + "&game=" + gameID + "&start=" + start + "&limit=" + limit));

			if (jmessage["error"] != null)
				return null;

			IList<HitBoxMediaProfile> profiles = null;

			foreach (JObject jlive in jmessage["livestream"])
			{
				if (jlive["media_profiles"].HasValues)
				{
					JArray jprofiles = JArray.Parse(System.Text.RegularExpressions.Regex.Unescape(jlive["media_profiles"].ToString()));

					profiles = new List<HitBoxMediaProfile>(jprofiles.Count);

					foreach (JToken jprofile in jprofiles)
					{
						profiles.Add(new HitBoxMediaProfile
						{
							Url		= jprofile["url"].ToString(),
							Height	= jprofile["height"].ToObject<int>(),
							Bitrate = jprofile["bitrate"].ToObject<int>()
						});
					}
				}

				livestreams.Add(new HitBoxLivestream
				{
					ID				= jlive["media_id"].ToObject<int>(),
					Title			= jlive["media_status"].ToString(),
					Viewers			= jlive["media_views"].ToObject<int>(),
					MediaFile		= jlive["media_file"].ToString(),
					Profiles		= profiles,
					IsLive			= jlive["media_is_live"].ToString() == "1",
					IsChatEnabled	= jlive["media_chat_enabled"].ToString() == "1",
					Countries		= jlive["media_countries"].HasValues ? jlive["media_countries"].ToObject<List<string>>() : null,
					Game = new HitBoxGame
					{
						ID				= jlive["category_id"].ToString() == "" ? 0 : jlive["category_id"].ToObject<int>(),
						Name			= jlive["category_name"].ToString(),
						Viewers			= jlive["category_viewers"].ToString() == "" ? 0 : jlive["category_viewers"].ToObject<int>(),
						ChannelCount	= jlive["category_channels"].ToString() == "" ? 0 : jlive["category_channels"].ToObject<int>(),
						LogoUrl			= jlive["category_logo_large"].ToString(),
						SeoKey			= jlive["category_seo_key"].ToString()
					},
					ThumbnailUrl = jlive["media_thumbnail"].ToString(),
					Channel = new HitBoxChannel
					{
						Recordings	= jlive["channel"]["recordings"].ToObject<int>(),
						Videos		= jlive["channel"]["videos"].ToObject<int>(),
						Teams		= jlive["channel"]["teams"].ToObject<int>(),
						User = new HitBoxUser
						{
							ID			= jlive["channel"]["user_id"].ToObject<int>(),
							Username	= jlive["channel"]["user_name"].ToString(),
							Followers	= jlive["channel"]["followers"].ToObject<int>(),
							MediaID		= jlive["channel"]["user_media_id"].ToObject<int>(),
							IsLive		= jlive["channel"]["media_is_live"].ToString() == "1",
							AvatarUrl	= jlive["channel"]["user_logo_small"].ToString(),
							CoverUrl	= jlive["channel"]["user_cover"].ToString(),
							LiveSince	= DateTime.Parse(jlive["channel"]["media_live_since"].ToString()),
							Twitter		= jlive["channel"]["twitter_account"].ToString()
						}
					}
				});
			}

			return livestreams;
		}

		public async Task<HitBoxLivestream> GetLivestream(string channel, bool useToken = false)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.Livestream + channel + (useToken ? "?authToken=" + _authOrAccessToken : "")));

			if (jmessage["error"] != null)
				return null;

			JToken jlive = jmessage["livestream"][0];

			IList<HitBoxMediaProfile> profiles = null;

			if (jlive["media_profiles"].HasValues)
			{
				JArray jprofiles = JArray.Parse(System.Text.RegularExpressions.Regex.Unescape(jlive["media_profiles"].ToString()));

				profiles = new List<HitBoxMediaProfile>(jprofiles.Count);

				foreach (JToken jprofile in jprofiles)
				{
					profiles.Add(new HitBoxMediaProfile
					{
						Url = jprofile["url"].ToString(),
						Height = jprofile["height"].ToObject<int>(),
						Bitrate = jprofile["bitrate"].ToObject<int>()
					});
				}
			}

			return new HitBoxLivestream
			{
				ID = jlive["media_id"].ToObject<int>(),
				Title = jlive["media_status"].ToString(),
				Viewers = jlive["media_views"].ToObject<int>(),
				MediaFile = jlive["media_file"].ToString(),
				Profiles = profiles,
				IsLive = jlive["media_is_live"].ToString() == "1",
				IsChatEnabled = jlive["media_chat_enabled"].ToString() == "1",
				Countries = jlive["media_countries"].HasValues ? jlive["media_countries"].ToObject<List<string>>() : null,
				Game = new HitBoxGame
				{
					ID = jlive["category_id"].ToString() == "" ? 0 : jlive["category_id"].ToObject<int>(),
					Name = jlive["category_name"].ToString(),
					Viewers = jlive["category_viewers"].ToString() == "" ? 0 : jlive["category_viewers"].ToObject<int>(),
					ChannelCount = jlive["category_channels"].ToString() == "" ? 0 : jlive["category_channels"].ToObject<int>(),
					LogoUrl = jlive["category_logo_large"].ToString(),
					SeoKey = jlive["category_seo_key"].ToString()
				},
				ThumbnailUrl = jlive["media_thumbnail"].ToString(),
				Channel = new HitBoxChannel
				{
					Recordings = jlive["channel"]["recordings"].ToObject<int>(),
					Videos = jlive["channel"]["videos"].ToObject<int>(),
					Teams = jlive["channel"]["teams"].ToObject<int>(),
					User = new HitBoxUser
					{
						ID = jlive["channel"]["user_id"].ToObject<int>(),
						Username = jlive["channel"]["user_name"].ToString(),
						Followers = jlive["channel"]["followers"].ToObject<int>(),
						MediaID = jlive["channel"]["user_media_id"].ToObject<int>(),
						IsLive = jlive["channel"]["media_is_live"].ToString() == "1",
						AvatarUrl = jlive["channel"]["user_logo_small"].ToString(),
						CoverUrl = jlive["channel"]["user_cover"].ToString(),
						LiveSince = DateTime.Parse(jlive["channel"]["media_live_since"].ToString()),
						Twitter = jlive["channel"]["twitter_account"].ToString()
					}
				}
			};
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

		/// <summary>Returns chat servers</summary>
		public static async Task<IList<string>> GetChatServers()
		{
			JArray jmessage = JArray.Parse(await Web.GET(HitBoxEndpoint.ChatServers));

			IList<string> servers = new List<string>();

			foreach(JToken jserver in jmessage)
			{
				servers.Add(jserver["server_ip"].ToString());
			}

			return servers;
		}

		/// <summary>Returns servers for "viewer"</summary>
		public static async Task<IList<string>> GetViewerServers()
		{
			JArray jmessage = JArray.Parse(await Web.GET(HitBoxEndpoint.ViewerServers));

			IList<string> servers = new List<string>();

			foreach (JToken jserver in jmessage)
			{
				servers.Add(jserver["server_ip"].ToString());
			}

			return servers;
		}

		/// <summary>Returns a chat server id</summary>
		public static async Task<string> GetChatServerSocketID(string serverIP)
		{
			return (await Web.GET("http://" + serverIP + "/socket.io/1/")).Split(':')[0];
		}

		/// <summary>Returns all possible chat colors you can use</summary>
		/// <returns></returns>
		public static async Task<IList<string>> GetChatColors()
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitBoxEndpoint.ChatColors));

			IList<string> colors = new List<string>(100);

			foreach(JToken jcolor in jmessage["colors"])
			{
				colors.Add(jcolor.ToString());
			}

			return colors;
		}

		/// <summary>Creates a new LivestreamViewer. If "auth" is not true, then you are viewing a livestream as guest/anonymous</summary>
		public HitBoxLivestreamViewer CreateLivestreamViewer(string channel, bool auth = false)
		{
			if(!_isLoggedIn && auth)
				throw new HitBoxException(HitBoxExceptionList.NotLoggedIn);

			if (_isLoggedIn && auth)
				return new HitBoxLivestreamViewer(new HitBoxLivestreamViewer.Parameters
				{
					Channel = channel,
					Username = User.Username,
					Token = _authOrAccessToken
				});

			return new HitBoxLivestreamViewer(new HitBoxLivestreamViewer.Parameters
			{
				Channel = channel
			});
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
