using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HitboxUWP8
{
	public class HitboxUser
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

		public string AvatarUrlSmall { get; set; }
		public string AvatarUrlLarge { get; set; }
		public string CoverUrl { get; set; }

		internal HitboxClientBase client;

		public async Task<bool> Follow()   => await client.Follow(Username);
		public async Task<bool> Unfollow() => await client.Unfollow(ID);

		public async Task<HitboxFollowingStatus> CheckFollowingStatus() => await client.CheckFollowingStatus(Username, client.User.Username);

		public async Task<int> GetTotalViews() => await client.GetTotalViews(Username);

		public async Task<IList<HitboxProfilePanel>> GetProfilePanels() => await client.GetProfilePanels(Username);
	}

	public class HitboxChannel
	{
		public int Videos		{ get; set; }
		public int Recordings	{ get; set; }
		public int Teams		{ get; set; }
		public HitboxUser User	{ get; set; }
	}

	public class HitboxFollower
	{
		public int		UserID	  { get; set; }
		public string	Username  { get; set; }
		public int		Followers { get; set; }
		public string	AvatarUrl { get; set; }
		public DateTime DateAdded { get; set; }
	}

	public class HitboxGame
	{
		public int	  ID		{ get; set; }
		public string Name		{ get; set; }
		public string SeoKey	{ get; set; }
		public int	  Viewers	{ get; set; }
		public int    Channels  { get; set; }
		public string LogoUrl	{ get; set; }
	}

	public class HitboxMedia
	{
		public int	  ID			 { get; set; }
		public string Title			 { get; set; }
		public string MediaFile		 { get; set; }
		public string ThumbnailUrl   { get; set; }
		public HitboxChannel Channel { get; set; }
		public HitboxGame	 Game	 { get; set; }
		public IList<HitboxMediaProfile> Profiles { get; set; }
	}

	public class HitboxLivestream : HitboxMedia
	{
		public int	Viewers			   { get; set; }
		public bool IsLive			   { get; set; }
		public bool IsChatEnabled	   { get; set; }
		public IList<string> Countries { get; set; }

		public int Views { get; set; }
		public int ViewsDaily { get; set; }
		public int ViewsWeekly { get; set; }
		public int ViewsMonthly { get; set; }
	}

	public class HitboxVideo : HitboxMedia
	{
		public string	Description	{ get; set; }
		public DateTime DateAdded	{ get; set; }
		public string   Duration	{ get; set; }
		public int		Views		{ get; set; }
	}

	public class HitboxMediaProfile
	{
		public string Url  { get; set; }
		public int Fps     { get; set; }
		public int Height  { get; set; }
		public int Bitrate { get; set; }
	}

	public enum HitboxRole
	{
		/// <summary>"You are a unregistered user. You cannot write in chat and any messages you send will be dropped. User list is also disallowed"</summary>
		Guest,
		/// <summary>"You are a normal viewer. You can write and see the user list. Strict messages/second limits apply. Duplicate messages are disallowed"</summary>
		Anon,
		/// <summary>"You are a moderator. You can kick/ban other users, set slow and sub mode but cannot IP ban a user"</summary>
		User,
		/// <summary>"You have full permission in chat. You can add/remove moderators and IP ban a user"</summary>
		Admin
	}

	public class HitboxAccessLevels
	{
		public int UserID				{ get; set; }
		public int AccessUserID			{ get; set; }
		public HitboxRole Settings		{ get; set; }
		public HitboxRole Account		{ get; set; }
		public HitboxRole Livestreams	{ get; set; }
		public HitboxRole Broadcast		{ get; set; }
		public HitboxRole Videos		{ get; set; }
		public HitboxRole Recordings	{ get; set; }
		public HitboxRole Statistics	{ get; set; }
		public HitboxRole Inbox			{ get; set; }
		public HitboxRole Revenues		{ get; set; }
		public HitboxRole Chat			{ get; set; }
		public HitboxRole Following		{ get; set; }
		public HitboxRole Teams			{ get; set; }
		public HitboxRole Subscriptions	{ get; set; }
		public HitboxRole Payments		{ get; set; }
		public bool IsSubscriber		{ get; set; }
		public bool IsFollower			{ get; set; }

		//public void Admin { get; set; }
		//public void Superadmin { get; set; }
		//public void Partner { get; set; }
	}

	public class HitboxCommBreak
	{
		public int		Count	  { get; set; }
		public string	Delay	  { get; set; } // TODO: delay in CommBreak
		public DateTime Timestamp { get; set; }
	}

	public class HitboxLastCommBreak
	{
		public int Count	  { get; set; }
		public int SecondsAgo { get; set; }
		public int Timeout	  { get; set; }
	}

	public class HitboxMediaStatus
	{
		public bool IsLive { get; set; }
		public int Viewers { get; set; }
	}

	public class HitboxFollowingStatus
	{
		public int UserId { get; set; }
		public bool Notify { get; set; }
		public int FollowerId { get; set; }
	}

	public class HitboxProfilePanel
	{
		public int	  ID		{ get; set; }
		public string Headline	{ get; set; }
		public string Content	{ get; set; }
		public string ImageLink { get; set; }
		public string ImageUrl	{ get; set; }
	}
}
