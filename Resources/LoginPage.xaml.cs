using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HitboxUWP8
{
	public sealed partial class LoginPage : Page
	{
		private HitBoxClient _client;

		public LoginPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			object[] parameters = (object[])e.Parameter;

			bool forceLogin = (bool)parameters[0];
			_client = (HitBoxClient)parameters[1];

			browser.Source = new Uri(HitBoxEndpoint.Login + "?" + (forceLogin ? "force_auth=true&" : "") + "app_token=" + _client.AppKey, UriKind.Absolute);
		}

		private async void browser_LoadCompleted(object sender, NavigationEventArgs args)
		{
			string url = args.Uri.Query;
			bool isDone = false;

			if (url.StartsWith("?error=", StringComparison.CurrentCultureIgnoreCase))
			{
				if (url.Substring(7) == "user_canceled")
					_client.OnLoggedIn(new LoginEventArgs() { State = LoginEventArgs.States.Cancelled });

				_client.OnLoggedIn(new LoginEventArgs() { Error = url.Substring(7), State = LoginEventArgs.States.Error, Method = LoginEventArgs.Methods.FirstLogin });

				isDone = true;
			}
			else if (url.StartsWith("?request_token=", StringComparison.CurrentCultureIgnoreCase))
			{
				_client._authOrAccessToken = await _client.GetAccessToken(url.Substring(15));

				_client.User = await _client.GetUser(await HitBoxClient.GetUserFromToken(_client._authOrAccessToken), true);

				_client.OnLoggedIn(new LoginEventArgs() { Token = _client._authOrAccessToken, State = LoginEventArgs.States.OK, Method = LoginEventArgs.Methods.FirstLogin });

				_client._isLoggedIn = true;

				isDone = true;
			}
			else if (url.StartsWith("?authToken=", StringComparison.CurrentCultureIgnoreCase))
			{
				_client._authOrAccessToken = url.Substring(11);

				_client.User = await _client.GetUser(await HitBoxClient.GetUserFromToken(_client._authOrAccessToken), true);

				_client.OnLoggedIn(new LoginEventArgs() { Token = _client._authOrAccessToken, State = LoginEventArgs.States.OK, Method = LoginEventArgs.Methods.FirstLogin });

				_client._isLoggedIn = true;

				isDone = true;
			}

			if (isDone)
			{
				Frame.GoBack();
				//Frame.BackStack.RemoveAt(Frame.BackStack.Count - 1);
			}
		}

		private void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			browser.Refresh();
		}
	}
}
