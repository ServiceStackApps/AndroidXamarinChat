# Android Xamarin Chat
An Android (Xamarin) Chat client for the ServiceStack Chat application!

## Overview
Android (Xamarin) Chat shows integration with ServiceStack server from a Xamarin application utilizing the following ServiceStack features.
- ServiceStack Authentication Providers (Twitter)
    - Using Xamarin.Auth to provide a more native experience
    - Xamarin.Auth generic wrapper for ServiceStack auth providers 
    - Storing ServiceStack account details using Xamarin `AccountStore`.
- Server Side Events feature (Chat messages and commands)
    - Command receivers
        - Announce reciever using Android notifications
        - CSS receiver intepreting messages to change background image and colors
        - Play youtube receiver to natively hand off to Android devices registered application
    - New `sseClient.SubscribeToChannels` to dynamically listen to new channels without restarting the connection. (4.0.53+)
    - Custom `IResolver`  
    - Getting active event subscribers
- Add ServiceStack Reference (Chat DTOs)

<img src="https://github.com/ServiceStack/Assets/blob/master/img/apps/Chat/androidxamchat-demo1.gif" width="250">

## Authentication using Xamarin.Auth
Xamarin.Auth is a component you can use when developing Xamarin clients that authenticate with common OAuth proviers like Twitter, Facebook, etc. This component can also be used with ServiceStack OAuth providers by [creating a wrapper for the `WebAuthenticator`](https://github.com/ServiceStackApps/AndroidXamarinChat/blob/d5d033c49d9b5f73f8679339e05f9dab21ad120f/AndroidXamarinChat/ServiceStackAuthenticator.cs#L10-L64).

```` CSharp
var ssAuth = new ServiceStackAuthenticator(
    MainActivity.BaseUrl,
    "twitter", jsonServiceClient =>
    {
        var userDetails = jsonServiceClient.Get(new GetUserDetails());
        return new Account(userDetails.UserName, jsonServiceClient.CookieContainer);
    });
````
> [More info about this wrapper at the TechStacksAuth repository](https://github.com/ServiceStackApps/TechStacksAuth#using-xamarinauth-with-servicestack)

## Command receivers

![](https://github.com/ServiceStack/Assets/blob/master/img/apps/Chat/androidxamchat-demo_dual.gif)


