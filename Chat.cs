using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if DEBUG
using System.Diagnostics;
#endif

using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;

using Newtonsoft.Json.Linq;

namespace HitboxUWP8
{
	/// <summary>Chat</summary>
	public class HitboxChat : IDisposable
	{
		/// <summary>Occurs when user has been connected to a chat</summary>
		public event EventHandler Connected;
		/// <summary>Occurs when user has been logged in chat</summary>
		public event EventHandler<HitboxChatLoggedInEventArgs> LoggedIn;
		/// <summary>Occurs on new message</summary>
		public event EventHandler<HitboxChatMessageReceivedEventArgs> MessageReceived;

		public bool IsConnected { get; private set; }
		public bool IsLoggedIn { get; private set; }

		private static class MessageType
		{
			public const char Connected = '1';
			public const char Echo = '2';
			public const char Interactions = '5';
		}

		private MessageWebSocket _socket;
		private DataWriter _writer;

		private ThreadPoolTimer _timer;

		private string _token;
		private string _username;
		private string _channel;

		private HitboxRole _role = HitboxRole.Guest;

		public HitboxChat()	{ }

		private HitboxChat(string token, string username)
		{
			_token = token;
			_username = username;
		}

		/// <summary>Connect to a chat server</summary>
		public async Task Connect()
		{
			Debug.WriteLine("Connect");

			_socket = new MessageWebSocket();
			_writer = new DataWriter(_socket.OutputStream);

			_socket.Control.MessageType = SocketMessageType.Utf8;
			_socket.MessageReceived += _socket_MessageReceived;
			_socket.Closed += _socket_Closed;

			IList<string> servers = await HitboxClientBase.GetChatServers();

			string serverUrl = servers[new Random().Next(servers.Count)];
			string webSocketID = await HitboxClientBase.GetChatServerSocketID(serverUrl);
			string connectionUrl = "ws://" + serverUrl + "/socket.io/1/websocket/" + webSocketID;

			_timer = ThreadPoolTimer.CreateTimer(async (timer) =>
			{
				if (!IsConnected)
				{
					_socket.Close(1000, string.Empty);

					Debug.WriteLine("Close");

					await Connect();
				}
				else
				{
					Debug.WriteLine("timer Cancel");
					timer.Cancel();
				}
			}, TimeSpan.FromSeconds(8.0));

			try
			{

				await _socket.ConnectAsync(new Uri(connectionUrl));
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}

		/// <summary>Disconnect from the chat server</summary>
		public void Disconnect()
		{
			if(IsConnected)
			{
				if(IsLoggedIn)
					Logout();

				_socket.Close(1000, string.Empty);

				Dispose();
			}
		}

		/// <summary>Login to a channel chat</summary>
		public void Login(string channel)
		{
			if (!IsConnected)
				throw new HitboxException();

			if(!IsLoggedIn)
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

		private void Logout() // TODO: chat logout
		{
			WriteToSocket("5:::{\"name\":\"message\",\"args\":[	{\"method\":\"partChannel\",\"params\":{\"name\":\"" + (_username == null ? "UnknownSoldier" : _username) + "\"}}]}");
			IsLoggedIn = false;
		}

		/// <param name="message">Limited to 300 chars</param>
		public void SendMessage(string message)
		{
			if (!IsLoggedIn)
				throw new HitboxException(ExceptionList.NotLoggedIn);

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
			try
			{
				using (DataReader reader = args.GetDataReader())
				{
					reader.UnicodeEncoding = UnicodeEncoding.Utf8;
					string read = reader.ReadString(reader.UnconsumedBufferLength);

					//Debug.WriteLine(read);

					switch (read[0])
					{
						case MessageType.Connected:
							{
								IsConnected = true;
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
											switch (content["params"]["role"].ToString())
											{
												case "guest":
													_role = HitboxRole.Guest;
													break;
												case "anon":
													_role = HitboxRole.Anon;
													break;
												case "user":
													_role = HitboxRole.User;
													break;
												case "admin":
													_role = HitboxRole.Admin;
													break;
											}

											IsLoggedIn = true;
											OnLoggedIn(new HitboxChatLoggedInEventArgs(_role));
										}
										break;
									case "chatMsg":
										{
											OnMessageReceived(new HitboxChatMessageReceivedEventArgs
											{
												Username = content["params"]["name"].ToString(),
												Text = content["params"]["text"].ToString(),
												//Time = DateTime.Parse(content["params"]["time"].ToString()),
												IsFollower = content["params"]["isFollower"].ToObject<bool>(),
												IsSubscriber = content["params"]["isSubscriber"].ToObject<bool>(),
												IsOwner = content["params"]["isOwner"].ToObject<bool>(),
												IsCommunity = content["params"]["isCommunity"].ToObject<bool>(),
												IsStaff = content["params"]["isStaff"].ToObject<bool>()
											});
										}
										break;
								}
							} // Interactions
							break;
						default:
							{
#if DEBUG
								Debug.WriteLine(read);
#endif

								// TODO: reconnect message
							}
							break;
					} // read[0] switch
				} // using DataReader
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
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
			Connected?.Invoke(this, e);
		}

		protected virtual void OnLoggedIn(HitboxChatLoggedInEventArgs e)
		{
			LoggedIn?.Invoke(this, e);
		}

		protected virtual void OnMessageReceived(HitboxChatMessageReceivedEventArgs e)
		{
			MessageReceived?.Invoke(this, e);
		}

		#endregion

		public void Dispose()
		{
			_socket = null;
			_writer = null;
			_timer = null;
		}

	}
}
