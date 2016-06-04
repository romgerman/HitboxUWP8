using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HitboxUWP8
{
	public sealed partial class LoginPage : Page
	{
		private HitboxClientBase _client;

		public LoginPage()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			object[] parameters = (object[])e.Parameter;

			bool forceLogin = (bool)parameters[0];
			_client = (HitboxClientBase)parameters[1];

			browser.Source = new Uri(HitboxEndpoint.Login + "?" + (forceLogin ? "force_auth=true&" : "") + "app_token=" + _client.appKey, UriKind.Absolute);
		}

		private async void browser_LoadCompleted(object sender, NavigationEventArgs args)
		{
			string url = args.Uri.Query;
			bool isDone = false;

			if (url.StartsWith("?error=", StringComparison.CurrentCultureIgnoreCase))
			{
				string error = url.Substring(7);

				if (error.Equals("user_canceled", StringComparison.CurrentCultureIgnoreCase))
					_client.OnLoggedIn(new LoginEventArgs() { Error = error, State = LoginEventArgs.States.Cancelled });

				_client.OnLoggedIn(new LoginEventArgs() { Error = error, State = LoginEventArgs.States.Error, Method = LoginEventArgs.Methods.FirstLogin });

				isDone = true;
			}
			else if (url.StartsWith("?request_token=", StringComparison.CurrentCultureIgnoreCase))
			{
				_client.authOrAccessToken = await _client.GetAccessToken(url.Substring(15));

				_client.User = await _client.GetUser(await _client.GetUserFromToken(_client.authOrAccessToken), true);

				_client.OnLoggedIn(new LoginEventArgs() { Token = _client.authOrAccessToken, State = LoginEventArgs.States.OK, Method = LoginEventArgs.Methods.FirstLogin });

				_client.isLoggedIn = true;

				isDone = true;
			}
			else if (url.StartsWith("?authToken=", StringComparison.CurrentCultureIgnoreCase))
			{
				_client.authOrAccessToken = url.Substring(11);

				_client.User = await _client.GetUser(await _client.GetUserFromToken(_client.authOrAccessToken), true);

				_client.OnLoggedIn(new LoginEventArgs() { Token = _client.authOrAccessToken, State = LoginEventArgs.States.OK, Method = LoginEventArgs.Methods.FirstLogin });

				_client.isLoggedIn = true;

				isDone = true;
			}

			if (isDone)
			{
				Frame.GoBack();
			}
		}

		private void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			browser.Refresh();
		}
	}
}
