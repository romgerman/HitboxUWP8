using System;

namespace HitboxUWP8
{
	public class LoginEventArgs : EventArgs
	{
		public enum States  { OK, Error, InvalidToken, Cancelled }
		public enum Methods { FirstLogin, Another }

		public States State   { get; set; }
		public Methods Method { get; set; }
		public string Error   { get; set; }
		public string Token	  { get; set; }
	}

	public class ViewerStatusChangedArgs : EventArgs
	{
		public HitBoxMediaStatus Status { get; set; }
		public int Followers   { get; set; }
		public int Subscribers { get; set; }
	}

	public class ChatLoggedInEventArgs : EventArgs
	{
		public HitBoxRole Role { get; set; }

		public ChatLoggedInEventArgs(HitBoxRole role)
		{
			Role = role;
		}
	}

	// Exceptions

	internal static class ExceptionList
	{
		public const string NotLoggedIn = "You must be logged in to use this";
		public const string AuthFailed = "authentication_failed";
	}

	public class HitBoxException : Exception
	{
		public HitBoxException() { }
		public HitBoxException(string message) : base(message) { }
	}
}
