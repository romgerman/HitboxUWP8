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

	public class HitBoxChannel
	{
		public int Videos		{ get; set; }
		public int Recordings	{ get; set; }
		public int Teams		{ get; set; }
		public HitBoxUser User	{ get; set; }
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

	public class HitBoxMedia
	{
		public int	  ID			 { get; set; }
		public string Title			 { get; set; }
		public string MediaFile		 { get; set; }
		public string ThumbnailUrl   { get; set; }
		public HitBoxChannel Channel { get; set; }
		public HitBoxGame	 Game	 { get; set; }
		public IList<HitBoxMediaProfile> Profiles { get; set; }
	}

	public class HitBoxLivestream : HitBoxMedia
	{
		public int	Viewers			  { get; set; }
		public bool IsLive			  { get; set; }
		public bool IsChatEnabled	  { get; set; }
		public List<string> Countries { get; set; }
	}

	public class HitBoxVideo : HitBoxMedia
	{
		public string	Description	{ get; set; }
		public DateTime DateAdded	{ get; set; }
		public string   Duration	{ get; set; }
		public int		Views		{ get; set; }
	}

	public class HitBoxMediaProfile
	{
		public string Url  { get; set; }
		public int Height  { get; set; }
		public int Bitrate { get; set; }
	}

	public class HitBoxAccessLevels
	{
		public enum Level { Anon, Admin }

		public int UserID			{ get; set; }
		public int AccessUserID		{ get; set; }
		public Level Settings		{ get; set; }
		public Level Account		{ get; set; }
		public Level Livestreams	{ get; set; }
		public Level Broadcast		{ get; set; }
		public Level Videos			{ get; set; }
		public Level Recordings		{ get; set; }
		public Level Statistics		{ get; set; }
		public Level Inbox			{ get; set; }
		public Level Revenues		{ get; set; }
		public Level Chat			{ get; set; }
		public Level Following		{ get; set; }
		public Level Teams			{ get; set; }
		public Level Subscriptions	{ get; set; }
		public Level Payments		{ get; set; }
		public bool IsSubscriber	{ get; set; }
		public bool IsFollower		{ get; set; }

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
