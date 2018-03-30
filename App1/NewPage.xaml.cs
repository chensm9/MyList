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
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using App1.Models;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        private Item current_item;

        public NewPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (e.NavigationMode == NavigationMode.New) {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NewPage"))
                    ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
            } else {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NewPage")) {
                    var composite = ApplicationData.Current.LocalSettings.Values["NewPage"] as ApplicationDataCompositeValue;
                    TitleBox.Text = composite["Title"] as string;
                    DetailBox.Text = composite["Detail"] as string;
                    DatePicker.Date = DateTime.Parse((string)composite["Date"]);
                    Image.Source = new BitmapImage(new Uri((string)composite["Image_uri"]));
                    //this.CreateButton.Content = composite["button_type"] as string;
                    //this.Frame.Background = new ImageBrush {
                    //    ImageSource = new BitmapImage(new Uri((string)composite["background_uri"]))
                    //};
                    ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
                }
            }

            this.Background = this.Frame.Background;
            base.OnNavigatedTo(e);
            if (e.Parameter != null) {
                current_item = ((Info)e.Parameter).item;
                TitleBox.Text = current_item.Title;
                DetailBox.Text = current_item.Detail;
                DatePicker.Date = current_item.Date;
                Image.Source = current_item.Image;
                DeleteButton.Visibility = Visibility.Visible;
                CreateButton.Content = "Update";
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            bool suspending = ((App)App.Current).issuspend;
            if (suspending) {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue {
                    ["Title"] = TitleBox.Text,
                    ["Detail"] = DetailBox.Text,
                    ["Date"] = DatePicker.Date.ToString(),
                    ["Image_uri"] = ((BitmapImage)Image.Source).UriSource.ToString(),
                    //["button_type"] = this.CreateButton.Content.ToString(),
                    //["background_uri"] = ((BitmapImage)((ImageBrush)this.Background).ImageSource).UriSource.ToString()
                };
                ApplicationData.Current.LocalSettings.Values["NewPage"] = composite;
                ((App)App.Current).issuspend = false;
            }
            base.OnNavigatedFrom(e);
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e) {
            string message = "";
            if (this.TitleBox.Text == "")
                message += "Title不能为空";
            if (this.DetailBox.Text == "")
                message += "\nDetail不能为空";
            if (this.DatePicker.Date.Year < DateTime.Now.Year ||
               (this.DatePicker.Date.Year == DateTime.Now.Year && this.DatePicker.Date.DayOfYear < DateTime.Now.DayOfYear))
                message += "\n任务创建日期不能小于当前日期";

            if ((String)this.CreateButton.Content == "Create") {
                if (message == "")
                    message = "任务创建成功";
                MessageDialog dialog = new MessageDialog(message);
                await dialog.ShowAsync();

                if (message.Equals("任务创建成功")) {
                    current_item = new Item {
                        ID = DateTime.Now.GetHashCode(),
                        Title = TitleBox.Text,
                        Detail = DetailBox.Text,
                        Date = DatePicker.Date.DateTime,
                        Image = Image.Source
                    };
                    Info info = new Info {
                        item = current_item,
                        option = "add"
                    };
                    Frame.Navigate(typeof(MainPage), info);
                }
            } else {
                if (message == "")
                    message = "修改成功";

                MessageDialog dialog = new MessageDialog(message);
                await dialog.ShowAsync();

                if (message == "修改成功") {
                    current_item.Image = Image.Source;
                    current_item.Title = TitleBox.Text;
                    current_item.Detail = DetailBox.Text;
                    current_item.Date = DatePicker.Date.DateTime;

                    Info info = new Info {
                        item = current_item,
                        option = "update"
                    };
                    Frame.Navigate(typeof(MainPage), info);
                }
            }
        }

        private async void Delete(object sender, RoutedEventArgs e) {
            Info info = new Info {
                item = current_item,
                option = "delete"
            };
            MessageDialog dialog = new MessageDialog("删除成功");
            await dialog.ShowAsync();
            Frame.Navigate(typeof(MainPage), info);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            if ((string)CreateButton.Content == "Create") {
                TitleBox.Text = "";
                DetailBox.Text = "";
                DatePicker.Date = DateTime.Now;
                Image.Source = new BitmapImage(new Uri(BaseUri, "Assets/gakki0.jpg"));
            } else {
                TitleBox.Text = current_item.Title;
                DetailBox.Text = current_item.Detail;
                DatePicker.Date = current_item.Date;
                Image.Source = current_item.Image;
            }
        }

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            this.Image.Width = 80 + this.slider.Value / 100 * (130-80);
            this.Image.Height = 80 + this.slider.Value / 100 * (130-80);
            Thickness temp = new Thickness {
                Top = 50 - 50 * slider.Value / 200
            };
            this.Image.Margin = temp;
        }

        private async void Select_Click(object sender, RoutedEventArgs e) {
            FileOpenPicker openPicker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail
            };
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null) {
                StorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
                StorageFolder folder = await applicationFolder.CreateFolderAsync("Picture", CreationCollisionOption.OpenIfExists);
                string new_name = DateTime.Now.ToString() + file.Name;
                await file.CopyAsync(folder, new_name, NameCollisionOption.ReplaceExisting);
                this.Image.Source = new BitmapImage(new Uri(folder.Path + "/" + new_name));
            }
        }

    }
}
