using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HitboxUWP8
{


	public class JsonHelper
    {
		#region Data structs


        public struct Bitrate
        {
            public string url;
            public int bitrate;
            public string label;
            public string provider;
            public bool isDefault;
        }

        public class Clip
        {
            public List<Bitrate> bitrates;
        }

        public struct LivestreamMediaObject
        {
            public Clip clip;
        }
		
		

        #endregion

		public static LivestreamMediaObject GetLivestreamMediaObject(string json)
        {
            return JsonConvert.DeserializeObject<LivestreamMediaObject>(json);
        }
    }
}
