using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Chat;
using ServiceStack;
using Xamarin.Auth;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace AndroidXamarinChat
{
    [Activity(Label = "Chat (Xamarin) - Login", Icon = "@mipmap/ic_launcher", MainLauncher = true,
        Theme = "@style/ChatApp", ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.login);

            SupportToolbar mToolbar = FindViewById<SupportToolbar>(Resource.Id.loginToolbar);
            SetSupportActionBar(mToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);

            var btnTwitter = FindViewById<ImageButton>(Resource.Id.btnTwitter);
            var btnAnon = FindViewById<ImageButton>(Resource.Id.btnAnon);
            var client = new JsonServiceClient(ChatClient.BaseUrl);
            Account existingAccount;
            // If cookies saved from twitter login, automatically continue to chat activity.
            if (TryResolveAccount(client, out existingAccount))
            {
                client.CookieContainer = existingAccount.Cookies;
                var intent = new Intent(this.BaseContext, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                intent.PutExtra("SSCookie", client.CookieContainer.GetCookieHeader(new Uri(ChatClient.BaseUrl)));
                StartActivity(intent);
                Finish();
            }

            btnTwitter.Click += (sender, args) =>
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                var twitterAccess = prefs.GetString("TwitterAccessKey", null);
                
                if (twitterAccess == null)
                {
                    var ssAuth = new ServiceStackAuthenticator(
                        ChatClient.BaseUrl,
                        "twitter", jsonServiceClient =>
                        {
                            var userDetails = jsonServiceClient.Get(new GetUserDetails());
                            return new Account(userDetails.UserName, jsonServiceClient.CookieContainer);
                        });
                    ssAuth.Title = "Twitter / Authorize Chat";
                    ssAuth.ServiceClientFactory = baseUrl => client;
                    StartActivity(ssAuth.GetUI(this));
                    ssAuth.Completed += (authSender, authArgs) =>
                    {
                        if (authArgs.IsAuthenticated)
                        {
                            AccountStore.Create(this).Save(authArgs.Account,"Twitter");
                            StartActivity(typeof(MainActivity));
                        }
                    };
                }
            };

            btnAnon.Click += (sender, args) =>
            {
                StartActivity(typeof(MainActivity));
            };
        }

        private bool TryResolveAccount(ServiceClientBase client, out Account account)
        {
            account = null;
            var existingTwitterAccount = AccountStore.Create(this).FindAccountsForService("Twitter");
            var twitterAccount = existingTwitterAccount as Account[] ?? existingTwitterAccount.ToArray();
            if (twitterAccount.Any())
            {
                account = twitterAccount.First();
                return true;
            }
            return false;
        }
    }
}