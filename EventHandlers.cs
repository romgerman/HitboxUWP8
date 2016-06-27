using System;

namespace HitboxUWP8
{
	public class HitboxLoginEventArgs : EventArgs
	{
		public enum States  { OK, Error, InvalidToken, Cancelled }
		public enum Methods { FirstLogin, Another }

		public States State   { get; set; }
		public Methods Method { get; set; }
		public string Error   { get; set; }
		public string Token	  { get; set; }
	}

	public class HitboxViewerStatusChangedArgs : EventArgs
	{
		public HitboxMediaStatus Status { get; set; }
		public int Followers   { get; set; }
		public int Subscribers { get; set; }
	}

	public class HitboxChatLoggedInEventArgs : EventArgs
	{
		public HitboxRole Role { get; set; }

		public HitboxChatLoggedInEventArgs(HitboxRole role)
		{
			Role = role;
		}
	}

	public class HitboxChatMessageReceivedEventArgs : EventArgs
	{
		public string Username { get; set; }
		public string Text { get; set; }
		public HitboxRole Role { get; set; }
		public bool IsFollower { get; set; }
		public bool IsSubscriber { get; set; }
		public bool IsOwner { get; set; }
		public bool IsStaff { get; set; }
		public bool IsCommunity { get; set; }
		public DateTime Time { get; set; }

		/*
		   "params":{
			   "channel":"CHANNEL",
			   "name":"USERNAME",
			   "nameColor":"HEXCODE",
			   "text":"Text of your message",
			   "time":1449986713,
			   "role":"role in chat",
			   "isFollower":true/false,
			   "isSubscriber":true/false,
			   "isOwner":true/false,
			   "isStaff":true/false,
			   "isCommunity":true/false,
			   "media":true/false,
			   "image":"Path to channel owner/subscriber image",
			   "buffer":true/false,
			   "buffersent":true/false
		   }
		*/
	}

	// Exceptions

	internal static class ExceptionList
	{
		public const string NotLoggedIn = "You must be logged in to use this";
		public const string AuthFailed  = "authentication_failed";
	}

	public class HitboxException : Exception
	{
		public HitboxException() { }
		public HitboxException(string message) : base(message) { }
	}
}
