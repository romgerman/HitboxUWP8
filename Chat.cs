using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace HitboxUWP8
{
	public class HitBoxChat // DOTO: chat connection timeout. "If you cannot connect after 8 seconds you should grab another server and connection ID."
	{
		public event EventHandler Connected;
		public event EventHandler<ChatLoggedInEventArgs> LoggenIn;

		private MessageWebSocket _socket;
		private DataWriter _writer;

		private bool _isConnected;
		private bool _isLoggedIn;

		private string _token;
		private string _username;

		private HitBoxRole _role = HitBoxRole.Guest;

		public HitBoxChat()	{ }

		private HitBoxChat(string token, string username)
		{
			_token = token;
			_username = username;
		}

		public async void Connect()
		{
			_socket = new MessageWebSocket();
			_writer = new DataWriter(_socket.OutputStream);

			_socket.Control.MessageType = SocketMessageType.Utf8;
			_socket.MessageReceived += _socket_MessageReceived;
			_socket.Closed += _socket_Closed;

			IList<string> servers = await HitBoxClientBase.GetChatServers();

			string serverUrl = servers[0];
			string webSocketID  = await HitBoxClientBase.GetChatServerSocketID(serverUrl);
			string connectionUrl = "ws://" + serverUrl + "/socket.io/1/websocket/" + webSocketID;

			await _socket.ConnectAsync(new Uri(connectionUrl));
		}

		public void Disconnect()
		{
			if(_isConnected)
			{
				if(_isLoggedIn)
					Logout();

				_socket.Close(1000, "");
			}
		}

		public void Login(string channel)
		{
			if (!_isConnected)
				throw new HitBoxException();

			if(!_isLoggedIn)
				WriteToSocket("5:::{\"name\":\"message\",\"args\":[{\"method\":\"joinChannel\",\"params\":{\"channel\":\"" + channel.ToLower() + "\",\"name\":\"" + (_username == null ? "UnknownSoldier" : _username) + "\",\"token\":\"" + (_token == null ? "null" : _token) + "\",\"isAdmin\":false}}]}");
		}

		private void Logout()
		{
			WriteToSocket("5:::{\"name\":\"message\",\"args\":[	{\"method\":\"partChannel\",\"params\":{\"name\":\"" + (_username == null ? "UnknownSoldier" : _username) + "\"}}]}");
			_isLoggedIn = false;
		}

		// http://developers.hitbox.tv/#permissions-and-roles

		private void _socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			using (DataReader reader = args.GetDataReader())
			{
				string read = string.Empty;
				reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
				read = reader.ReadString(reader.UnconsumedBufferLength);

				switch (read[0])
				{
					case '1': // Connected
						{
							_isConnected = true;
							OnConnected(new EventArgs());
						}
						break;
					case '2': // Echo
						{
							WriteToSocket("2::");
						}
						break;
					case '5': // Interactions
						{
							JObject response = JObject.Parse(read.Substring(4));
							JObject content = JObject.Parse(response["args"][0].ToString());

							switch (content["method"].ToString())
							{
								case "loginMsg":
									{
										switch(content["params"]["role"].ToString())
										{
											case "guest":
												_role = HitBoxRole.Guest;
												break;
											case "anon":
												_role = HitBoxRole.Anon;
												break;
											case "user":
												_role = HitBoxRole.User;
												break;
											case "admin":
												_role = HitBoxRole.Admin;
												break;
										}

										_isLoggedIn = true;
										OnLoggedIn(new ChatLoggedInEventArgs(_role));
									}
									break;
							}
						} // Interactions
						break;
					default:
						Debug.WriteLine(read);
						break;
				} // read[0] switch
			} // using DataReader
		}

		private void _socket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
		{
			//throw new NotImplementedException();
		}

		private async void WriteToSocket(string message)
		{
			_writer.WriteString(message);
			await _writer.StoreAsync();
		}

		#region Handlers

		protected virtual void OnConnected(EventArgs e)
		{
			if (Connected != null)
				Connected(this, e);
		}

		protected virtual void OnLoggedIn(ChatLoggedInEventArgs e)
		{
			if (LoggenIn != null)
				LoggenIn(this, e);
		}

		#endregion

	}
}
