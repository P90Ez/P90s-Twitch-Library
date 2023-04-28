
# P90's Twitch Library

A C# library with the goal to enable easy access to the Twitch services. Scalability is also a big factor.


## Authors

- [@P90Ez](https://github.com/P90Ez)
[<img align="right" alt="P90Ez | Twitch" width="22px" src="https://cdn.jsdelivr.net/npm/simple-icons@v3/icons/twitch.svg" />](https://twitch.tv/p90ez.com)
[<img align="right" alt="P90Ez | Twitter" width="22px" src="https://cdn.jsdelivr.net/npm/simple-icons@v3/icons/twitter.svg" />](https://twitter.com/P90Eazy)
[<img align="right" alt="P90Ez | YouTube" width="22px" src="https://cdn.jsdelivr.net/npm/simple-icons@v3/icons/youtube.svg" />](https://p90ez.com/abop90code)
[<img align="right" alt="P90Ez | PayPal" width="22px" src="https://cdn.jsdelivr.net/npm/simple-icons@v3/icons/paypal.svg" />](https://paypal.me/p90ez)

## Features

- Easy to use (Auth) **Token Generator**. (see *[Login](https://github.com/P90Ez/P90s-Twitch-Library/blob/master/P90's%20Twitch%20Library/Login/Login.cs)*)
- Straightforward implementation of Twitch's **API Endpoints**. (see *[API](https://github.com/P90Ez/P90s-Twitch-Library/tree/master/P90's%20Twitch%20Library/API/Endpoints)*\*)
- Complete **Chat** access with up-to-date moderation tools. (see *[IRC](https://github.com/P90Ez/P90s-Twitch-Library/blob/master/P90's%20Twitch%20Library/IRC/Controller.cs)*)
- Convenient implementation of **Twitch PubSub** to monitor Channel Point redemtions, usage of bits and even channel subscriptions. (see *Pubsub*\*\*)
- Simple usage of **EventSub** let's you monitor when a broadcaster goes online, new followers and subscriber and much more. (see *[EventSub](https://github.com/P90Ez/P90s-Twitch-Library/tree/master/P90's%20Twitch%20Library/EventSub)*\*)
- Uncomplicated use of **Extensions**. (see *Extensions*\*\*)


    \* Implementation was not finished yet, but every available function was tested and is ready to go.

    \*\* Will get implemented in the future. 


## Usage/Examples

- **How to create an User Access or App Access Token?** [How does Twitch's Token system work?](https://dev.twitch.tv/docs/authentication) - Chech out my YouTube Video on how Twitch Tokens work [here](https://www.youtube.com/watch?v=5Bd1EC7541k) (english subtitles are available).
```c#
using P90Ez.Twitch;

//Set your App's redirect URL (if you don't have an 'App', create one here: https://dev.twitch.tv/console)
Login.redirecturl = "http://localhost:3000";



//Create an User Access Token with the Implicit Grant Flow (for Apps that don't use a backend)
var credentials = Login.ImplicitGrantFlow("YOUR APP's CLIENT ID", new List<string>() {"PUT SCOPES IN HERE"});

//here is an example:
var credentials = Login.ImplicitGrantFlow("123456789abcdefgh", new List<string>() {"channel:manage:broadcast", "bits:read"});



//Create an User Access Token with the Authorization Code Flow (for Apps that use a front- & backend)
//put this part in your frontend and send the 'code' to your backend
string code = Login.AuthorizationCodeFlow_FrontEnd("YOUR APP's CLIENT ID", new List<string>() {"PUT SCOPES IN HERE"});
//put this part in your backend, you realy don't want to expose your client secret to the frontend
var credentials = Login.AuthorizationCodeFlow_BackEnd("YOUR APP's CLIENT ID", "YOUR APP's CLIENT SECRET", code);

//here is an example:
string code = Login.AuthorizationCodeFlow_FrontEnd("123456789abcdefgh", new List<string>() {"channel:manage:broadcast", "bits:read"});
var credentials = Login.AuthorizationCodeFlow_BackEnd("123456789abcdefgh", "hgfedcba987654321", code);



//Create an App Access Token with the Client Credentials Grant Flow (for Apps that only a backend)
var credentials = Login.ClientCredentialsGrantFlow("YOUR APP's CLIENT ID", "YOUR APP's CLIENT SECRET");

//here is an example:
var credentials = Login.ClientCredentialsGrantFlow("123456789abcdefgh", "hgfedcba987654321");
```

---

- **How to use an API endpoint?**
```c#
using P90Ez.Twitch.API.Endpoints;

//the super simple version: (SimplifiedRequests only contains hard to use and frequently used endpoints, like 'GetBroadcasterID' or moderation tools)

SimplifiedRequests simpleRq = new SimplifiedRequests(credentials);
long id = simpleRq.GetBroadcasterID("BROADCASTER NAME");


//the still simple version:

//the library comes with a lot of documentation. You should have an easy time using different endpoints.
bool isSuccess;       //indicates if the request was successful - every endpoint outputs an isSuccess indication
int httpStatuscode;   //contains the http status code of the request - very helpful for debuging (0: error before making the request, -200: data is from cache)

GetChannelInformation channelInfo = GetChannelInformation.Go(credentials, "USER ID", out isSuccess, out httpStatuscode);

//Note on caching:
//Items which are requested from the API are stored in a cache (lifespan is 30 seconds) to protect you from Twitch's rate limits.
//You can skip the cache and request items directly from the server, just add a 'true' at the end of the method, like this:
GetChannelInformation channelInfo = GetChannelInformation.Go(credentials, "USER ID", out isSuccess, out httpStatuscode, true);
//This works with every endpoint which uses cache.
```

---

- **How to access Twitch's chat?**

**Note**: The credentials must be an User Access Token and contain following scopes: *chat:read, chat:edit* (extra scope to use moderation tools: *channel:moderate* - extra scopes to recieve and send whispers: *whispers:read, whispers:edit*)
```c#
using P90Ez.Twitch.IRC;

//Create a new instance of the IRC Controller:
var controller = new Controller(credentials, "CHANNEL NAME");

//The controller fires an event each time a message was sent to chat. Bin a function that should be executed when a message was sent:
controller.Events.OnChatMessage += onChatMessage;

//How this function should look like:
static void onChatMessage(object sender, irc_privsmg args)
{
    //your code - more samples below
}

//Start the controller: (to recieve messages)
controller.Start();


//an example on how your onChatMessage function could look like:
static void onChatMessage(object source, irc_privsmg args)
{
    if (args == null) return; //check if args are not null - they should not be null but you never know^^

    //example command
    if (args.Command[0] == "!test") //a simple test command
        args.SendMessage("Thats a great chat message!"); //sends a message to the chat without replying directly to the original message.


    //moderation example (will be easier to filter words in the future)
    if (args.Message.Contains("bad word"))
    {
        args.DeleteMessageFromChat();  //deletes this message from the chat
        args.TimeOutMessageSender(60); //timeouts the user who sent this message for 60 seconds
    }

    //example direct reply message
    if (args.IsBitMessage)
        args.Reply($"Thanks for {args.BitsAmount} Bits, @{args.Displayname}!"); //replys directly to this message
}
```

- **How to use EventSub?**
**NOTE**: The credentials must be an User Access Token and contain the scopes reqired by every event.
```
using P90Ez.Twitch.EventSub;

EventSubInstance EventSub = new EventSubInstance(credentials); //creating a new instance (you can provide a logger, but you don't have to)
EventSub.Add_Follows("BROADCASTER ID").Followed += OnFollow;   //Adding a event subscription and register the function 'OnFollow' to the EventHandler.
//The function 'OnFollow' will be invoked whenever a user follows the specified channel.
```

## Support / Bug Reports

For **support** or **bug reports** write me an **E-Mail <twitchlibrary@p90ez.com>** or contact me via **Discord: P90Ez#9675**

*Thank you for reading and happy coding!*

