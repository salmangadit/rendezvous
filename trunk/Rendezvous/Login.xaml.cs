﻿using Parse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Facebook;
using System.Dynamic;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Rendezvous
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        private const string AppId = "162446510564975";
        private const string ExtendedPermissions = "user_about_me,read_stream,publish_stream";
        private readonly FacebookClient _fb = new FacebookClient();

        public Login()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void btnLogin_Click_1(object sender, RoutedEventArgs e)
        {
            
            //browser.LoadCompleted += browser_LoadCompleted;

            //var loginUrl = GetFacebookLoginUrl(AppId, ExtendedPermissions);
            //browser.Navigate(loginUrl);
            // Make your browser control visible
            browser.Visibility = Visibility.Visible;
            try
            {
                ParseUser user = await ParseFacebookUtils.LogInAsync(
                    browser, new[] { "user_likes", "email" });
                // The user logged in with Facebook!
                
            }
            catch
            {
                // User cancelled the Facebook login or did not fully authorize.
            }
            // Hide your browser control
            browser.Visibility = Visibility.Collapsed;
            LoginSucceeded(ParseFacebookUtils.AccessToken);
        }

        private async void LoginSucceeded(string p)
        {
            dynamic parameters = new ExpandoObject();
            parameters.access_token = p;
            parameters.fields = "id";

            dynamic result = await _fb.GetTaskAsync("me", parameters);
            parameters = new ExpandoObject();
            parameters.id = result.id;
            parameters.access_token = p;

            Frame.Navigate(typeof(ItemsPage), (Object)parameters);
        }


        //private void browser_Loaded_1(object sender, RoutedEventArgs e)
        //{
            
        //}

        //private Uri GetFacebookLoginUrl(string appId, string extendedPermissions)
        //{
        //    dynamic parameters = new ExpandoObject();
        //    parameters.client_id = appId;
        //    parameters.redirect_uri = "https://www.facebook.com/connect/login_success.html";
        //    parameters.response_type = "token";
        //    parameters.display = "popup";

        //    // add the 'scope' parameter only if we have extendedPermissions.
        //    if (!string.IsNullOrWhiteSpace(extendedPermissions))
        //    {
        //        // A comma-delimited list of permissions
        //        parameters.scope = extendedPermissions;
        //    }

        //    return _fb.GetLoginUrl(parameters);
        //}

        //private void browser_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        //{
        //    FacebookOAuthResult oauthResult;
        //    if (!_fb.TryParseOAuthCallbackUrl(e.Uri, out oauthResult))
        //    {
        //        return;
        //    }

        //    if (oauthResult.IsSuccess)
        //    {
        //        var accessToken = oauthResult.AccessToken;
        //        LoginSucceded(accessToken);
        //    }
        //    else
        //    {
        //        // user cancelled
        //    }
        //}

    }
}
