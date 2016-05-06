using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SharU
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Pull : Page
    {
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

        string runningEvent = "";
        bool dialogResponse = false;

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

        public Pull()
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

                var promtMsg = new MessageDialog("Are you sure ?", "Download");

                UICommand yesBtn = new UICommand("Yes");
                yesBtn.Invoked = yesBtn_Click;

                promtMsg.Commands.Add(yesBtn);

                UICommand noBtn = new UICommand("No");
                noBtn.Invoked = noBtn_Click;

                promtMsg.Commands.Add(noBtn);

                await promtMsg.ShowAsync();

                if (dialogResponse)
                {
                    try
                    {
                        string hn = item.handler.HandleName.ToString();
                        /*string hnNew = "";
                        foreach(char c in hn)
                        {
                            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                                hnNew += c;
                        }*/

                        if (String.IsNullOrWhiteSpace(hn))
                        {
                            throw new Exception("Empty handle.\nPlease enter a handle name.");
                        }

                        CloudStorageAccount account;
                        CloudBlobClient blobClient;
                        CloudBlobContainer container;
                        account = new CloudStorageAccount(new StorageCredentials("shareu", "+jJaJga9Asoffv60OMsJu5g63uKdfBMBfntJsZ6rROnXH2QXJwDr2LtnamzbIfdTG7LIaTn6OaG/jafxet+Low=="), true);
                        blobClient = account.CreateCloudBlobClient();

                        container = blobClient.GetContainerReference("hack");
                        if (await container.ExistsAsync())
                        {
                            List<BlobData> allImages = await App.MobileService.GetTable<BlobData>().Where(BlobDataItem => BlobDataItem.folder == item.handler.HandleName.ToString()).ToListAsync();
                            //ImageList.ItemsSource = allImages;
                            //int downloadedImgCount = 0;

                            string toast = "<toast>"
                                + "<visual>"
                                + "<binding template = \"ToastGeneric\" >"
                                + "<image placement=\"appLogoOverride\" src=\"Assets\\StoreLogo.png\" />"
                                + "<text> Downloading... </text>"
                                + "<text> Your memories are being downloaded. </text>"
                                + "</binding>"
                                + "</visual>"
                                + "</toast>";

                            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
                            toastDOM.LoadXml(toast);
                            ToastNotification toastNotification = new ToastNotification(toastDOM);
                            var toastNotifier = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
                            toastNotifier.Show(toastNotification);

                            StorageFolder localfolder = Windows.Storage.KnownFolders.PicturesLibrary;

                            string desiredname = item.handler.HandleName;

                            StorageFolder newfolder = await localfolder.CreateFolderAsync(desiredname, CreationCollisionOption.ReplaceExisting);
                            
                            foreach (var img in allImages)
                            {
                                //var downloadProgress = new MessageDialog(String.Format("Downloaded {0}/{1} images.", downloadedImgCount, allImages.Count)).ShowAsync();                        
                                try
                                {
                                    BitmapImage bmp1 = new BitmapImage();
                                    bmp1.UriSource = new Uri(img.imageUrl);
                                    RandomAccessStreamReference rasr = RandomAccessStreamReference.CreateFromUri(bmp1.UriSource);
                                    var streamWithContent = await rasr.OpenReadAsync();
                                    byte[] buffer = new byte[streamWithContent.Size];
                                    await streamWithContent.ReadAsync(buffer.AsBuffer(), (uint)streamWithContent.Size, InputStreamOptions.None);
                                    StorageFile sf = await ApplicationData.Current.LocalFolder.CreateFileAsync(img.name, CreationCollisionOption.GenerateUniqueName);
                                    using (IRandomAccessStream filestream = await sf.OpenAsync(FileAccessMode.ReadWrite))
                                    {
                                        filestream.Seek(0);
                                        using (IOutputStream outputstream = filestream.GetOutputStreamAt(0))
                                        {
                                            using (DataWriter dataWriter = new DataWriter(outputstream))
                                            {
                                                dataWriter.WriteBytes(buffer);
                                                await dataWriter.StoreAsync();
                                                dataWriter.DetachStream();
                                            }
                                            await outputstream.FlushAsync();
                                        }
                                        await filestream.FlushAsync();
                                    }
                                    
                                    await sf.MoveAsync(newfolder, img.name, NameCollisionOption.ReplaceExisting);
                                }
                                catch (Exception ex)
                                {
                                    await new MessageDialog(ex.Message).ShowAsync();
                                }


                            }

                            toast = "<toast>"
                                + "<visual>"
                                + "<binding template = \"ToastGeneric\" >"
                                + "<image placement=\"appLogoOverride\" src=\"Assets\\StoreLogo.png\" />"
                                + "<text> Download Complete!!! </text>"
                                + "<text> All your memories have been saved.</text>"
                                + "</binding>"
                                + "</visual>"
                                + "</toast>";

                            toastDOM.LoadXml(toast);
                            toastNotifier.Show(new ToastNotification(toastDOM));
                            //await new MessageDialog("Download Complete").ShowAsync();
                        }
                        else {
                            var msg = new MessageDialog("The handle " + item.handler.HandleName.ToString() + " does not exists.\n\nPlease Check it again.", "Handle error..!").ShowAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        var m2 = new MessageDialog(ex.Message).ShowAsync();
                    }
                }
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

        private void yesBtn_Click(IUICommand command)
        {
            dialogResponse = true;
        }

        private void noBtn_Click(IUICommand command)
        {
            dialogResponse = false;
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }

}
