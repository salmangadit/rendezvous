using Rendezvous.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Facebook;
using Newtonsoft.Json.Linq;
using Parse;
using Windows.UI.Xaml.Media.Imaging;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Rendezvous
{

    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : Rendezvous.Common.LayoutAwarePage
    {
        private readonly FacebookClient _fb = new FacebookClient();
        private string _userId;
        private string _accessToken;

        public ItemsPage()
        {
            this.InitializeComponent();
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
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = SampleDataSource.GetGroups("AllGroups");
            dynamic parameters = (dynamic)navigationParameter;
            this.DefaultViewModel["Items"] = sampleDataGroups;
            _userId = parameters.id;
            _accessToken = parameters.access_token;
            _fb.AccessToken = _accessToken;
            LoadFacebookData();
        }

        private void LoadFacebookData()
        {
            GetIdAndName();
            GetEvents();
            GetUserProfilePicture();
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
        private async void GetIdAndName()
        {
            
            try
            {
                dynamic result = await _fb.GetTaskAsync("me");
                dynamic name = result.name;

                // if dynamic you don't need to cast explicitly.
                fbName.Text = name;
            }
            catch (FacebookApiException ex)
            {

            }
        }

        private async void GetEvents()
        {
            try
            {
                dynamic result = await _fb.GetTaskAsync("me/events");

                List<dynamic> eventsData = result.data;

                foreach (dynamic eventData in eventsData)
                {
                    //push each event into parse
                    ParseObject eventObject = new ParseObject("Event");
                    eventObject["userId"] = _userId;
                    eventObject["eventId"] = eventData.id;
                    eventObject["name"] = eventData.name;
                    eventObject["startTime"] = eventData.start_time;
                    eventObject["endTime"] = eventData.endtime;
                    eventObject["location"] = eventData.location;
                    eventObject["rsvpStatus"] = eventData.rsvp_status;
                    await eventObject.SaveAsync();
                }

            }
            catch (FacebookApiException ex)
            {
                // handel error message
            }
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(SplitPage), groupId);
        }
    }
}
