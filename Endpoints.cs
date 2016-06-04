namespace HitboxUWP8
{
	public static class HitboxEndpoint
	{
		public const string ImageStorage	= "http://edge.sf.hitbox.tv/";

		public const string Api				= "https://api.hitbox.tv/";

		public const string Login			= Api + "oauth/login";
		public const string TokenValidation = Api + "auth/valid/";
		public const string UserFromToken	= Api + "userfromtoken/";
		public const string ExchangeRequest = Api + "oauth/exchange";
		public const string StreamKey		= Api + "mediakey/";
		public const string CommercialBreak = Api + "ws/combreak/";
		public const string TwitterPost		= Api + "twitter/post";
		public const string FacebookPost	= Api + "facebook/post";
		public const string EmailVerified   = Api + User + "checkVerifiedEmail/";

		public const string AccessLevels	= User + "access/";

		public const string Games			= Api + "games";
		public const string Game			= Api + "game/";
		public const string User			= Api + "user/";
		public const string ProfilePanels	= Api + "profile/";
		public const string Media			= Api + "media/";
		public const string Livestream		= Media + "live/";
		public const string Livestreams		= Livestream + "list";
		public const string Video			= Media + "video/";
		public const string Videos			= Video + "list";
		public const string Subscription	= User + "subscription/";
		public const string MediaStatus		= Media + "status/";
		public const string TotalViews		= Media + "views/";
		public const string Followers		= Api + "followers/user/";
		public const string Following		= Api + "following/user/";
		public const string Follow			= Api + "follow";

		public const string ChatColors		= Api + "chat/colors";
		public const string ChatServers		= Api + "chat/servers";
		public const string ViewerServers	= Api + "player/server";

		/// <summary>Replace {url} with server url</summary>
		internal const string LivestreamViewer = "ws://{url}/viewer";
	}

	public static class HitboxUrl
	{
		public const string Home   = "http://www.hitbox.tv/";
		public const string Browse = Home + "browse/";
	}
}
