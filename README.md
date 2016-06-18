# HitboxUWP8
C# wrapper for hitbox api for Universal Windows Apps 8.1 (WP and W) (.NET 4.6)

#### Get it from NuGet
https://www.nuget.org/packages/HitboxUWP8/ (may be not the latest version)

### It's simple
```cs
HitBoxClient client = new HitBoxClient(app_key, app_secret); // Initialize main class with your key and secret
client.LoggedIn += (s, args) => // Catch logged in event
{
	if(args.State == LoginEventArgs.States.OK)
	{
		// Do whatever you want
	}
};
client.Login(); // It will open a new frame with hitbox api login page
// ...
```

If you want to use built-in empty page for your application (redirecting after login), enter "**ms-appx-web:///Resources/success.html**" in Application URI field (Developer Applications section).

### Now supporting
##### REST
| Name                              | Methods      |
|-----------------------------------|--------------|
| OAuth Login                       | +            |
| User                              | GET          |
| Check Verified Email              | GET          |
| User Access Levels                | GET          |
| Stream Key & Reset Stream Key     | GET/PUT      |
| Commercial Break (Run & Get Last) | POST/GET     |
| Profile Panels                    | GET          |
| Twitter/Facebook Post             | POST         |
| Livestreams (Get One & Get List)  | GET          |
| Video (Get One & Get List)        | GET          |
| Game (Get One & Get List)         | GET          |
| Media Status                      | GET          |
| Total Channel Views               | GET          |
| List of Followers/Following       | GET          |
| Check Following Status            | GET          |
| Follow/Unfollow                   | POST/DELETE  |
| Check Subscription Status         | GET          |
| Get Chat/Viewer(Player) servers   | GET          |
| Get Chat Colors                   | GET          |

##### WebSockets
* LivestreamViewer (alpha)

### Soon
* Chat (currently you can only __connect__ to channel chat)

#### Additional dependencies
* Newtonsoft.Json
* Microsoft.Bcl.Compression (Addition to System.IO.Compression because you can't decompress data by default (only compress))