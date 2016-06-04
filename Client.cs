using System;

namespace HitboxUWP8
{
	/// <summary>Main class that you should use</summary>
	public class HitboxClient : HitboxClientBase
	{
		public HitboxClient(string key, string secret) : base(key, secret) { }

		// TODO: methods with HitBoxObjects

		/// <summary>Create a new LivestreamViewer</summary>
		/// <param name="auth">If not true, then you are viewing a livestream as guest/anonymous</param>
		public HitboxLivestreamViewer CreateLivestreamViewer(string channel, bool auth = false)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");

			if (!isLoggedIn && auth)
				throw new HitboxException(ExceptionList.NotLoggedIn);

			if (auth)
				return new HitboxLivestreamViewer(new HitboxLivestreamViewer.Parameters
				{
					Channel = channel,
					Username = User.Username,
					Token = authOrAccessToken
				});

			return new HitboxLivestreamViewer(new HitboxLivestreamViewer.Parameters
			{
				Channel = channel
			});
		}
	}
}
