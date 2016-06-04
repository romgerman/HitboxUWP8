using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HitboxUWP8
{
	/// <summary>Helper class for web requests</summary>
	internal static class Web
	{
		private enum Method { GET, DELETE, POST, PUT }

		public static async Task<string> GET(string url)
		{
			return await GET_DELETE(url);
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

		public static async Task<string> PUT(string url, string body)
		{
			return await POST_PUT(url, body);
		}

		private static async Task<string> GET_DELETE(string url, Method method = Method.GET)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method == Method.GET ? "GET" : "DELETE";

			//try
			//{
				using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			//}
			//catch (WebException e)
			//{
				//using (HttpWebResponse response = (HttpWebResponse)e.Response)
				//{
					//using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					//{
						//return reader.ReadToEnd();
					//}
				//}
			//}
		}

		private static async Task<string> POST_PUT(string url, string body, Method method = Method.POST)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method == Method.POST ? "POST" : "PUT";
			request.ContentType = "application/json";

			using (var stream = await Task.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, null))
			{
				byte[] jsonAsBytes = Encoding.UTF8.GetBytes(body);
				await stream.WriteAsync(jsonAsBytes, 0, jsonAsBytes.Length);
			}

			//try
			//{
				using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
				{
					using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			//}
			//catch (WebException e)
			//{
				//using (HttpWebResponse response = (HttpWebResponse)e.Response)
				//{
					//using (StreamReader reader = new StreamReader(CheckForCompression(response), Encoding.UTF8))
					//{
						//return reader.ReadToEnd();
					//}
				//}
			//}
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
			public static async Task<Stream> GET(string url)
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.Method = "GET";

				//try
				//{
					using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
					{
						return CheckForCompression(response);
					}
				//}
				//catch (WebException e)
				//{
					//using (HttpWebResponse response = (HttpWebResponse)e.Response)
					//{
						//return CheckForCompression(response);
					//}
				//}
			}
		}
	}
}
