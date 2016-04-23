using System.Collections.Generic;
using System.Threading.Tasks;

namespace HitboxUWP8
{
	/// <summary>Main class that you should use</summary>
	public class HitBoxClient : HitBoxClientBase
	{
		public HitBoxClient(string key, string secret) : base(key, secret) { }

		// TODO: methods with HitBoxObjects

		public async Task<IList<HitBoxFollower>> GetFollowers(int offset = 0, int limit = 10)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(ExceptionList.NotLoggedIn);

			return await GetFollowers(User.Username, offset, limit);
		}

		public async Task<IList<HitBoxFollower>> GetFollowing(int offset = 0, int limit = 10)
		{
			return await GetFollowing(User.Username, offset, limit);
		}
	}
}
