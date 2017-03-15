using System;

namespace HitboxUWP
{
	/// <summary>Main class that you should use</summary>
	public class HitboxClient : HitboxClientBase
	{
		public HitboxClient() : base() { }
		public HitboxClient(string key, string secret) : base(key, secret) { }

		// TODO: methods with HitBoxObjects

		/// <summary>Create a new LivestreamViewer</summary>
		/// <param name="authenticate">If not true, then you are viewing a livestream as guest/anonymous</param>
		public HitboxLivestreamViewer CreateLivestreamViewer(string channel, bool authenticate = true)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			if (!isLoggedIn && authenticate)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (authenticate)
				return new HitboxLivestreamViewer(channel, User.Username, authOrAccessToken);

			return new HitboxLivestreamViewer(channel);
		}

		public HitboxChat CreateChat(bool authenticate = true)
		{
			if (!isLoggedIn && authenticate)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (authenticate)
				return new HitboxChat(authOrAccessToken, User.Username);

			return new HitboxChat();
		}
	}
}
