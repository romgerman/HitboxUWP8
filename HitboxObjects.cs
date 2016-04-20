using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		internal HitBoxClientBase _client;

		public async Task<bool> Follow()   => await _client.Follow(Username);
		public async Task<bool> Unfollow() => await _client.Unfollow(ID);

		public async Task<bool> CheckFollowingStatus() => await HitBoxClientBase.CheckFollowingStatus(Username, _client.User.Username);

		public async Task<int> GetTotalViews() => await HitBoxClientBase.GetTotalViews(Username);

		public async Task<IList<HitBoxProfilePanel>> GetProfilePanels() => await HitBoxClientBase.GetProfilePanels(Username);
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

	public enum HitBoxRole
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

	public class HitBoxAccessLevels
	{
		public int UserID				{ get; set; }
		public int AccessUserID			{ get; set; }
		public HitBoxRole Settings		{ get; set; }
		public HitBoxRole Account		{ get; set; }
		public HitBoxRole Livestreams	{ get; set; }
		public HitBoxRole Broadcast		{ get; set; }
		public HitBoxRole Videos		{ get; set; }
		public HitBoxRole Recordings	{ get; set; }
		public HitBoxRole Statistics	{ get; set; }
		public HitBoxRole Inbox			{ get; set; }
		public HitBoxRole Revenues		{ get; set; }
		public HitBoxRole Chat			{ get; set; }
		public HitBoxRole Following		{ get; set; }
		public HitBoxRole Teams			{ get; set; }
		public HitBoxRole Subscriptions	{ get; set; }
		public HitBoxRole Payments		{ get; set; }
		public bool IsSubscriber		{ get; set; }
		public bool IsFollower			{ get; set; }

		//public void Admin { get; set; }
		//public void Superadmin { get; set; }
		//public void Partner { get; set; }
	}

	public class HitBoxCommBreak
	{
		public int		Count	  { get; set; }
		public string	Delay	  { get; set; } // TODO: delay in CommBreak
		public DateTime Timestamp { get; set; }
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

	public class HitBoxProfilePanel
	{
		public int	  ID		{ get; set; }
		public string Headline	{ get; set; }
		public string Content	{ get; set; }
		public string ImageLink { get; set; }
		public string ImageUrl	{ get; set; }
	}
}
