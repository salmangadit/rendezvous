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
using System.Text;
using System.Globalization;

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

        private List<dynamic> eventsData = new List<dynamic>();

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

            //get events and populate first
            dynamic parameters = (dynamic)navigationParameter;
            _userId = parameters.id;
            _accessToken = parameters.access_token;
            _fb.AccessToken = _accessToken;

            LoadFacebookData();

        }

        private void LoadFacebookData()
        {
            GetEventsData();
            GetIdAndName();
            GetUserProfilePicture();
        }

        private async void GetEventsData()
        {
            try
            {
                dynamic result = await _fb.GetTaskAsync("me/events");

                eventsData = result.data;

                List<SampleDataGroup> allEventsFromFb = new List<SampleDataGroup>();

                foreach (dynamic eventData in eventsData)
                {
                    DateTime startDate = DateTime.Parse((string)eventData.start_time);

                    DateTime endDate;
                    string endDateString;

                    try
                    {
                        endDate = DateTime.Parse((string)eventData.end_time);
                        endDateString = endDate.ToString();

                    }
                    catch
                    {
                        endDateString = "";
                    }

                    string eventPicture = "Assets/LightGray.png";
                    string attendeePicture = "Assets/LightGray.png";

                    dynamic eventResult = await _fb.GetTaskAsync("/" + eventData.id);

                    try
                    {
                        // available picture types: square (50x50), small (50xvariable height), large (about 200x variable height) (all size in pixels)
                        // for more info visit http://developers.facebook.com/docs/reference/api
                        eventPicture = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", eventData.id,
                            "large", _fb.AccessToken);
                    }
                    catch (FacebookApiException ex)
                    {
                        // handel error message
                    }

                    SampleDataGroup eventObject = new SampleDataGroup(eventData.id, eventData.name,
                        startDate.ToString(), endDateString, eventPicture, eventResult.description);

                    try
                    {
                        dynamic attendeeResult = await _fb.GetTaskAsync(String.Format("/{0}/invited", eventData.id));

                        foreach (dynamic user in attendeeResult.data)
                        {
                            string attendeeName = (string)user.name;
                            string attendeeFbId = (string)user.id;
                            string rsvpStatus = (string)user.rsvp_status;

                            switch (rsvpStatus)
                            {
                                case "attending":
                                    rsvpStatus = "Attending";
                                    break;
                                case "not_replied":
                                    rsvpStatus = "Not Replied";
                                    break;
                                case "not_attending":
                                    rsvpStatus = "Not Attending";
                                    break;
                                default:
                                    rsvpStatus = "Error in response";
                                    break;
                            }

                            attendeePicture = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", attendeeFbId, "large", _fb.AccessToken);

                            eventObject.Items.Add(new SampleDataItem(attendeeFbId, attendeeName, rsvpStatus, attendeePicture, eventObject));
                        }
                    }
                    catch (FacebookApiException ex)
                    {
                        // handel error message
                    }

                    allEventsFromFb.Add(eventObject);
                }

                SampleDataSource.SetEvents(allEventsFromFb);

                var allEvents = SampleDataSource.GetEvents("AllEvents"); //populate with all events

                this.DefaultViewModel["Items"] = allEvents;
            }
            catch (FacebookApiException ex)
            {
                // handel error message
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

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param
        /// 
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(SplitPage), groupId);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            dynamic parameters = new ExpandoObject();
            parameters.id = _userId;
            parameters.access_token = _accessToken;

            this.Frame.Navigate(typeof(NewEvent), (Object)parameters);
        }
    }
}
