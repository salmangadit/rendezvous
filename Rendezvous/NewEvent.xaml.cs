using Facebook;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Rendezvous
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class NewEvent : Rendezvous.Common.LayoutAwarePage
    {
        private readonly FacebookClient _fb = new FacebookClient();
        private string _userId;
        private string _accessToken;
        List<string> friends;
        List<string> ids;
        public NewEvent()
        {
            this.InitializeComponent();

            friends = new List<string>();
            ids = new List<string>();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            dynamic parameters = (dynamic)navigationParameter;
            _userId = parameters.id;
            _accessToken = parameters.access_token;
            _fb.AccessToken = _accessToken;
            LoadFacebookData();
        }

        private void LoadFacebookData()
        {
            GraphApiAsyncDynamicExample();
            GetUserProfilePicture();
            GetAllFriends();
        }

        private async void GetAllFriends()
        {
            try
            {
                // instead of casting to IDictionary<string,object> or IList<object>
                // you can also make use of the dynamic keyword.
                dynamic result = await _fb.GetTaskAsync("me/friends");

                foreach (dynamic friend in result.data)
                {
                    friends.Add(friend.name);
                    ids.Add(friend.id);
                }
            }
            catch (FacebookApiException ex)
            {
                int a;
                // handle error
            }
        }

        private async void GetUserProfilePicture()
        {
            try
            {
                dynamic result = await _fb.GetTaskAsync("me?fields=picture");
                string id = result.id;

                // available picture types: square (50x50), small (50xvariable height), large (about 200x variable height) (all size in pixels)
                // for more info visit http://developers.facebook.com/docs/reference/api
                string profilePictureUrl = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", _userId, "large", _fb.AccessToken);

                picProfile.Source = new BitmapImage(new Uri(profilePictureUrl));
            }
            catch (FacebookApiException ex)
            {
                // handel error message
            }
        }
        private async void GraphApiAsyncDynamicExample()
        {
            try
            {
                // instead of casting to IDictionary<string,object> or IList<object>
                // you can also make use of the dynamic keyword.
                dynamic result = await _fb.GetTaskAsync("me");

                // You can either access it this way, using the .
                dynamic id = result.id;
                dynamic name = result.name;

                // if dynamic you don't need to cast explicitly.
                fbName.Text = name;
            }
            catch (FacebookApiException ex)
            {
                int a;
                // handle error
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void comboBox1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string nameEvent = eventName.Text;
            string descEvent = description.Text;
            DateTime timeStart = (DateTime)startTime.Value;
            //string startTimeString = timeStart.ToString();
            //DateTime.TryParseExact(startTimeString, "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out timeStart);

            DateTime timeEnd = (DateTime)endTime.Value;
            //string endTimeString = timeEnd.ToString();
            //DateTime.TryParseExact(endTimeString, "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out timeEnd);

            string loc = location.Text;
            string eventType = cmbEventType.SelectedItem.ToString();

            CreateEvent(nameEvent, descEvent, timeStart.ToString("yyyy-MM-ddTHH:mm:ssZ"), timeEnd.ToString("yyyy-MM-ddTHH:mm:ssZ"), loc, eventType);
        }

        private async void CreateEvent(string name, string desc, string start, string end, string loc, string type)
        {
            List<string> invited = new List<string>();
            FacebookClient _fb = new FacebookClient();
            _fb.AccessToken = _accessToken;

            _fb.PostCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    var result = (IDictionary<string, object>)e.GetResultData();
                    var newPostId = (string)result["id"];
                    AddFriendsToEvent(newPostId, invited);
                }


            };

            foreach (string item in lstInvitedFriends.Items)
            {
                invited.Add(item);
            }

            var parameters = new Dictionary<string, object>
            {
             {"name", name},
             {"start_time", start},
             {"end_time", end},
             {"description", desc},
             {"location", loc},
             {"privacy_type", type.ToUpper()}
            };

            try
            {
                var postId = await _fb.PostTaskAsync(_userId + "/events", parameters);
            }
            catch (FacebookOAuthException ex)
            {
                //handle oauth exception
                int a;
            }
            catch (FacebookApiException ex)
            {
                //handle facebook exception
                int a;
            }

            int c;
        }

        private async void AddFriendsToEvent(string newPostId, List<string> invited)
        {
            FacebookClient _fb = new FacebookClient();
            _fb.AccessToken = _accessToken;
            
            _fb.PostCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    ShowSuccessMessage();
                }
            };
            
            
            List<string> invitedIds = new List<string>();
            string thechosenones = string.Empty;

            foreach (string selected in invited)
            {
                int index = friends.IndexOf(selected);

                thechosenones += ids[index];
                thechosenones += ",";
            }

            if (thechosenones.Length != 0)
            {
                thechosenones = thechosenones.Remove(thechosenones.Length - 1, 1);

                var parameters = new Dictionary<string, object>
            {
             {"users", thechosenones},
            };
                try
                {
                    var postId = await _fb.PostTaskAsync(newPostId + "/invited", parameters);
                }
                catch (FacebookOAuthException ex)
                {
                    //handle oauth exception
                    int a;
                }
                catch (FacebookApiException ex)
                {
                    //handle facebook exception
                    int a;
                }
            }
            else
                ShowSuccessMessage();
        }

        private async void ShowSuccessMessage()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var messageDialog = new MessageDialog("Event Created On Facebook and Users Invited!", "Event Created Successfully!");

                await messageDialog.ShowAsync();
            });
        }

        private void lstSuggestedFriends_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (lstSuggestedFriends.SelectedItem == null)
                return;

            lstInvitedFriends.Items.Add(lstSuggestedFriends.SelectedItem);
            lstSuggestedFriends.Items.Remove(lstSuggestedFriends.SelectedIndex);
        }

        private void friendName_KeyUp_1(object sender, KeyRoutedEventArgs e)
        {
            FilterList(friendName.Text);
        }

        private void FilterList(string filter)
        {
            List<string> results = friends.FindAll(delegate(string s)
            {
                return s.ToUpper().StartsWith(filter.ToUpper());
            });

            lstSuggestedFriends.ItemsSource = results;
            lstSuggestedFriends.DataContext = results;
        }
    }
}
