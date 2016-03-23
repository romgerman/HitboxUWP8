using System;
using System.Collections.Generic;

namespace HitboxUWP8
{
	public class HitBoxUser
	{
		public int	  ID		  { get; set; }
		public int	  MediaID	  { get; set; }
		public string Username	  { get; set; }
		public int	Followers	  { get; set; }
		public bool IsLive		  { get; set; }
		public bool IsBroadcaster { get; set; }
		public DateTime LiveSince { get; set; }
		public string   Twitter	  { get; set; }
		public string   Email	  { get; set; }

		public string AvatarUrl	{ get; set; }
		public string CoverUrl	{ get; set; }
	}

	public class HitBoxFollower
	{
		public int		UserID	  { get; set; }
		public string	Username  { get; set; }
		public int		Followers { get; set; }
		public string	AvatarUrl { get; set; }
		public DateTime DateAdded { get; set; }
	}

	public class HitBoxGame
	{
		public int	  ID		{ get; set; }
		public string Name		{ get; set; }
		public string SeoKey	{ get; set; }
		public int	  Viewers	{ get; set; }
		public int ChannelCount { get; set; }
		public string LogoUrl	{ get; set; }
	}

	public class HitBoxLivestream
	{
		public int	  ID			   { get; set; }
		public string Title			   { get; set; }
		public string Username		   { get; set; }
		public string UsernameDisplay  { get; set; }
		public string Game			   { get; set; }
		public int	  Viewers		   { get; set; }
		public List<string> Countries  { get; set; }
		public DateTime     LiveSince  { get; set; }
		public string		AvatarUrl  { get; set; }
		public string		CoverUrl   { get; set; }
	}

	public class HitBoxVideo
	{
		public int		ID			{ get; set; }
		public string	Title		{ get; set; }
		public string	Description	{ get; set; }
		public DateTime DateAdded	{ get; set; }
		public string   Duration	{ get; set; }
		public int		Views		{ get; set; }
		public string	CoverUrl	{ get; set; }
		public HitBoxUser	User		{ get; set; }
		public HitBoxGame    Game		{ get; set; }
	}

	public class HitBoxAccessLevels
	{
		public enum AccessLevel { Anon, Admin }

		public int UserID				 { get; set; }
		public int AccessUserID			 { get; set; }
		public AccessLevel Settings		 { get; set; }
		public AccessLevel Account		 { get; set; }
		public AccessLevel Livestreams	 { get; set; }
		public AccessLevel Broadcast	 { get; set; }
		public AccessLevel Videos		 { get; set; }
		public AccessLevel Recordings	 { get; set; }
		public AccessLevel Statistics	 { get; set; }
		public AccessLevel Inbox		 { get; set; }
		public AccessLevel Revenues		 { get; set; }
		public AccessLevel Chat			 { get; set; }
		public AccessLevel Following	 { get; set; }
		public AccessLevel Teams		 { get; set; }
		public AccessLevel Subscriptions { get; set; }
		public AccessLevel Payments		 { get; set; }
		public bool IsSubscriber		 { get; set; }
		public bool IsFollower			 { get; set; }

		//public void Admin { get; set; }
		//public void Superadmin { get; set; }
		//public void Partner { get; set; }
	}

	public class HitBoxLastCommBreak
	{
		public int Count	  { get; set; }
		public int SecondsAgo { get; set; }
		public int Timeout	  { get; set; }
	}

	public class HitBoxMediaStatus
	{
		public bool IsLive { get; set; }
		public int Viewers { get; set; }
	}
}
