using System;
using System.Diagnostics;

using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

using Newtonsoft.Json.Linq;

namespace HitboxUWP8
{
	/// <summary>TODO: HitBoxLivestreamViewer info</summary>
	public class HitBoxLivestreamViewer // TODO: exceptions and comments
	{
		public class Parameters
		{
			public string Channel	= string.Empty;
			public string Username	= string.Empty;
			public string Token		= string.Empty;
		}

		/// <summary>Occurs when viewers/followers/subscribers count changes or livestream goes offline/online</summary>
		public event EventHandler<ViewerStatusChangedArgs> StatusChanged;

		private const string url = "/viewer";

		private MessageWebSocket _socket;
		private DataWriter _writer;
		private bool _isWatching;

		private Parameters _params;

		internal HitBoxLivestreamViewer(Parameters parameters)
		{
			_params = parameters;
			_socket = new MessageWebSocket();
			_writer = new DataWriter(_socket.OutputStream);

			_socket.Control.MessageType = SocketMessageType.Utf8;
			_socket.MessageReceived += socket_MessageReceived;
			_socket.Closed += socket_Closed;
		}

		public async void Watch()
		{
			if (_isWatching)
			{
				throw new HitBoxException("already working");
			}
			else
			{
				if(_params.Channel == string.Empty)
					throw new HitBoxException("you must enter channel name");

				string socketUrl = (await HitBoxClientBase.GetViewerServers())[0];

				try
				{
					await _socket.ConnectAsync(new Uri("ws://" + socketUrl + url));
				}
				catch(Exception e)
				{
					Debug.WriteLine("Viewer: " + e.ToString());
				}
				
				WriteToSocket(new JsonObject
				{
					{ "method", JsonValue.CreateStringValue("joinChannel") },
					{ "params", new JsonObject
						{
							{ "channel", JsonValue.CreateStringValue(_params.Channel) },
							{ "name", JsonValue.CreateStringValue(_params.Username) },
							{ "token", JsonValue.CreateStringValue(_params.Token) },
							{ "uuid", JsonValue.CreateStringValue(Guid.NewGuid().ToString()) }
						}
					}
				}.Stringify());
				_isWatching = true;
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
							OnStatusChanged(new ViewerStatusChangedArgs
							{
								Status = new HitBoxMediaStatus
								{
									IsLive = jmessage["params"]["online"].ToObject<bool>(),
									Viewers = jmessage["params"]["viewers"].ToObject<int>()
								},
								Followers = jmessage["params"]["followers"].ToObject<int>(),
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
						Debug.WriteLine(jmessage.ToString());
						break;
				}
			}
		}

		private void socket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
		{
			if(args.Code != 1000)
			{
				
			}

			Debug.WriteLine("LivestreamViewer: " + args.Code.ToString());
		}

		private async void WriteToSocket(string message)
		{
			_writer.WriteString(message);
			await _writer.StoreAsync();
		}

		public void Stop()
		{
			if (!_isWatching)
				throw new HitBoxException("nothing is started");
			else
			{
				_socket.Close(1000, string.Empty);
				_isWatching = false;
			}
		}

		// Event handler

		protected virtual void OnStatusChanged(ViewerStatusChangedArgs e)
		{
			if (StatusChanged != null)
				StatusChanged(this, e);
		}
	}
}
