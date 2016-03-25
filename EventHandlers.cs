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

	public class ViewerCountChangedArgs : EventArgs
	{
		public int CurrentCount { get; set; }
	}

	// Exceptions

	internal static class HitBoxExceptionList
	{
		public const string NotLoggedIn = "not logged in";
		public const string AuthFailed = "authentication_failed";
	}

	public class HitBoxException : Exception
	{
		public HitBoxException() { }
		public HitBoxException(string message) : base(message) { }
	}
}
