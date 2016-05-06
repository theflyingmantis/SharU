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
using Windows.UI.Popups;
using Windows.Networking.Connectivity;
using Windows.UI.Notifications;
using Windows.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SharU
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public class BlobData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string imageUrl { get; set; }
        public string folder { get; set; }
    }

    public sealed partial class Push : Page
    {
        public string msgUserChoice { get; private set; }

        public class PushItems : Button
        {
            public localEventsTable handler { set; get; }

            public PushItems()
            {
                handler = new localEventsTable();
            }

            public PushItems(localEventsTable h)
            {
                handler = h;
            }
        }

        public class imagefile : Button
        {
            public StorageFile file { get; set; }
            public string handleName { get; set; }
            public localEventsTable handler { get; set; }
            public imagefile(StorageFile f, localEventsTable h, string hn)
            {
                file = f;
                handler = h;
                handleName = hn;
            }
        }


        public async void button_click(Object sender, RoutedEventArgs ee)
        {
            try
            {
                var item = sender as imagefile;
                if (String.IsNullOrWhiteSpace(item.handler.HandleName))
                {
                    throw new Exception("Empty handle.\nPlease enter a handle name.");
                }

                CloudStorageAccount account;
                CloudBlobClient blobClient;
                CloudBlobContainer container;
                account = new CloudStorageAccount(new StorageCredentials("shareu", "+jJaJga9Asoffv60OMsJu5g63uKdfBMBfntJsZ6rROnXH2QXJwDr2LtnamzbIfdTG7LIaTn6OaG/jafxet+Low=="), true);
                blobClient = account.CreateCloudBlobClient();
                string hn = item.handleName;
                container = blobClient.GetContainerReference(hn);

                Debug.WriteLine(item.handler.HandleName);

                msgUserChoice = "Yes";
                if (await container.ExistsAsync())
                {
                    var msg = new MessageDialog("The handle " + hn + " already exists.\n\nAre you sure you want to continue?", "Handle repeat..!");
                    //OK Button
                    UICommand okBtn = new UICommand("Yes");
                    okBtn.Invoked = OkBtnClick;
                    msg.Commands.Add(okBtn);

                    //Cancel Button
                    UICommand cancelBtn = new UICommand("No");
                    cancelBtn.Invoked = CancelBtnClick;
                    msg.Commands.Add(cancelBtn);

                    await msg.ShowAsync();
                }
                else
                    await container.CreateAsync();
                //await container.CreateIfNotExistsAsync();

                if (msgUserChoice == "Yes")
                {
                    StorageFile sf = item.file;

                    Debug.WriteLine("Name : " + sf.Path);

                    IRandomAccessStream str = await sf.OpenAsync(FileAccessMode.Read);

                    BlobContainerPermissions per = new BlobContainerPermissions();
                    per.PublicAccess = BlobContainerPublicAccessType.Container;
                    await container.SetPermissionsAsync(per);
                    CloudBlockBlob blob = container.GetBlockBlobReference(sf.Name);
                    await blob.UploadFromFileAsync(sf); 

                    BitmapImage bmp = new BitmapImage();
                    bmp.UriSource = blob.Uri;
                    BlobData imageData = new BlobData
                    {
                        imageUrl = blob.Uri.ToString(),
                        name = sf.Name,
                        folder = hn
                    };
                    await App.MobileService.GetTable<BlobData>().InsertAsync(imageData);
                    var m1 = new MessageDialog("Image Uploaded.").ShowAsync();
                    //list.Items.Remove(sf);
                }
            }
            catch (Exception ex)
            {
                var m2 = new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void OkBtnClick(IUICommand command)
        {
            msgUserChoice = "Yes";
        }

        private void CancelBtnClick(IUICommand command)
        {
            msgUserChoice = "No";
        }

        string runningEvent = "";
        
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
                    localEventsTable lastEvent = new localEventsTable();
                    foreach (localEventsTable evt in events)
                    {
                        if (evt.pushed)
                            continue;
                        PushItems item = new PushItems(evt);

                        item.Content = evt.HandleName;
                        item.Name = (evt.HandleName).Replace(" ", "");
                        item.Height = 40;
                        item.Width = 300;
                        item.Margin = new Thickness(0, 10, 0, 0);
                        item.Background = new SolidColorBrush(new Windows.UI.Color { A = 124, R = 255, G = 255, B = 255 });
                        item.Foreground = new SolidColorBrush(new Windows.UI.Color { A = 255, R = 255, G = 255, B = 255 });
                        item.HorizontalContentAlignment = HorizontalAlignment.Left;
                        //item.VerticalContentAlignment = VerticalAlignment.Center;
                        item.Click += new RoutedEventHandler(handleItem);

                        list.Items.Add(item);

                        lastEvent = evt;
                    }

                    string currDateTime = DateTime.Now.ToString();
                    string currDate = ReverseDateFormat(currDateTime.Remove(10));
                    string currTime = currDateTime.Remove(0, 11);

                    string lastEventDate = ReverseDateFormat(lastEvent.EndDate);
                    string lastEventTime = lastEvent.EndTime;

                    int dateCompRes = string.Compare(currDate, lastEventDate);
                    int timeCompRes = string.Compare(currTime, lastEventTime);

                    if (dateCompRes < 0 || (dateCompRes == 0 && timeCompRes < 0))
                        runningEvent = lastEvent.HandleName;
                    else
                        runningEvent = "";
                }
            }
            catch (NotSupportedException err)
            {
                Debug.WriteLine("Exception in Push : " + err.Message);
            }
            catch (SQLite.Net.SQLiteException err)
            {
                Debug.WriteLine("Exception in Push : " + err.Message);
            }
            catch (NullReferenceException err)
            {
                Debug.WriteLine("Exception in Push : " + err.Message);
            }
        }

        public Push()
        {
            this.InitializeComponent();

            getData();
        }

        private async void handleItem(object sender, RoutedEventArgs e)
        {

            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            // connectionProfile can be null (e.g. airplane mode)
            if (connectionProfile != null && connectionProfile.IsWlanConnectionProfile)
            {
                var item = sender as PushItems;
                
                StorageFolder appFolder = KnownFolders.PicturesLibrary;//()()()()()()(()()()()change
                    
                // Get the files in the current folder.
                IReadOnlyList<StorageFile> filesInFolder = await appFolder.GetFilesAsync();

                //****************MAKING A NEW FOLDER*******************
                StorageFolder localFolder = KnownFolders.PicturesLibrary;

                // Create a new subfolder in the current folder.
                // Raise an exception if the folder already exists.
                string desiredName = item.handler.HandleName.ToString();
                StorageFolder newFolder = await localFolder.CreateFolderAsync(desiredName, CreationCollisionOption.ReplaceExisting);
                //*******************************************************

                int flag = 0;
                foreach (StorageFile file in filesInFolder)
                {
                    string a, dd = "";
                    a = file.DateCreated.ToString();

                    //Debug.WriteLine("Name : " + file.Name);

                    dd = a.Remove(11);

                    string time = "";

                    time = a.Remove(0, 11);
                    time = time.Remove(6);

                    time += "00";
                        
                    // String.Format("{0:MM/dd/yyyy}", file.DateCreated);
                    //string extension = ".jpg";

                    string ddRev = ReverseDateFormat(dd);
                    string sdRev = ReverseDateFormat(item.handler.StartDate);
                    string edRev = ReverseDateFormat(item.handler.EndDate);

                    int sddd = string.Compare(sdRev, ddRev);
                    int dded = string.Compare(ddRev, edRev);

                    /*
                    Debug.WriteLine("IMG =  Date : " + dd + " ddRev : " + ddRev + " Time : " + time);
                    Debug.WriteLine("ST =  Date : " + item.handler.StartDate + " sdRev : " + sdRev + " stTime : " + item.handler.StartTime);
                    Debug.WriteLine("E =  Date : " + item.handler.EndDate + " edRev : " + edRev + " eTime : " + item.handler.EndTime);

                    Debug.WriteLine("sddd : " + sddd);
                    Debug.WriteLine("dded : " + dded);
                    Debug.WriteLine("stt : " + string.Compare(item.handler.StartTime, time));
                    Debug.WriteLine("tet : " + string.Compare(time, item.handler.EndTime));
                    */

                    if ((sddd < 0 || (sddd == 0 && string.Compare(item.handler.StartTime, time) <= 0)) && (dded < 0 || (dded == 0 && string.Compare(time, item.handler.EndTime) <= 0)))
                    {
                        Debug.WriteLine("Correct");
                        await file.MoveAsync(newFolder);
                    }
                }

                IReadOnlyList<StorageFile> files2 = await newFolder.GetFilesAsync();

                list.Items.Clear();
                    
                foreach (StorageFile file2 in files2)
                {
                    flag = flag + 1;

                    Image img = new Image();
                    img.Height = 100;
                    img.Width = 100;
                    using (IRandomAccessStream fileStream = 
                    await file2.OpenAsync(FileAccessMode.Read))
                    {
                        // Set the image source to the selected bitmap.
                        BitmapImage bitmapImage =
                            new BitmapImage();

                        bitmapImage.SetSource(fileStream);
                        img.Source = bitmapImage;
                    }

                    imagefile button = new imagefile(file2, item.handler, desiredName);
                    button.Content = "Add";
                    button.Click += new RoutedEventHandler(button_click);

                    list.Items.Add(img);
                    list.Items.Add(button);
                }

                if (item.handler.HandleName != runningEvent)
                {
                    item.handler.pushed = true;
                    await App.MobileService.GetTable<localEventsTable>().UpdateAsync(item.handler);
                }
                
                //this.Frame.Navigate(typeof(Push));
            }
            else
            {
                string toast = "<toast>"
                + "<visual>"
                + "<binding template = \"ToastGeneric\" >"
                + "<image placement=\"appLogoOverride\" src=\"Assets\\noWifi.png\" />"
                + "<text> No Internet connection available!!! </text>"
                + "<image placement=\"inline\" src=\"Assets\\noWifi.png\" />"
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

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
