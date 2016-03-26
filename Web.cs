using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HitboxUWP8
{
	internal static class Web
	{
		public static async Task<string> GET(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "GET";

			try
			{
				using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
			catch (WebException e)
			{
				using (HttpWebResponse response = (HttpWebResponse)e.Response)
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
		}

		public static async Task<string> DELETE(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "DELETE";

			try
			{
				using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
			catch (WebException e)
			{
				using (HttpWebResponse response = (HttpWebResponse)e.Response)
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
		}

		public static async Task<string> POST(string url, string body)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ContentType = "application/json";

			using (var stream = await Task.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, null))
			{
				byte[] jsonAsBytes = Encoding.UTF8.GetBytes(body);
				await stream.WriteAsync(jsonAsBytes, 0, jsonAsBytes.Length);
			}

			try
			{
				using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
			catch (WebException e)
			{
				using (HttpWebResponse response = (HttpWebResponse)e.Response)
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
		}

		private static Stream CheckForCompression(HttpWebResponse response)
		{
			Stream responseStream = response.GetResponseStream();

			string encoding = response.Headers["Content-Encoding"];

			if (encoding != null)
			{
				if (encoding.Equals("gzip"))
					responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
				else if (encoding.Equals("deflate"))
					responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
			}

			return responseStream;
		}
	}
}
