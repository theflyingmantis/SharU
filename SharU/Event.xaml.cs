using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SharU
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Event : Page
    {
        public Event()
        {
            this.InitializeComponent();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        public static string ReverseDateFormat(string s)
        {
            string newString = "" + s[6] + s[7] + s[8] + s[9] + s[5] + s[3] + s[4] + s[2] + s[0] + s[1];
            return newString;
        }

        private async void startBtn_Click(object sender, RoutedEventArgs e)
        {
            string currDateTime = DateTime.Now.ToString();
            string currDate = ReverseDateFormat(currDateTime.Remove(10));
            string currTime = currDateTime.Remove(0, 11);
            currTime = currTime.Remove(currTime.Length - 2);
            currTime += "00";

            string hn = handleName.Text;

            if(hn.Length == 0)
            {
                await new MessageDialog("Event Name cannot be empty.").ShowAsync();
                return;
            }



            string sd = startDate.Date.ToString().Remove(10);
            string st = startTime.Time.ToString();

            string sdRev = ReverseDateFormat(sd);

            int compRes = string.Compare(sdRev, currDate);

            Debug.WriteLine("curr:" + currTime);
            Debug.WriteLine("st:" + st);

            if (compRes < 0 || (compRes == 0 && string.Compare(st, currTime) < 0))
            {
                await new MessageDialog("Start Date must be present or future").ShowAsync();
                return;
            }


            string ed = endDate.Date.ToString().Remove(10);
            string et = endTime.Time.ToString();

            string edRev = ReverseDateFormat(ed);

            compRes = string.Compare(sdRev, edRev);
            if (compRes > 0 || (compRes == 0 && string.Compare(st, et) >= 0))
            {
                await new MessageDialog("End Date must be greater than Start Date.").ShowAsync();
                return;
            }

            localEventsTable evt = new localEventsTable
            {
                HandleName = hn,
                StartDate = sd,
                StartTime = st,
                EndDate = ed,
                EndTime = et,
                CreatedAt = DateTime.Now.Date,
                pushed = false
            };

            await App.MobileService.GetTable<localEventsTable>().InsertAsync(evt);

            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
