using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HitboxUWP8
{
	public static class Extensions
	{
		public static bool IsNull(this JToken token)
		{
			return (token == null);
		}
	}

	public class JsonHelper
    {
		#region Data structs
		public struct JsonChatServer
		{
			[JsonProperty("server_ip")]
			public string IP { get; set; }
		}

		public class ErrorObject
		{
			public string error_msg;
		}

        public struct VideoChannel
        {
            public string followers;
            public string user_id;
            public string user_name;
            public string user_status;
            public string user_logo;
            public string user_cover;
            public string user_logo_small;
            public string user_partner;
            public string user_beta_profile;
            public string media_is_live;
            public string media_live_since;
            public string user_media_id;
            public string twitter_account;
            public string twitter_enabled;
            public string livestream_count;
        }

        public struct Video
        {
            public string media_user_name;
            public string media_id;
            public string media_file;
            public string media_user_id;
            public string media_profiles;
            public string media_type_id;
            public string media_is_live;
            public string media_live_delay;
            public string media_date_added;
            public object media_live_since;
            public object media_transcoding;
            public string media_chat_enabled;
            public object media_countries;
            public object media_hosted_id;
            public object media_mature;
            public object media_hidden;
            public object media_offline_id;
            public object user_banned;
            public string media_name;
            public string media_display_name;
            public string media_status;
            public string media_title;
            public string media_description;
            public string media_description_md;
            public string media_tags;
            public string media_duration;
            public object media_bg_image;
            public string media_views;
            public string media_views_daily;
            public string media_views_weekly;
            public string media_views_monthly;
            public string category_id;
            public string category_name;
            public object category_name_short;
            public string category_seo_key;
            public string category_viewers;
            public string category_media_count;
            public object category_channels;
            public object category_logo_small;
            public string category_logo_large;
            public string category_updated;
            public string team_name;
            public string media_start_in_sec;
            public string media_duration_format;
            public string media_thumbnail;
            public string media_thumbnail_large;
            public VideoChannel channel;
        }

        public struct VideoRootObject
        {
            public List<Video> video;
        }

        public struct Bitrate
        {
            public string url;
            public int bitrate;
            public string label;
            public string provider;
            public bool isDefault;
        }

        public class Clip
        {
            public List<Bitrate> bitrates;
        }

        public struct LivestreamMediaObject
        {
            public Clip clip;
        }

        public struct ChatServer
        {
            public string server_ip;
        }

        public struct ChatResponseObject
        {
            public string name;
            public List<string> args;
        }

        #endregion

        public static VideoRootObject GetVideos(string json)
        {
            return JsonConvert.DeserializeObject<VideoRootObject>(json);
        }

        public static LivestreamMediaObject GetLivestreamMediaObject(string json)
        {
            return JsonConvert.DeserializeObject<LivestreamMediaObject>(json);
        }

        public static List<ChatServer> GetChatServer(string json)
        {
            return JsonConvert.DeserializeObject<List<ChatServer>>(json);
        }

        public static ChatResponseObject LoginToChatResponse(string json)
        {
            return JsonConvert.DeserializeObject<ChatResponseObject>(json);
        }
    }
}
