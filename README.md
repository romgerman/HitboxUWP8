# HitboxUWP8
Unnoficial C# hitbox library for Universal Windows Apps 8.1 (WP and W)

### How to work with it
```cs
HitBoxClient client = new HitBoxClient(app_key, app_secret); // Initialize main class with your key and secret
client.LoggedIn += (s, args) => // Catch logged in event
{
	if(args.State == LoginEventArgs.States.OK)
	{
		// Do whatever you want
	}
};
client.Login(); // It will open a new page with hitbox api login page
// ...
```
Only several methods are static


### Now supporting
* Base api methods (login, video/livestream get/list)

### Soon
* LivestreamViewer
* Chat