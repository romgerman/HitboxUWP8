using System;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace HitboxUWP8
{
	public class HitBoxLivestreamViewer
	{
		public class Parameters
		{
			public string Channel = string.Empty;
			public string Username = string.Empty;
			public string Token = string.Empty;
			public bool IsSubscriber;
			public bool? IsFollower = null;
		}

		public event EventHandler<ViewerCountChangedArgs> ViewerCountChanged;

		private const string url = "/viewer";

		private static HitBoxLivestreamViewer _instance;

		public static HitBoxLivestreamViewer Instance
		{
			get
			{
				if (_instance == null)
					_instance = new HitBoxLivestreamViewer();
				return _instance;
			}
		}

		private MessageWebSocket _socket;
		private DataWriter _writer;
		private bool _isWorking;

		private int _currentViewerCount;

		private HitBoxLivestreamViewer() { }

		public async void Connect(string socketUrl, Parameters parameters)
		{
			if (_isWorking)
			{
				throw new Exception("already working");
			}
			else
			{
				if(parameters.Channel == string.Empty)
				{
					throw new Exception("you must enter channel name");
				}

				_socket = new MessageWebSocket();
				_socket.Control.MessageType = SocketMessageType.Utf8;
				_socket.MessageReceived += socketMessageReceived;
				_socket.Closed += socketClosed;

				if(socketUrl.StartsWith("ws://", StringComparison.CurrentCultureIgnoreCase))
					await _socket.ConnectAsync(new Uri(socketUrl.ToString() + url));
				else
					await _socket.ConnectAsync(new Uri("ws://" + socketUrl.ToString() + url));

				_writer = new DataWriter(_socket.OutputStream);

				WriteToSocket(new JsonObject
				{
					{ "method", JsonValue.CreateStringValue("joinChannel") },
					{ "params", new JsonObject
						{
							{ "channel", JsonValue.CreateStringValue(parameters.Channel) },
							{ "name", JsonValue.CreateStringValue(parameters.Username) },
							{ "token", JsonValue.CreateStringValue(parameters.Token) },
							{ "isSubscriber", JsonValue.CreateBooleanValue(parameters.IsSubscriber) },
							{ "isFollower", parameters.IsFollower == null ? null : JsonValue.CreateBooleanValue(parameters.IsFollower.Value) }
						}
					}
				}.Stringify());
				_isWorking = true;
			}
		}
		
		private void socketMessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			//throw new NotImplementedException();
		}

		private void socketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
		{
			Debug.WriteLine(args.Code.ToString());
			
			//throw new NotImplementedException();
		}

		private async void WriteToSocket(string message)
		{
			_writer.WriteString(message);
			await _writer.StoreAsync();
		}

		public void Disconnect()
		{
			if (!_isWorking)
				throw new Exception("nothing is started");
			else
			{
				_socket.Close(1000, null);
				_isWorking = false;
			}

			
		}

		protected virtual void OnViewerCountChanged(ViewerCountChangedArgs e)
		{
			if (ViewerCountChanged != null)
				ViewerCountChanged(this, e);
		}
	}
}
