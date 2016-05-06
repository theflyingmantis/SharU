using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SharU
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RunningEvent : Page
    {
        localEventsTable lastEvent;

        private static string getMonth(string month)
        {
            switch(month)
            {
                case "01": return "January";
                case "02": return "February";
                case "03": return "March";
                case "04": return "April";
                case "05": return "May";
                case "06": return "June";
                case "07": return "July";
                case "08": return "August";
                case "09": return "September";
                case "10": return "October";
                case "11": return "November";
                case "12": return "December";
                default: return "Invalid";
            }
        }

        private static void getDMY(string date, out string day, out string month, out string year)
        {
            day = "" + date[0] + date[1];
            month = "" + date[3] + date[4];
            year = "" + date[6] + date[7] + date[8] + date[9];
        }
        
        private static void getHM(string time, out string hour, out string minute)
        {
            hour = "" + time[0] + time[1];
            minute = "" + time[3] + time[4];
        }

        private async void getData()
        {
            try
            {
                List<localEventsTable> events = await App.MobileService.GetTable<localEventsTable>().ToListAsync();
                events = events.OrderBy(o => o.CreatedAt).ToList();
                if (events.Count != 0)
                {
                    lastEvent = events.Last();

                    string startDate = lastEvent.StartDate;
                    string startTime = lastEvent.StartTime;

                    string stD, stM, stY, stH, stMin;

                    getDMY(startDate, out stD, out stM, out stY);
                    getHM(startTime, out stH, out stMin);

                    string endDate = lastEvent.EndDate;
                    string endTime = lastEvent.EndTime;

                    string eD, eM, eY, eH, eMin;

                    getDMY(endDate, out eD, out eM, out eY);
                    getHM(endTime, out eH, out eMin);

                    handleName.Text = lastEvent.HandleName;
                    startDay.Text = stD;
                    startMonth.Text = getMonth(stM);
                    startYear.Text = stY;
                    startHour.Text = stH;
                    startMinute.Text = stMin;

                    endDay.Text = eD;
                    endMonth.Text = getMonth(eM);
                    endYear.Text = eY;
                    endHour.Text = eH;
                    endMinute.Text = eMin;
                }
            }
            catch (NotSupportedException err)
            {
                Debug.WriteLine("Exception in RunningEvent : " + err.Message);
            }
        }

        public RunningEvent()
        {
            this.InitializeComponent();

            getData();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void terminateBtn_Click(object sender, RoutedEventArgs e)
        {
            string currDateTime = DateTime.Now.ToString();
            string currDate = currDateTime.Remove(10);
            string currTime = currDateTime.Remove(0, 11);

            lastEvent.EndDate = currDate;
            lastEvent.EndTime = currTime;

            await App.MobileService.GetTable<localEventsTable>().UpdateAsync(lastEvent);

            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
