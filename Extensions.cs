using Newtonsoft.Json.Linq;

namespace HitboxUWP8
{
	internal static class Extensions
	{
		/// <summary>Check if null</summary>
		public static bool IsNull(this JToken token)
		{
			return token == null || token.Type == JTokenType.Null;
		}

		/// <summary>Return null if null and string if not null</summary>
		public static string NotNullToString(this JToken token)
		{
			return (token.IsNull() ? null : token.ToString());
		}

		/// <summary>Specially for hitbox</summary>
		public static T ToValue<T>(this JToken token, bool useNull = false)
		{
			object value = new object();

			if (typeof(T) == typeof(bool))
			{
				if (useNull)
					value = token.IsNull() ? false : token.ToObject<bool>();
				else
					value = token.ToString() == "1";
			}
			else if(typeof(T) == typeof(int))
			{
				if (useNull)
					value = token.IsNull() ? 0 : token.ToObject<int>();
				else
					value = token.ToString() == "" ? 0 : token.ToObject<int>();
			}

			return (T)value;
		}
		
	}
}
