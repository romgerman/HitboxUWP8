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
##### REST
| Name                      | Methods      |
|---------------------------|--------------|
| User                      | GET          |
| User Access Levels        | GET          |
| Stream Key                | GET          |
| Twitter/Facebook Post     | POST         |
| Video                     | GET/GET list |
| List of Followers         | GET          |
| List of Following         | GET          |
| Follow/Unfollow           | POST/DELETE  |
| Check Following Status    | GET          |
| Check Subscription Status | GET          |
| Media Status              | GET          |
| Total Channel Views       | GET          |
| Game                      | GET/GET list |
| Run Commercial Break      | POST         |
| Get Last Commercial Break | GET          |

##### WebSockets
* LivestreamViewer (alpha)


### Soon
* Chat

#### Additional dependencies
* Newtonsoft.Json
* Microsoft.Bcl.Compression