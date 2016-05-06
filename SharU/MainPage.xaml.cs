using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Diagnostics;
using Windows.Networking.Connectivity;
using Windows.UI.Notifications;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SharU
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public sealed partial class MainPage : Page
    {
        bool isAnyEventRunning = false;

        public static string ReverseDateFormat(string s)
        {
            string newString = "" + s[6] + s[7] + s[8] + s[9] + s[5] + s[3] + s[4] + s[2] + s[0] + s[1];
            return newString;
        }

        private async void getData()
        {
            try
            {
                List<localEventsTable> events = await App.MobileService.GetTable<localEventsTable>().ToListAsync();

                events = events.OrderBy(o => o.CreatedAt).ToList();
                if (events.Count != 0)
                {
                    string currDateTime = DateTime.Now.ToString();
                    string currDate = ReverseDateFormat(currDateTime.Remove(10));
                    string currTime = currDateTime.Remove(0, 11);

                    localEventsTable lastEvent = events.Last();

                    string lastEventDate = ReverseDateFormat(lastEvent.EndDate);
                    string lastEventTime = lastEvent.EndTime;

                    int dateCompRes = string.Compare(currDate, lastEventDate);
                    int timeCompRes = string.Compare(currTime, lastEventTime);

                    if (dateCompRes < 0 || (dateCompRes == 0 && timeCompRes < 0))
                    {
                        isAnyEventRunning = true;
                        msg.Text = "Event \"" + lastEvent.HandleName + "\" is running...\n" + "Click to view detail";
                        newEventBtn.Content = lastEvent.HandleName;
                    }
                    else
                    {
                        isAnyEventRunning = false;
                        msg.Text = "";
                        newEventBtn.Content = "New Event";
                    }

                    /*
                    int count = 0;
                    foreach(localEventsTable evt in events)
                        if (!evt.pushed)
                            count++;

                    if(count != 0)
                    {
                        var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                        // connectionProfile can be null (e.g. airplane mode)
                        if (connectionProfile != null && connectionProfile.IsWlanConnectionProfile)
                        {
                            string toast = "<toast>"
                            + "<visual>"
                            + "<binding template = \"ToastGeneric\" >"
                            + "<image placement=\"appLogoOverride\" src=\"Assets\\wifi.png\" />"
                            + "<text> Please push the files... </text>"
                            + "<image placement=\"inline\" src=\"Assets\\push.png\" />"
                            + "</binding>"
                            + "</visual>"
                            + "</toast>";

                            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
                            toastDOM.LoadXml(toast);

                            ToastNotification toastNotification = new ToastNotification(toastDOM);

                            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                            toastNotifier.Show(toastNotification);
                        }
                    }
                    */

                    //this.Frame.Navigate(typeof(RunningEvent));

                    //var latestEvent = db.ExecuteScalar<localEventsTable>("SELECT * FROM localEventsTable ORDER BY Id DESC LIMIT 1");
                    //System.Diagnostics.Debug.WriteLine("Last : " + latestEvent.HandleName);
                }
            }
            catch (NotSupportedException err)
            {
                Debug.WriteLine("Exception in MainPage : " + err.Message);
            }
            catch (SQLite.Net.SQLiteException err)
            {
                Debug.WriteLine("Exception in MainPage : " + err.Message);
            }
            catch (NullReferenceException err)
            {
                Debug.WriteLine("Exception in MainPage : " + err.Message);
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            /*msg.Margin = new Thickness(this.Width / 10, this.Height / 7, 0, 0);
            newEventBtn.Width = this.Width - this.Width / 5;
            newEventBtn.Margin = new Thickness(this.Width / 10, this.Height / 10, this.Width / 10, 0);
            pushBtn.Width = this.Width - this.Width / 5;
            pushBtn.Margin = new Thickness(this.Width / 10, this.Height / 10, this.Width / 10, 0);*/
            //pullBtn.Width = this.Width - this.Width / 5;
            //pullBtn.Margin = new Thickness(this.Width / 10, this.Height / 10, this.Width / 10, 0);

            getData();
        }

        private void newEventBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isAnyEventRunning)
                this.Frame.Navigate(typeof(RunningEvent));
            else
                this.Frame.Navigate(typeof(Event));
        }

        private void pushBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Push));
        }

        private void pullBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Pull));
        }
    }
}
