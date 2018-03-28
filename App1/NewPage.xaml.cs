﻿using System;
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
            this.Main_Grid.Background = this.Frame.Background;
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
                    Item item = new Item {
                        Title = TitleBox.Text,
                        Detail = DetailBox.Text,
                        Date = DatePicker.Date.DateTime,
                        Image = Image.Source
                    };
                    Info info = new Info {
                        item = item,
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
                    Item old_item = new Item {
                        Image = current_item.Image,
                        Title = current_item.Title,
                        Detail = current_item.Detail,
                        Date = current_item.Date
                    };
                    current_item.Image = Image.Source;
                    current_item.Title = TitleBox.Text;
                    current_item.Detail = DetailBox.Text;
                    current_item.Date = DatePicker.Date.DateTime;

                    Info info = new Info {
                        item = current_item,
                        old_item = old_item,
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
            Thickness temp = new Thickness();
            temp.Top = 50 - 50 * slider.Value / 200;
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

            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null) {
                using (IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
                    BitmapImage srcImage = new BitmapImage();
                    await srcImage.SetSourceAsync(stream);
                    this.Image.Source = srcImage;
                    save_image(file.Name);
                }
            }
        }

        private async void save_image(string desiredName) {
            StorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            StorageFolder folder = await applicationFolder.CreateFolderAsync("Picture", CreationCollisionOption.OpenIfExists);
            StorageFile saveFile = await folder.CreateFileAsync(desiredName, CreationCollisionOption.OpenIfExists);
            RenderTargetBitmap bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(this.Image);
            var pixelBuffer = await bitmap.GetPixelsAsync();
            using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite)) {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                    BitmapAlphaMode.Ignore,
                                    (uint)bitmap.PixelWidth,
                                    (uint)bitmap.PixelHeight,
                                    DisplayInformation.GetForCurrentView().LogicalDpi,
                                    DisplayInformation.GetForCurrentView().LogicalDpi,
                                    pixelBuffer.ToArray());
                await encoder.FlushAsync();
            }
            this.Image.Source = new BitmapImage(new Uri(folder.Path + "/" + desiredName));
        }
    }
}
