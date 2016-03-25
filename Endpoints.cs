namespace HitboxUWP8
{
	public static class HitBoxEndpoint
	{
		public const string ImageStorage	= "http://edge.sf.hitbox.tv";
		
		public const string Login			= "https://api.hitbox.tv/oauth/login";
		public const string TokenValidation = "https://api.hitbox.tv/auth/valid/";
		public const string UserFromToken	= "https://api.hitbox.tv/userfromtoken/";
		public const string ExchangeRequest = "https://api.hitbox.tv/oauth/exchange";
		public const string StreamKey		= "https://api.hitbox.tv/mediakey/";
		public const string AccessLevels	= User + "access/";
		public const string CommercialBreak = "https://api.hitbox.tv/ws/combreak/";
		public const string TwitterPost		= "https://api.hitbox.tv/twitter/post";
		public const string FacebookPost	= "https://api.hitbox.tv/facebook/post";

		public const string Games			= "https://api.hitbox.tv/games";
		public const string Game			= "https://api.hitbox.tv/game/";
		public const string User			= "https://api.hitbox.tv/user/";
		public const string Livestreams		= "https://www.hitbox.tv/api/media/live/list";
		public const string Videos			= "https://www.hitbox.tv/api/media/video/list";
		public const string Followers		= "https://api.hitbox.tv/followers/user/";
		public const string Following		= "https://api.hitbox.tv/following/user/";
		public const string Follow			= "https://api.hitbox.tv/follow";
		public const string Subscription	= User + "subscription/";
		public const string MediaStatus		= "https://api.hitbox.tv/media/status/";
		public const string TotalViews		= "https://api.hitbox.tv/media/views/";

		public const string ChatServers		= "https://api.hitbox.tv/chat/servers";

		/// <summary>Replace {url} with server url</summary>
		internal const string LivestreamViewer = "ws://{url}/viewer";
	}

	public static class Url
	{
		public const string Home   = "http://www.hitbox.tv/";
		public const string Browse = Home + "browse/";
	}
}
