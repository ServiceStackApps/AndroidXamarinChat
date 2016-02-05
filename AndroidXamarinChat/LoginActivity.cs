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
            if (TryResolveAccount(out existingAccount))
            {
                StartChatActivity(client, existingAccount);
            }

            btnTwitter.Click += (sender, args) =>
            {
                //TODO try to resolve twitter access key if available?
                var ssAuth = new ServiceStackAuthenticator(
                        ChatClient.BaseUrl,
                        "twitter", jsonServiceClient =>
                        {
                            var userDetails = jsonServiceClient.Get(new GetUserDetails());
                            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                            prefs.Edit().PutString("TwitterUserName", userDetails.UserName).Commit();
                            return new Account(userDetails.UserName, jsonServiceClient.CookieContainer);
                        });
                ssAuth.Title = "Twitter / Authorize Chat";
                ssAuth.ServiceClientFactory = baseUrl => client;
                StartActivity(ssAuth.GetUI(this));
                ssAuth.Completed += (authSender, authArgs) =>
                {
                    if (authArgs.IsAuthenticated)
                    {
                        AccountStore.Create(this).Save(authArgs.Account, "Twitter");
                        StartChatActivity(client, authArgs.Account);
                    }
                };
            };

            btnAnon.Click += (sender, args) =>
            {
                StartChatActivity(client);
            };
        }

        private void StartChatActivity(JsonServiceClient client, Account existingAccount)
        {
            client.CookieContainer = existingAccount.Cookies;
            var intent = new Intent(this.BaseContext, typeof (MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            intent.PutExtra("SSCookie", client.CookieContainer.GetCookieHeader(new Uri(ChatClient.BaseUrl)));
            StartActivity(intent);
            Finish();
        }

        private void StartChatActivity(JsonServiceClient client)
        {
            client.ClearCookies();
            var intent = new Intent(this.BaseContext, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            intent.PutExtra("SSCookie", "");
            StartActivity(intent);
            Finish();
        }

        private bool TryResolveAccount(out Account account)
        {
            account = null;
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            string userName = prefs.GetString("TwitterUserName",null);
            var existingTwitterAccount = AccountStore.Create(this).FindAccountsForService("Twitter");
            var twitterAccount = existingTwitterAccount.FirstOrDefault(x => x.Username == userName);
            if (twitterAccount != null)
            {
                account = twitterAccount;
                return true;
            }
            return false;
        }
    }
}