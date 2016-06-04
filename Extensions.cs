using Newtonsoft.Json.Linq;

namespace HitboxUWP8
{
	public static class Extensions
	{
		public static bool IsNull(this JToken token)
		{
			return token == null || token.Type == JTokenType.Null;
		}
	}
}
