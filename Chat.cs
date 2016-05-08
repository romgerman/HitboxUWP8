using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

using Newtonsoft.Json.Linq;

namespace HitboxUWP8
{
	/// <summary>Chat</summary>
	public class HitBoxChat // DOTO: chat connection timeout. "If you cannot connect after 8 seconds you should grab another server and connection ID."
	{
		/// <summary>Occurs when user has been connected to a chat</summary>
		public event EventHandler Connected;
		/// <summary>Occurs when user has been logged in chat</summary>
		public event EventHandler<ChatLoggedInEventArgs> LoggenIn;
		/// <summary>Occurs on new message</summary>
		public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;

		private static class MessageType
		{
			public const char Connected = '1';
			public const char Echo = '2';
			public const char Interactions = '5';
		}

		private MessageWebSocket _socket;
		private DataWriter _writer;

		private bool _isConnected;
		private bool _isLoggedIn;

		private string _token;
		private string _username;
		private string _channel;

		private HitBoxRole _role = HitBoxRole.Guest;

		public HitBoxChat()	{ }

		private HitBoxChat(string token, string username)
		{
			_token = token;
			_username = username;
		}

		/// <summary>Connect to a chat server</summary>
		public async Task Connect()
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

		/// <summary>Disconnect from the chat server</summary>
		public void Disconnect()
		{
			if(_isConnected)
			{
				if(_isLoggedIn)
					Logout();

				_socket.Close(1000, string.Empty);
			}
		}

		/// <summary>Login to a channel chat</summary>
		public void Login(string channel)
		{
			if (!_isConnected)
				throw new HitBoxException();

			if(!_isLoggedIn)
			{
				WriteToSocket("5:::" + new JsonObject
				{
					{ "name", JsonValue.CreateStringValue("message") },
					{ "args", new JsonArray
						{
							new JsonObject
							{
								{ "method", JsonValue.CreateStringValue("joinChannel") },
								{ "params", new JsonObject
									{
										{ "channel", JsonValue.CreateStringValue(channel.ToLower()) },
										{ "name", JsonValue.CreateStringValue(_username == null ? "UnknownSoldier" : _username) },
										{ "token", JsonValue.CreateStringValue(_token == null ? "null" : _token) },
										{ "isAdmin", JsonValue.CreateBooleanValue(false) }
									}
								}
							}
						}
					}
				}.Stringify());
			}

			_channel = channel;
		}

		private void Logout()
		{
			WriteToSocket("5:::{\"name\":\"message\",\"args\":[	{\"method\":\"partChannel\",\"params\":{\"name\":\"" + (_username == null ? "UnknownSoldier" : _username) + "\"}}]}");
			_isLoggedIn = false;
		}

		/// <param name="message">Limited to 300 chars</param>
		public void SendMessage(string message)
		{
			if (!_isLoggedIn)
				throw new HitBoxException(ExceptionList.NotLoggedIn);

			if (message == null)
				throw new ArgumentNullException("message");

			WriteToSocket("5:::" + new JsonObject
			{
				{ "name", JsonValue.CreateStringValue("message") },
				{ "args", new JsonArray
					{
						new JsonObject
						{
							{ "method", JsonValue.CreateStringValue("chatMsg") },
							{ "params", new JsonObject
								{
									{ "channel", JsonValue.CreateStringValue(_channel) },
									{ "name", JsonValue.CreateStringValue(_username) },
									//{ "nameColor", JsonValue.CreateStringValue("") },
									{ "text", JsonValue.CreateStringValue(message) }
								}
							}
						}
					}
				}
			}.Stringify());
		}

		// http://developers.hitbox.tv/#permissions-and-roles

		private void _socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			using (DataReader reader = args.GetDataReader())
			{
				string read = string.Empty;
				reader.UnicodeEncoding = UnicodeEncoding.Utf8;
				read = reader.ReadString(reader.UnconsumedBufferLength);

				switch (read[0])
				{
					case MessageType.Connected:
						{
							_isConnected = true;
							OnConnected(new EventArgs());
						}
						break;
					case MessageType.Echo:
						{
							WriteToSocket("2::");
						}
						break;
					case MessageType.Interactions:
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
						{
							Debug.WriteLine(read);

							// TODO: reconnect message
						}
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

		protected virtual void OnMessageReceived(ChatMessageReceivedEventArgs e)
		{
			if (MessageReceived != null)
				MessageReceived(this, e);
		}

		#endregion

	}
}
