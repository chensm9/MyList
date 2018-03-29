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
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using App1.Models;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Item> Items;
        private Item current_item;
        private database DB;
        
        public MainPage() {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            this.InitializeComponent();

            DB = database.Get_db_instance();
            Items = DB.GetAllItems();
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            if (this.ScrollViewer.Visibility == Visibility.Visible) {
                CreateButton.Content = "Create";
                clear();
                return;
            }
            this.Frame.Background = Main_Grid.Background;
            this.Frame.Navigate(typeof(NewPage));
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
                        ID = DateTime.Now.GetHashCode(),
                        Title = TitleBox.Text,
                        Detail = DetailBox.Text,
                        Date = DatePicker.Date.DateTime,
                        Image = Image.Source
                    };
                    clear();
                    Items.Add(item);
                    DB.Insert_Item(item);
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
                    DB.Update_Item(current_item);

                    CreateButton.Content = "Create";
                    clear();
                }
            }
        }

        private void Edit(object sender, ItemClickEventArgs e) {
            current_item = e.ClickedItem as Item;
            if (Window.Current.Bounds.Width >= 800) {
                CreateButton.Content = "Update";
                clear();

            } else {
                Info info = new Info {
                    item = current_item,
                    option = "update"
                };
                this.Frame.Background = this.Main_Grid.Background;
                Frame.Navigate(typeof(NewPage), info);
            }
        }

        private void Delete(object sender, RoutedEventArgs e) {
            Items.Remove(current_item);
            DB.Delete_Item(current_item);
            CreateButton.Content = "Create";
            clear();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            clear();
        }

        private void clear() {
            if ((string)this.CreateButton.Content == "Create") {
                TitleBox.Text = "";
                DetailBox.Text = "";
                DatePicker.Date = DateTime.Now;
                Image.Source = new BitmapImage(new Uri(BaseUri, "Assets/gakki2.jpg"));
                DeleteButton.Visibility = Visibility.Collapsed;
            } else {
                TitleBox.Text = current_item.Title;
                DetailBox.Text = current_item.Detail;
                DatePicker.Date = current_item.Date;
                Image.Source = current_item.Image;
                DeleteButton.Visibility = Visibility.Visible;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (e.NavigationMode == NavigationMode.New) {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MainPage"))
                    ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
            } else {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MainPage")) {
                    var composite = ApplicationData.Current.LocalSettings.Values["MainPage"] as ApplicationDataCompositeValue;
                    TitleBox.Text = composite["Title"] as string;
                    DetailBox.Text = composite["Detail"] as string;
                    DatePicker.Date = DateTime.Parse((string)composite["Date"]);
                    Image.Source = new BitmapImage(new Uri((string)composite["Image_uri"]));
                    ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
                }
            }

            base.OnNavigatedTo(e);
            if (e.Parameter.ToString() != "") {
                Info info = (Info)e.Parameter;
                if (info.option == "add") {
                    Items.Add(info.item);
                    DB.Insert_Item(info.item);
                } else if (info.option == "delete") {
                    DB.Delete_Item(info.item);
                    Items.Remove(info.item);
                } else if (info.option == "update") {
                    DB.Update_Item(info.item);
                }
            }
            //隐藏回退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            bool suspending = ((App)App.Current).issuspend;
            if (suspending) {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["Title"] = TitleBox.Text;
                composite["Detail"] = DetailBox.Text;
                composite["Date"] = DatePicker.Date.ToString();
                composite["Image_uri"] = ((BitmapImage)Image.Source).UriSource.ToString();
                ApplicationData.Current.LocalSettings.Values["MainPage"] = composite;
            }
            base.OnNavigatedFrom(e);
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

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            this.Image.Width = 80 + this.slider.Value / 100 * (130 - 80);
            this.Image.Height = 80 + this.slider.Value / 100 * (130 - 80);
            Thickness temp = new Thickness();
            temp.Top = 50 - 50 * slider.Value / 200;
            this.Image.Margin = temp;
        }

        private void Background_Change(object sender, RoutedEventArgs e) {
            var s = (MenuFlyoutItem)sender;
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(BaseUri, "Assets/"+s.Text+".jpg"));
            Main_Grid.Background = imageBrush;
            if (s.Text == "sky" || s.Text == "nepal" || s.Text == "raindrops")
                this.RequestedTheme = ElementTheme.Light;
            else
                this.RequestedTheme = ElementTheme.Dark;
            this.Frame.Background = this.Main_Grid.Background;
        }

    }

}
