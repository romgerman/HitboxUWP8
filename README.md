# HitboxUWP8
C# wrapper for hitbox api for Universal Windows Apps 8.1 (WP and W)

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
Only several methods are static


### Now supporting
##### REST
| Name                            | Methods      |
|---------------------------------|--------------|
| OAuth login                     | —            |
| User                            | GET          |
| User Access Levels              | GET          |
| Stream Key                      | GET          |
| Twitter/Facebook Post           | POST         |
| Video                           | GET/GET list |
| Livestreams                     | GET/GET list |
| Game                            | GET/GET list |
| List of Followers/Following     | GET          |
| Follow/Unfollow                 | POST/DELETE  |
| Check Following Status          | GET          |
| Check Subscription Status       | GET          |
| Media Status                    | GET          |
| Total Channel Views             | GET          |
| Run Commercial Break            | POST         |
| Get Last Commercial Break       | GET          |
| Get Chat/Viewer(Player) servers | GET          |
| Get Chat Colors                 | GET          |

##### WebSockets
* LivestreamViewer (alpha)


### Soon
* Chat (now you can only __connect__ to channel chat)

#### Additional dependencies
* Newtonsoft.Json
* Microsoft.Bcl.Compression (Addition to System.IO.Compression because you can't decompress data by default (only compress))