using System;
#if DEBUG
using System.Diagnostics;
#endif

using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

using Newtonsoft.Json.Linq;

namespace HitboxUWP8
{
	/// <summary>Class for "viewing" a channel</summary>
	public class HitboxLivestreamViewer : IDisposable
	{
		/// <summary>Occurs when viewers/followers/subscribers count changes or livestream goes offline/online</summary>
		public event EventHandler<HitboxViewerStatusChangedArgs> StatusChanged;

		private const string url = "/viewer";

		private MessageWebSocket socket;
		private DataWriter writer;
		private bool isWatching;

		string channel;
		string username;
		string token;

		internal HitboxLivestreamViewer(string channel, string username = "", string token = "")
		{
			this.channel  = channel;
			this.username = username;
			this.token = token;

			socket = new MessageWebSocket();
			writer = new DataWriter(socket.OutputStream);

			socket.Control.MessageType = SocketMessageType.Utf8;
			socket.MessageReceived += socket_MessageReceived;
			socket.Closed += socket_Closed;
		}

		public async void Watch()
		{
			if (isWatching)
			{
				throw new HitboxException("already working");
			}
			else
			{
				if(channel == string.Empty)
					throw new HitboxException("you must enter channel name");

				string socketUrl = (await HitboxClientBase.GetViewerServers())[0];

				try
				{
					await socket.ConnectAsync(new Uri("ws://" + socketUrl + url));
				}
				catch(Exception e)
				{
#if DEBUG
					Debug.WriteLine("Viewer: " + e.ToString());
#else
					throw;
#endif
				}
				
				WriteToSocket(new JsonObject
				{
					{ "method", JsonValue.CreateStringValue("joinChannel") },
					{ "params", new JsonObject
						{
							{ "channel", JsonValue.CreateStringValue(channel) },
							{ "name",    JsonValue.CreateStringValue(username) },
							{ "token",   JsonValue.CreateStringValue(token) },
							{ "uuid",    JsonValue.CreateStringValue(Guid.NewGuid().ToString()) }
						}
					}
				}.Stringify());

				isWatching = true;
			}
		}
		
		private void socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			using (DataReader reader = args.GetDataReader())
			{
				reader.UnicodeEncoding = UnicodeEncoding.Utf8;
				string read = reader.ReadString(reader.UnconsumedBufferLength);

				JObject jmessage = JObject.Parse(read);

				switch(jmessage["method"].ToString())
				{
					case "infoMsg":
						{
							OnStatusChanged(new HitboxViewerStatusChangedArgs
							{
								Status = new HitboxMediaStatus
								{
									IsLive  = jmessage["params"]["online"].ToObject<bool>(),
									Viewers = jmessage["params"]["viewers"].ToObject<int>()
								},
								Followers   = jmessage["params"]["followers"].ToObject<int>(),
								Subscribers = jmessage["params"]["subscribers"].ToObject<int>()
							});
						}
						break;
					case "commercialBreak":
						{
							// {"method":"commercialBreak","params":{"channel":"ectvlol","count":"2","delay":"0","url":"http://hitbox.tv","timestamp":1459004216}}
						}
						break;
					default:
#if DEBUG
						Debug.WriteLine(jmessage.ToString());
#endif
						break;
				}
			}
		}

		private void socket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
		{
			if(args.Code != 1000)
			{
				
			}

#if DEBUG
			Debug.WriteLine("LivestreamViewer: " + args.Code.ToString());
#endif
		}

		private async void WriteToSocket(string message)
		{
			writer.WriteString(message);
			await writer.StoreAsync();
		}

		public void Stop()
		{
			if (isWatching)
			{
				socket.Close(1000, string.Empty);
				isWatching = false;
			}
		}

		// Event handler

		protected virtual void OnStatusChanged(HitboxViewerStatusChangedArgs e)
		{
			StatusChanged?.Invoke(this, e);
		}

		public void Dispose()
		{
			
		}
	}
}
