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

	public class HitboxException : Exception
	{
		public HitboxException() { }
		public HitboxException(string message) : base(message) { }
	}
}
