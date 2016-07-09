using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Data.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HitboxUWP8
{
	/// <summary>Base class that contains all functionality</summary>
	public class HitboxClientBase : IDisposable
	{
		/// <summary>Occurs when a user has logged in</summary>
		public event EventHandler<HitboxLoginEventArgs> LoggedIn;

		/// <summary>Logged in user</summary>
		public HitboxUser User { get; set; }
		
		private bool EnableFast { get; set; } // TODO: fast option in HitBoxClient
		public bool IsLoggedIn { get { return isLoggedIn; } }

		internal bool isLoggedIn;

		internal string authOrAccessToken;

		internal string appKey;
		private string  appSecret;

		public HitboxClientBase() { }

		/// <summary>Initializes the class with application key and secret</summary>
		/// <param name="key">App key</param>
		/// <param name="secret">App secret key</param>
		public HitboxClientBase(string key, string secret)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (secret == null)
				throw new ArgumentNullException("secret");

			appKey	= key;
			appSecret = secret;
		}

		private void Login(string email, string password)
		{
			if (email == null)
				throw new ArgumentNullException("email");

			if (password == null)
				throw new ArgumentNullException("password");

			// TODO: basic login
		}

		/// <summary>Login through hitbox api page. Will open a new frame</summary>
		/// <param name="force">Enable force login</param>
		public void Login(bool force = false)
		{
			if(!isLoggedIn)
				(Window.Current.Content as Frame).Navigate(typeof(LoginPage), new object[] { force, this });
		}

		/// <summary>Login with auth or access token</summary>
		/// <param name="authOrAccessToken">auth token = access token</param>
		public async Task Login(string authOrAccessToken)
		{
			if (isLoggedIn)
				return;

			if (authOrAccessToken == null)
				throw new ArgumentNullException("authOrAccessToken");

			string username = await GetUserFromToken(authOrAccessToken);

			if (username == null)
			{
				OnLoggedIn(new HitboxLoginEventArgs { Method = HitboxLoginEventArgs.Methods.NotFirstTime, State = HitboxLoginEventArgs.States.InvalidToken });
				return;
			}

			this.authOrAccessToken = authOrAccessToken;

			User = await GetUser(username, true);

			isLoggedIn = true;

			OnLoggedIn(new HitboxLoginEventArgs { Method = HitboxLoginEventArgs.Methods.NotFirstTime, State = HitboxLoginEventArgs.States.OK });
		}
		
		/// <summary>Logout from the current client</summary>
		public void Logout()
		{
			isLoggedIn = false;

			Dispose();
		}

		/// <summary>Get access token from request token.</summary>
		/// <exception cref="HitboxException">Throws an exception if auth was failed</exception>
		internal async Task<string> GetAccessToken(string requestToken)
		{
			if (requestToken == null)
				throw new ArgumentNullException("requestToken");

			string hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(appKey + appSecret));

			string response = await Web.POST(HitboxEndpoint.ExchangeRequest, new JsonObject
			{
				{ "request_token", JsonValue.CreateStringValue(requestToken) },
				{ "app_token", JsonValue.CreateStringValue(appKey) },
				{ "hash", JsonValue.CreateStringValue(hash) }
			}.Stringify());

			if (response.Equals("authentication_failed", StringComparison.CurrentCultureIgnoreCase))
				throw new HitboxException(ExceptionList.AuthFailed);

			return JObject.Parse(response)["access_token"].ToString();
		}

		/// <summary>Get username from a token</summary>
		/// <returns>Returns null if no user was found</returns>
		internal async Task<string> GetUserFromToken(string token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.UserFromToken + token));

			return jmessage["user_name"].NotNullToString();
		}

		/// <summary>Check if the user token is valid</summary>
		private async Task<bool> CheckUserToken(string authOrAccessToken) // MAYBE: why it isn't working?
		{
			if (authOrAccessToken == null)
				throw new ArgumentNullException("authOrAccessToken");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.TokenValidation + appKey + "?token=" + authOrAccessToken));

			return jmessage["success"].ToObject<bool>();
		}

		/// <summary>Get user</summary>
		/// <param name="useToken">If true returns private user details</param>
		/// <returns>Null if user was not found</returns>
		public async Task<HitboxUser> GetUser(string username, bool useToken = false)
		{
			if (username == null)
				throw new ArgumentNullException("username");

			if (useToken && authOrAccessToken == null)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.User + username + (!useToken ? "" : "?authToken=" + authOrAccessToken)));

			if (jmessage["user_name"].IsNull())
				return null;

			return new HitboxUser
			{
				ID = jmessage["user_id"].ToObject<int>(),
				Username  = jmessage["user_name"].ToString(),
				CoverUrl  = jmessage["user_cover"].ToString(),
				AvatarUrlSmall = jmessage["user_logo_small"].ToString(),
				AvatarUrlLarge = jmessage["user_logo"].ToString(),
				Followers = jmessage["followers"].ToObject<int>(),
				IsLive    = !jmessage["is_live"].IsNull() ? jmessage["is_live"].ToValue<bool>() : jmessage["media_is_live"].ToValue<bool>(),
				LiveSince = !jmessage["live_since"].IsNull() ? DateTime.Parse(jmessage["live_since"].ToString()) : DateTime.Parse(jmessage["media_live_since"].ToString()),
				IsBroadcaster = jmessage["user_is_broadcaster"].ToValue<bool>(true),
				MediaID   = jmessage["user_media_id"].ToValue<int>(true),
				Twitter   = jmessage["twitter_account"].NotNullToString(),
				Email     = jmessage["user_email"].NotNullToString(),
				client    = this
			};
		}

		private bool EditUser(string oldUsername, string newUsername, bool twitterEnabled, string newTwitterAccount)
		{
			// TODO: EditUser

			return false;
		}

		/// <summary>Check if user has validated their email address</summary>
		public async Task<bool> CheckVerifiedEmail(string username)
		{
			if (username == null)
				throw new ArgumentNullException("username");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.EmailVerified + username));

			return jmessage["user"].IsNull() ? false : bool.Parse(jmessage["user"]["user_activated"].ToString());
		}

		/// <summary>Get user access levels for specified channel</summary>
		public async Task<HitboxAccessLevels> GetAccessLevels(string channel)
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.AccessLevels + channel + "/" + (authOrAccessToken ?? "")));

			if (jmessage["user_id"].IsNull())
				return new HitboxAccessLevels
				{
					IsFollower   = jmessage["isFollower"].ToObject<bool>(),
					IsSubscriber = jmessage["isSubscriber"].ToObject<bool>()
				};

			return new HitboxAccessLevels
			{
				UserID = jmessage["user_id"].ToObject<int>(),
				AccessUserID  = jmessage["access_user_id"].ToObject<int>(),
				IsFollower    = jmessage["isFollower"].ToObject<bool>(),
				IsSubscriber  = jmessage["isSubscriber"].ToObject<bool>(),
				Settings      = jmessage["settings"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Account       = jmessage["account"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Broadcast     = jmessage["broadcast"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Livestreams   = jmessage["livestreams"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Revenues      = jmessage["revenues"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Videos        = jmessage["videos"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Recordings    = jmessage["recordings"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Statistics    = jmessage["statistics"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Inbox         = jmessage["inbox"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Chat          = jmessage["chat"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Following     = jmessage["following"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Teams         = jmessage["teams"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Payments      = jmessage["payments"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon,
				Subscriptions = jmessage["subscriptions"].ToString() == "admin" ? HitboxRole.Admin : HitboxRole.Anon
			};
		}

		// MAYBE: Default Team
		// MAYBE: OAuth

		/// <summary>Get user stream key</summary>
		/// <returns>Returns null on error</returns>
		public async Task<string> GetStreamKey()
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.StreamKey + User.Username + "?authToken=" + authOrAccessToken));

			if (!jmessage["error"].IsNull())
				return null;

			return User.Username + "?key=" + jmessage["streamKey"].ToString();
		}

		/// <summary>Sets a new stream key for a channel. Editors can run this API</summary>
		/// <returns>Null on error</returns>
		public async Task<string> ResetStreamKey(string channel)
		{
			if (!isLoggedIn)
				throw new ArgumentNullException(ExceptionList.NotLoggedIn);

			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.PUT(HitboxEndpoint.StreamKey + channel + "?authToken=" + authOrAccessToken, ""));

			if (!jmessage["error"].IsNull())
				return null;

			return jmessage["streamKey"].ToString();
		}

		/// <summary>Run specified amount of commercial breaks. Editors can run it</summary>
		/// <returns>Returns null on error</returns>
		public async Task<HitboxCommBreak> RunCommercialBreak(string channel, int amount = 1)
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.POST(HitboxEndpoint.CommercialBreak + channel + "/" + amount, new JsonObject
			{
				{ "user_name", JsonValue.CreateStringValue(User.Username) },
				{ "authToken", JsonValue.CreateStringValue(authOrAccessToken) }
			}.Stringify()));

			if (!jmessage["error"].IsNull())
				return null;

			return new HitboxCommBreak
			{
				Count = jmessage["params"]["count"].ToObject<int>(),
				Delay = jmessage["params"]["delay"].ToString(),
				Timestamp = DateTime.FromFileTime(jmessage["params"]["timestamp"].ToObject<long>())
			};
		}

		/// <summary>Get last commecrial break for specified channel</summary>
		/// <returns>Returns null if channel never run ads</returns>
		public async Task<HitboxLastCommBreak> GetLastCommercialBreak(string channel)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.CommercialBreak + channel));

			if (jmessage["seconds_ago"].IsNull())
				return null;

			return new HitboxLastCommBreak
			{
				Count	   = jmessage["ad_count"].ToObject<int>(),
				SecondsAgo = jmessage["seconds_ago"].ToObject<int>(),
				Timeout	   = jmessage["timeout"].ToObject<int>()
			};
		}

		// MAYBE: Statistics
		// TODO: Editors

		// MAYBE: Toggle panels

		/// <summary>Get list of user profile panels</summary>
		/// <returns>Null if user have no panels or the panels are made through old editor</returns>
		public async Task<IList<HitboxProfilePanel>> GetProfilePanels(string channel)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.ProfilePanels + channel));

			if (!jmessage["profile"].HasValues)
				return null;

			IList<HitboxProfilePanel> panels = new List<HitboxProfilePanel>();

			foreach (JToken jpanel in jmessage["profile"]["panels"])
			{
				panels.Add(new HitboxProfilePanel
				{
					ID = jpanel["id"].ToObject<int>(),
					Headline  = jpanel["headline"].ToString(),
					Content   = jpanel["content"].ToString(),
					ImageLink = jpanel["link"].ToString(),
					ImageUrl  = jpanel["image"].ToString()
				});
			}

			return panels;
		}

		// MAYBE: Update panels

		/// <summary>Send message to twitter</summary>
		/// <returns>Returns true on success, false on error</returns>
		public async Task<bool> TwitterPost(string message)
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (message == null)
				throw new ArgumentNullException("message");

			JObject jmessage = JObject.Parse(await Web.POST(HitboxEndpoint.TwitterPost + "?authToken=" + authOrAccessToken + "&user_name=" + User.Username, new JsonObject
			{
				{ "user_name", JsonValue.CreateStringValue(User.Username) },
				{ "authToken", JsonValue.CreateStringValue(authOrAccessToken) },
				{ "message",   JsonValue.CreateStringValue(message) }
			}.Stringify()));

			if (!jmessage["error"].IsNull())
				return false;

			return true;
		}

		/// <summary>Send message to facebook</summary>
		/// <returns>Returns true on success, false on error</returns>
		public async Task<bool> FacebookPost(string message)
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (message == null)
				throw new ArgumentNullException("message");

			JObject jmessage = JObject.Parse(await Web.POST(HitboxEndpoint.FacebookPost + "?authToken=" + authOrAccessToken + "&user_name=" + User.Username, new JsonObject
			{
				{ "user_name", JsonValue.CreateStringValue(User.Username) },
				{ "authToken", JsonValue.CreateStringValue(authOrAccessToken) },
				{ "message",   JsonValue.CreateStringValue(message) }
			}.Stringify()));

			if (!jmessage["error"].IsNull())
				return false;

			return true;
		}

		// TODO: Hosters

		/// <summary>Get livestream for specified channel</summary>
		/// <returns>Return null if no livestream was found</returns>
		public async Task<HitboxLivestream> GetLivestream(string channel, bool useToken = false)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			if (useToken && !isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			string response = string.Empty;

			try
			{
				response = await Web.GET(HitboxEndpoint.Livestream + channel + (useToken ? "?authToken=" + authOrAccessToken : ""));
			}
			catch(WebException e)
			{
				var httpResponse = e.Response as HttpWebResponse;

				if (httpResponse.StatusCode != HttpStatusCode.NotFound)
					throw;
			}

			JObject jmessage = JObject.Parse(response);

			if (!jmessage["error"].IsNull())
				return null;

			JToken jlive = jmessage["livestream"][0];

			IList<HitboxMediaProfile> profiles = null;

			if (!jlive["media_profiles"].IsNull())
			{
				JArray jprofiles = JArray.Parse(Regex.Unescape(jlive["media_profiles"].ToString()));
				  
				profiles = new List<HitboxMediaProfile>(jprofiles.Count);

				foreach (JToken jprofile in jprofiles)
				{
					profiles.Add(jprofile.ToObject<HitboxMediaProfile>());
				}
			}

			return new HitboxLivestream
			{
				ID = jlive["media_id"].ToObject<int>(),
				Title     = jlive["media_status"].ToString(),
				Viewers   = jlive["media_views"].ToObject<int>(),
				MediaFile = jlive["media_file"].ToString(),
				Profiles  = profiles,
				IsLive    = jlive["media_is_live"].ToValue<bool>(),
				IsChatEnabled = jlive["media_chat_enabled"].ToValue<bool>(),
				Countries     = jlive["media_countries"].HasValues ? jlive["media_countries"].ToObject<IList<string>>() : null,
				Views         = jlive["media_views"].ToObject<int>(),
				ViewsDaily    = jlive["media_views_daily"].ToObject<int>(),
				ViewsWeekly   = jlive["media_views_weekly"].ToObject<int>(),
				ViewsMonthly  = jlive["media_views_monthly"].ToObject<int>(),
				Game = new HitboxGame
				{
					ID = jlive["category_id"].ToValue<int>(),
					Name     = jlive["category_name"].ToString(),
					Viewers  = jlive["category_viewers"].ToValue<int>(),
					Channels = jlive["category_channels"].ToValue<int>(),
					LogoUrl  = jlive["category_logo_large"].ToString(),
					SeoKey   = jlive["category_seo_key"].ToString()
				},
				ThumbnailUrl = jlive["media_thumbnail"].ToString(),
				Channel = new HitboxChannel
				{
					Recordings = jlive["channel"]["recordings"].ToObject<int>(),
					Videos     = jlive["channel"]["videos"].ToObject<int>(),
					Teams      = jlive["channel"]["teams"].ToObject<int>(),
					User = new HitboxUser
					{
						ID = jlive["channel"]["user_id"].ToObject<int>(),
						Username  = jlive["channel"]["user_name"].ToString(),
						Followers = jlive["channel"]["followers"].ToObject<int>(),
						MediaID   = jlive["channel"]["user_media_id"].ToObject<int>(),
						IsLive    = jlive["channel"]["media_is_live"].ToValue<bool>(),
						AvatarUrlSmall = jlive["channel"]["user_logo_small"].ToString(),
						AvatarUrlLarge = jlive["channel"]["user_logo"].ToString(),
						CoverUrl  = jlive["channel"]["user_cover"].ToString(),
						LiveSince = DateTime.Parse(jlive["channel"]["media_live_since"].ToString()),
						Twitter   = jlive["channel"]["twitter_account"].ToString(),
						client    = this
					}
				}
			};
		}

		/// <summary>Get list of livestream for specified game</summary>
		/// <param name="gameID">0 = all games</param>
		public async Task<IList<HitboxLivestream>> GetLivestreams(int gameID = 0, int start = 0, int limit = 10, bool liveonly = true, bool useToken = false)
		{
			if (useToken && !isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			Stream response = await Web.Streams.GET(HitboxEndpoint.Livestreams + "?liveonly=" + (liveonly ? "true" : "false") + "&game=" + gameID + "&start=" + start + "&limit=" + limit);

			using (StreamReader streamReader = new StreamReader(response))
			{
				using (JsonReader jsonReader = new JsonTextReader(streamReader))
				{
					JObject jmessage = JObject.Load(jsonReader);

					IList<HitboxLivestream> livestreams = new List<HitboxLivestream>(limit);

					if (!jmessage["error"].IsNull())
						return livestreams;

					IList<HitboxMediaProfile> profiles = null;

					foreach (JObject jlive in jmessage["livestream"])
					{
						if (jlive["media_profiles"].HasValues)
						{
							JArray jprofiles = JArray.Parse(Regex.Unescape(jlive["media_profiles"].ToString()));

							profiles = new List<HitboxMediaProfile>(jprofiles.Count);

							foreach (JToken jprofile in jprofiles)
							{
								profiles.Add(jprofile.ToObject<HitboxMediaProfile>());
							}
						}

						livestreams.Add(new HitboxLivestream
						{
							ID = jlive["media_id"].ToObject<int>(),
							Title     = jlive["media_status"].ToString(),
							Viewers   = jlive["media_views"].ToObject<int>(),
							MediaFile = jlive["media_file"].ToString(),
							Profiles  = profiles,
							IsLive    = jlive["media_is_live"].ToValue<bool>(),
							IsChatEnabled = jlive["media_chat_enabled"].ToValue<bool>(),
							Countries = jlive["media_countries"].HasValues ? jlive["media_countries"].ToObject<List<string>>() : null,
							Game = new HitboxGame
							{
								ID = jlive["category_id"].ToValue<int>(),
								Name     = jlive["category_name"].ToString(),
								Viewers  = jlive["category_viewers"].ToValue<int>(),
								Channels = jlive["category_channels"].ToValue<int>(),
								LogoUrl  = jlive["category_logo_large"].ToString(),
								SeoKey   = jlive["category_seo_key"].ToString()
							},
							ThumbnailUrl = jlive["media_thumbnail"].ToString(),
							Channel = new HitboxChannel
							{
								Recordings = jlive["channel"]["recordings"].ToObject<int>(),
								Videos     = jlive["channel"]["videos"].ToObject<int>(),
								Teams      = jlive["channel"]["teams"].ToObject<int>(),
								User = new HitboxUser
								{
									ID = jlive["channel"]["user_id"].ToObject<int>(),
									Username  = jlive["channel"]["user_name"].ToString(),
									Followers = jlive["channel"]["followers"].ToObject<int>(),
									MediaID   = jlive["channel"]["user_media_id"].ToObject<int>(),
									IsLive    = jlive["channel"]["media_is_live"].ToValue<bool>(),
									AvatarUrlSmall = jlive["channel"]["user_logo_small"].ToString(),
									AvatarUrlLarge = jlive["channel"]["user_logo"].ToString(),
									CoverUrl  = jlive["channel"]["user_cover"].ToString(),
									LiveSince = DateTime.Parse(jlive["channel"]["media_live_since"].ToString()),
									Twitter   = jlive["channel"]["twitter_account"].ToString(),
									client    = this
								}
							}
						});
					}

					return livestreams;
				}
			}
		}

		// TODO: Update Live Media
		// MAYBE: Featured media

		/// <summary>Get media status and viewer count for a channel</summary>
		/// <returns>Null if channel is not live</returns>
		public async Task<HitboxMediaStatus> MediaStatus(string channel)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.MediaStatus + channel));

			if (jmessage["media_is_live"].IsNull())
				return null;

			return new HitboxMediaStatus
			{
				IsLive = jmessage["media_is_live"].ToValue<bool>(),
				Viewers = jmessage["media_views"].ToObject<int>()
			};
		}

		/// <summary>Get total views for channel</summary>
		public async Task<int> GetTotalViews(string channel)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.TotalViews + channel));

			if (jmessage["total_live_views"].ToString() == "false")
				return 0;

			return jmessage["total_live_views"].ToObject<int>();
		}

		// TODO: Stream Details
		// MAYBE: Get game accounts
		// MAYBE: Update game accounts

		/// <summary>Get video with specified id</summary>
		/// <returns>Returns null if no video was found</returns>
		public async Task<HitboxVideo> GetVideo(int id, bool useToken = false)
		{
			if (useToken && !isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.Video + id + (useToken ? "?authToken=" + authOrAccessToken : "")));

			if (!jmessage["error"].IsNull())
				return null;

			JToken jvideo = jmessage["video"][0];

			IList<HitboxMediaProfile> profiles = null;

			if (!jvideo["media_profiles"].IsNull())
			{
				JArray jprofiles = JArray.Parse(Regex.Unescape(jvideo["media_profiles"].ToString()));

				profiles = new List<HitboxMediaProfile>(jprofiles.Count);

				foreach (JToken jprofile in jprofiles)
				{
					profiles.Add(jprofile.ToObject<HitboxMediaProfile>());
				}
			}

			return new HitboxVideo
			{
				ID = jvideo["media_id"].ToObject<int>(),
				Title        = jvideo["media_title"].ToString(),
				Description  = jvideo["media_description"].ToString(),
				DateAdded    = DateTime.Parse(jvideo["media_date_added"].ToString()),
				Duration     = jvideo["media_duration_format"].ToString(),
				Views        = jvideo["media_views"].ToObject<int>(),
				ThumbnailUrl = jvideo["media_thumbnail"].ToString(),
				MediaFile    = jvideo["media_file"].ToString(),
				Profiles     = profiles,
				Game = new HitboxGame
				{
					ID = jvideo["category_id"].ToValue<int>(),
					Name     = jvideo["category_name"].ToString(),
					Viewers  = jvideo["category_viewers"].ToValue<int>(),
					Channels = jvideo["category_channels"].ToValue<int>(),
					LogoUrl  = jvideo["category_logo_large"].ToString(),
					SeoKey   = jvideo["category_seo_key"].ToString()
				},
				Channel = new HitboxChannel
				{
					Recordings = jvideo["channel"]["recordings"].ToObject<int>(),
					Videos     = jvideo["channel"]["videos"].ToObject<int>(),
					Teams      = jvideo["channel"]["teams"].ToObject<int>(),
					User = new HitboxUser
					{
						ID = jvideo["channel"]["user_id"].ToObject<int>(),
						Username  = jvideo["channel"]["user_name"].ToString(),
						IsLive    = jvideo["channel"]["media_is_live"].ToValue<bool>(),
						LiveSince = DateTime.Parse(jvideo["channel"]["media_live_since"].ToString()),
						MediaID   = jvideo["channel"]["user_media_id"].ToObject<int>(),
						AvatarUrlSmall = jvideo["channel"]["user_logo_small"].ToString(),
						AvatarUrlLarge = jvideo["channel"]["user_logo"].ToString(),
						CoverUrl  = jvideo["channel"]["user_cover"].ToString(),
						Followers = jvideo["channel"]["followers"].ToObject<int>(),
						Twitter   = jvideo["channel"]["twitter_account"].ToString(),
						client    = this
					}
				}
			};
		}

		/// <summary>Get list of videos for specified game</summary>
		/// <param name="gameID">0 = all games</param>
		public async Task<IList<HitboxVideo>> GetVideos(int gameID = 0, int start = 0, int limit = 10, bool weekly = true)
		{
			Stream response = await Web.Streams.GET(HitboxEndpoint.Videos + "?filter=" + (weekly ? "weekly" : "popular") + "&game=" + gameID + "&start=" + start + "&limit=" + limit);

			using (StreamReader streamReader = new StreamReader(response))
			{
				using (JsonReader jsonReader = new JsonTextReader(streamReader))
				{
					JObject jmessage = JObject.Load(jsonReader);

					List<HitboxVideo> videos = new List<HitboxVideo>(limit);

					if (!jmessage["error"].IsNull())
						return videos;

					foreach (JObject jvideo in jmessage["video"])
					{
						IList<HitboxMediaProfile> profiles = null;

						if (!jvideo["media_profiles"].IsNull())
						{
							JArray jprofiles = JArray.Parse(Regex.Unescape(jvideo["media_profiles"].ToString()));

							profiles = new List<HitboxMediaProfile>(jprofiles.Count);

							foreach (JToken jprofile in jprofiles)
							{
								profiles.Add(jprofile.ToObject<HitboxMediaProfile>());
							}
						}

						videos.Add(new HitboxVideo
						{
							ID = jvideo["media_id"].ToObject<int>(),
							Title = jvideo["media_title"].ToString(),
							Views = jvideo["media_views"].ToObject<int>(),
							Description  = jvideo["media_description"].ToString(),
							DateAdded    = DateTime.Parse(jvideo["media_date_added"].ToString()),
							Duration     = jvideo["media_duration_format"].ToString(),
							ThumbnailUrl = jvideo["media_thumbnail"].ToString(),
							MediaFile    = jvideo["media_file"].ToString(),
							Profiles     = profiles,
							Game = new HitboxGame
							{
								ID       = jvideo["category_id"].ToValue<int>(),
								Name     = jvideo["category_name"].ToString(),
								Viewers  = jvideo["category_viewers"].ToValue<int>(),
								Channels = jvideo["category_channels"].ToValue<int>(),
								LogoUrl  = jvideo["category_logo_large"].ToString(),
								SeoKey   = jvideo["category_seo_key"].ToString()
							},
							Channel = new HitboxChannel
							{
								Recordings = jvideo["channel"]["recordings"].ToObject<int>(),
								Videos     = jvideo["channel"]["videos"].ToObject<int>(),
								Teams      = jvideo["channel"]["teams"].ToObject<int>(),
								User = new HitboxUser
								{
									ID = jvideo["channel"]["user_id"].ToObject<int>(),
									Username  = jvideo["channel"]["user_name"].ToString(),
									Followers = jvideo["channel"]["followers"].ToObject<int>(),
									MediaID   = jvideo["channel"]["user_media_id"].ToObject<int>(),
									IsLive    = jvideo["channel"]["media_is_live"].ToValue<bool>(),
									LiveSince = DateTime.Parse(jvideo["channel"]["media_live_since"].ToString()),
									AvatarUrlSmall = jvideo["channel"]["user_logo_small"].ToString(),
									AvatarUrlLarge = jvideo["channel"]["user_logo"].ToString(),
									CoverUrl  = jvideo["channel"]["user_cover"].ToString(),
									Twitter   = jvideo["channel"]["twitter_account"].ToString(),
									client    = this
								}
							}
						});
					}

					return videos;
				}
			}
		}

		/// <summary>Create a video from a recording</summary>
		/// <returns></returns>
		private async Task<bool> CreateVideo()
		{
			// TODO: CreateVideo method
			return false;
		}

		private void UpdateVideo()
		{
			// TODO: UpdateVideo method
		}

		// TODO: Recordings

		/// <summary>Get list of followers for specified channel</summary>
		public async Task<IList<HitboxFollower>> GetFollowers(string channel, int offset = 0, int limit = 10)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			Stream response = await Web.Streams.GET(HitboxEndpoint.Followers + channel + "?offset=" + offset + "&limit=" + limit, true);

			using (StreamReader streamReader = new StreamReader(response))
			{
				using (JsonReader jsonReader = new JsonTextReader(streamReader))
				{
					JObject jmessage = JObject.Load(jsonReader);

					List<HitboxFollower> followers = new List<HitboxFollower>(limit);

					if (!jmessage["error"].IsNull())
						return followers;

					foreach (JObject jfollower in jmessage["followers"])
					{
						followers.Add(new HitboxFollower
						{
							UserID    = jfollower["user_id"].ToObject<int>(),
							Username  = jfollower["user_name"].ToString(),
							Followers = jfollower["followers"].ToObject<int>(),
							DateAdded = DateTime.Parse(jfollower["date_added"].ToString()),
							AvatarUrl = jfollower["user_logo_small"].ToString()
						});
					}

					return followers;
				}
			}
		}

		/// <summary>Get list of channels that specified user follows</summary>
		public async Task<IList<HitboxFollower>> GetFollowing(string user, int offset = 0, int limit = 10)
		{
			if (user == null)
				throw new ArgumentNullException("user");

			Stream response = await Web.Streams.GET(HitboxEndpoint.Following + "?user_name=" + user + "&offset=" + offset + "&limit=" + limit, true);

			using (StreamReader streamReader = new StreamReader(response))
			{
				using (JsonReader jsonReader = new JsonTextReader(streamReader))
				{
					JObject jmessage = JObject.Load(jsonReader);

					List<HitboxFollower> following = new List<HitboxFollower>(limit);

					if (!jmessage["error"].IsNull())
						return following;

					foreach (JObject jfollower in jmessage["following"])
					{
						following.Add(new HitboxFollower
						{
							UserID    = jfollower["user_id"].ToObject<int>(),
							Username  = jfollower["user_name"].ToString(),
							Followers = jfollower["followers"].ToObject<int>(),
							DateAdded = DateTime.Parse(jfollower["date_added"].ToString()),
							AvatarUrl = jfollower["user_logo_small"].ToString()
						});
					}

					return following;
				}
			}
		}

		/// <summary>Check if user is following given channel</summary>
		/// <returns>Null if not following</returns>
		public async Task<HitboxFollowingStatus> CheckFollowingStatus(string channel, string user)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			if (user == null)
				throw new ArgumentNullException("user");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.Following + channel + "?user_name=" + user, true));

			if (!jmessage["error"].IsNull())
				return null;

			return new HitboxFollowingStatus
			{
				UserId = jmessage["following"]["follow_id"].ToObject<int>(),
				Notify = jmessage["following"]["follower_notify"].ToValue<bool>(),
				FollowerId = jmessage["following"]["follower_user_id"].ToObject<int>()
			};
		}

		// MAYBE: Follower statistics

		/// <summary>Follow a user</summary>
		/// <returns>Returns false if user is already following channel</returns>
		public async Task<bool> Follow(string usernameOrUserID)
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (usernameOrUserID == null)
				throw new ArgumentNullException("usernameOrUserID");

			JObject jmessage = JObject.Parse(await Web.POST(HitboxEndpoint.Follow + "?authToken=" + authOrAccessToken, new JsonObject
			{
				{ "type", JsonValue.CreateStringValue("user") },
				{ "follow_id", JsonValue.CreateStringValue(usernameOrUserID) }
			}.Stringify()));

			return jmessage["success"].ToObject<bool>();
		}

		/// <summary>Unfollow a user</summary>
		/// <returns>Returns false on error</returns>
		public async Task<bool> Unfollow(int userID)
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			string url = HitboxEndpoint.Follow + "?type=user&authToken=" + authOrAccessToken + "&follow_id=" + userID;

			JObject jmessage = JObject.Parse(await Web.DELETE(url));

			return jmessage["success"].ToObject<bool>();
		}

		// TODO: Subscriptions
		// TODO: Subscribers
		// TODO: Subscriber badge

		/// <summary>Check if the user has subscription to specified channel</summary>
		public async Task<bool> CheckSubscriptionStatus(string channel)
		{
			if (!isLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (channel == null)
				throw new ArgumentNullException("channel");

			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.Subscription + channel + "/" + authOrAccessToken));

			return jmessage["isSubscriber"].ToObject<bool>();
		}

		// TODO: Check subscription info

		// MAYBE: Teams

		/// <summary>Get list of games</summary>
		/// <param name="query">Search query</param>
		/// <param name="liveOnly">Only games that has live broadcasts</param>
		/// <param name="limit">Max = 100</param>
		public static async Task<IList<HitboxGame>> GetGames(string query = null, bool liveOnly = true, int limit = 100)
		{
			List<HitboxGame> games = new List<HitboxGame>();

			Stream stream = await Web.Streams.GET(HitboxEndpoint.Games + "?limit=" + limit + (query == null ? "" : "&q=" + query) + (liveOnly ? "&liveonly=true" : ""));

			using (StreamReader streamReader = new StreamReader(stream))
			{
				using (JsonReader jsonReader = new JsonTextReader(streamReader))
				{
					JObject jmessage = JObject.Load(jsonReader);

					foreach (JObject jgame in jmessage["categories"])
					{
						games.Add(new HitboxGame
						{
							ID = jgame["category_id"].ToObject<int>(),
							Name = jgame["category_name"].ToString(),
							Viewers  = jgame["category_viewers"].ToValue<int>(true),
							LogoUrl  = jgame["category_logo_large"].ToString(),
							SeoKey   = jgame["category_seo_key"].ToString(),
							Channels = jgame["category_media_count"].ToValue<int>(true),
						});
					}

					return games;
				}
			}
		}

		/// <summary>Get game with specified id</summary>
		/// <returns>Returns null if game was not found</returns>
		public static async Task<HitboxGame> GetGame(int id)
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.Game + id));

			if (jmessage["category"] == null)
				return null;

			return new HitboxGame
			{
				ID		 = jmessage["category"]["category_id"].ToObject<int>(),
				Name	 = jmessage["category"]["category_name"].ToString(),
				Viewers	 = jmessage["category"]["category_viewers"].ToValue<int>(true),
				Channels = jmessage["category"]["category_media_count"].ToValue<int>(true),
				LogoUrl	 = jmessage["category"]["category_logo_large"].ToString(),
				SeoKey   = jmessage["category"]["category_seo_key"].ToString()
			};
		}

		// MAYBE: Ingests

		/// <summary>Get list of chat servers</summary>
		public static async Task<IList<string>> GetChatServers()
		{
			JArray jmessage = JArray.Parse(await Web.GET(HitboxEndpoint.ChatServers));

			IList<string> servers = new List<string>();

			foreach(JToken jserver in jmessage)
				servers.Add(jserver["server_ip"].ToString());

			return servers;
		}

		/// <summary>Get list of servers for "viewer" (player servers)</summary>
		public static async Task<IList<string>> GetViewerServers()
		{
			JArray jmessage = JArray.Parse(await Web.GET(HitboxEndpoint.ViewerServers));

			IList<string> servers = new List<string>();

			foreach (JToken jserver in jmessage)
				servers.Add(jserver["server_ip"].ToString());

			return servers;
		}

		/// <summary>Get chat server id</summary>
		public static async Task<string> GetChatServerSocketID(string serverIP)
		{
			if (serverIP == null)
				throw new ArgumentNullException("serverIP");

			return (await Web.GET("http://" + serverIP + "/socket.io/1/")).Split(':')[0];
		}

		/// <summary>Get all possible chat colors you can use</summary>
		public static async Task<IEnumerable<string>> GetChatColors()
		{
			JObject jmessage = JObject.Parse(await Web.GET(HitboxEndpoint.ChatColors));

			LinkedList<string> colors = new LinkedList<string>();

			foreach (JToken jcolor in jmessage["colors"])
				colors.AddLast(jcolor.ToString());

			return colors;
		}

		/*MAYBE: Upload
					Update User Avatar
					Update Channel Banner
					Get Description Images
					Upload Description Image
					Removes Description Image
					Uploads Team Logo or Cover
					Upload Emoji Image*/

		#region Handlers

		protected internal virtual void OnLoggedIn(HitboxLoginEventArgs e)
		{
			LoggedIn?.Invoke(this, e);
		}

		#endregion

		public void Dispose()
		{
			User = null;
			authOrAccessToken = null;
		}
	}
}
