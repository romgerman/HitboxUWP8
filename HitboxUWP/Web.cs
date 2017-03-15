using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HitboxUWP
{
	/// <summary>Helper class for web requests</summary>
	internal static class Web
	{
		private enum Method { GET, DELETE, POST, PUT }

		private static Random random = new Random(41780);

		public static async Task<string> GET(string url, bool disableCache = false)
		{
			return await GET_DELETE(url, Method.GET, disableCache);
		}

		public static async Task<string> DELETE(string url)
		{
			return await GET_DELETE(url, Method.DELETE);
		}

		public static async Task<string> POST(string url, string body)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ContentType = "application/json";

			using (Stream stream = await Task.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, null))
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

		public static async Task<string> PUT(string url, string body)
		{
			return await POST_PUT(url, body);
		}

		private static async Task<string> GET_DELETE(string url, Method method = Method.GET, bool disableCache = false)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (disableCache ? "&random=" + random.Next(100000) : string.Empty));
			request.Method = method == Method.GET ? "GET" : "DELETE";
			
			using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
			{
				using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private static async Task<string> POST_PUT(string url, string body, Method method = Method.POST)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method == Method.POST ? "POST" : "PUT";
			request.ContentType = "application/json";

			using (Stream stream = await Task.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, null))
			{
				byte[] jsonAsBytes = Encoding.UTF8.GetBytes(body);
				await stream.WriteAsync(jsonAsBytes, 0, jsonAsBytes.Length);
			}

			using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
			{
				using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private static Stream CheckForCompression(HttpWebResponse response)
		{
			Stream responseStream = response.GetResponseStream();

			string encoding = response.Headers["Content-Encoding"];

			if (encoding != null)
			{
				if (encoding.Equals("gzip", StringComparison.CurrentCultureIgnoreCase))
					responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
				else if (encoding.Equals("deflate", StringComparison.CurrentCultureIgnoreCase))
					responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
			}

			return responseStream;
		}

		public static class Streams
		{
			public static async Task<Stream> GET(string url, bool disableCache = false)
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (disableCache ? "&random=" + random.Next(100000) : string.Empty));
				request.Method = "GET";
				
				using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
				{
					return CheckForCompression(response);
				}
			}
		}
	}
}
