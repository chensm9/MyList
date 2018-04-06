using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using App1.Models;
using Windows.Storage;
using App1.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        private ListItemViewModels listItemViewModels;

        public NewPage() {
            this.InitializeComponent();
            listItemViewModels = ListItemViewModels.get_instance();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {

            this.Main_Grid.Background = this.Frame.Background;
            base.OnNavigatedTo(e);
            if (e.Parameter != null && (string)e.Parameter == "update" && listItemViewModels.select_item != null) {
                DeleteButton.Visibility = Visibility.Visible;
                CreateButton.Content = "Update";
                TitleBox.Text = listItemViewModels.select_item.Title;
                DetailBox.Text = listItemViewModels.select_item.Detail;
                DatePicker.Date = listItemViewModels.select_item.Date;
                Image.Source = listItemViewModels.select_item.Image;
            }

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
                    this.CreateButton.Content = composite["Button_type"] as string;
                    if (composite.ContainsKey("Item_Id")) {
                        listItemViewModels.select_item = 
                            listItemViewModels.GetListItemByID((int)composite["Item_Id"]);
                        DeleteButton.Visibility = Visibility.Visible;
                        CreateButton.Content = "Update";
                    }
                    ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
                }
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
                    ["Button_type"] = this.CreateButton.Content.ToString()
                };
                
                //保存当前select_item的ID以恢复
                if (listItemViewModels.select_item != null)
                    composite["Item_Id"] = listItemViewModels.select_item.ID;
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
                    listItemViewModels.select_item = new ListItem {
                        ID = DateTime.Now.GetHashCode(),
                        Title = TitleBox.Text,
                        Detail = DetailBox.Text,
                        Date = DatePicker.Date.DateTime,
                        Image = Image.Source
                    };
                    listItemViewModels.AddListItem();
                    Frame.Navigate(typeof(MainPage));
                }
            } else {
                if (message == "")
                    message = "修改成功";

                MessageDialog dialog = new MessageDialog(message);
                await dialog.ShowAsync();

                if (message == "修改成功") {
                    listItemViewModels.select_item.Image = Image.Source;
                    listItemViewModels.select_item.Title = TitleBox.Text;
                    listItemViewModels.select_item.Detail = DetailBox.Text;
                    listItemViewModels.select_item.Date = DatePicker.Date.DateTime;

                    listItemViewModels.UpdateListItem();
                    Frame.Navigate(typeof(MainPage));
                }
            }
        }

        private async void Delete(object sender, RoutedEventArgs e) {
            listItemViewModels.DeleteListItem();
            MessageDialog dialog = new MessageDialog("删除成功");
            await dialog.ShowAsync();
            Frame.Navigate(typeof(MainPage));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            if ((string)CreateButton.Content == "Create") {
                TitleBox.Text = "";
                DetailBox.Text = "";
                DatePicker.Date = DateTime.Now;
                Image.Source = new BitmapImage(new Uri(BaseUri, "Assets/gakki2.jpg"));
            } else {
                TitleBox.Text = listItemViewModels.select_item.Title;
                DetailBox.Text = listItemViewModels.select_item.Detail;
                DatePicker.Date = listItemViewModels.select_item.Date;
                Image.Source = listItemViewModels.select_item.Image;
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
                //string new_name = DateTime.Now.Ticks + file.Name;
                string new_name = file.Name;
                await file.CopyAsync(folder, new_name, NameCollisionOption.ReplaceExisting);
                this.Image.Source = new BitmapImage(new Uri(folder.Path + "/" + new_name));
            }
        }

    }
}
