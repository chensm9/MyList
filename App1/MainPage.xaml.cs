using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.Storage;
using App1.ViewModels;
using App1.Models;
using Windows.Storage.Streams;
using System.Threading.Tasks;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ListItemViewModels listItemViewModels;
        
        public MainPage() {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            this.InitializeComponent();
            
            listItemViewModels = ListItemViewModels.get_instance();
            this.Init();
        }

        private async void Init() {
            StorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            StorageFolder folder = await applicationFolder.CreateFolderAsync("Picture", CreationCollisionOption.OpenIfExists);
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(BaseUri, "Assets/gakki2.jpg"));
            string new_name = file.Name;
            await file.CopyAsync(folder, new_name, NameCollisionOption.ReplaceExisting);
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            if (this.ScrollViewer.Visibility == Visibility.Visible) {
                CreateButton.Content = "Create";
                clear();
                return;
            }

            this.Frame.Background = this.Main_Grid.Background;
            this.Frame.RequestedTheme = this.RequestedTheme;
            this.Frame.Navigate(typeof(NewPage), "add");
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
                    ListItem item = new ListItem {
                        ID = DateTime.Now.GetHashCode(),
                        Title = TitleBox.Text,
                        Detail = DetailBox.Text,
                        Date = DatePicker.Date.DateTime,
                        Image = Image.Source,
                        Complete = false
                    };
                    listItemViewModels.AddListItem(item);
                    clear();
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

                    CreateButton.Content = "Create";
                    clear();
                }
            }
        }

        private void Edit(object sender, ItemClickEventArgs e) {
            listItemViewModels.select_item = e.ClickedItem as ListItem;
            if (Window.Current.Bounds.Width >= 800) {
                CreateButton.Content = "Update";
                clear();

            } else {
                this.Frame.RequestedTheme = this.RequestedTheme;
                this.Frame.Background = this.Main_Grid.Background;
                Frame.Navigate(typeof(NewPage), "update");
            }
        }

        private void Delete(object sender, RoutedEventArgs e) {
            listItemViewModels.DeleteListItem();
            CreateButton.Content = "Create";
            clear();
        }

        private async void Share(object sender, RoutedEventArgs e) {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = listItemViewModels.select_item.title;
            emailMessage.Body = "due time: " + listItemViewModels.select_item.date + "\n" +
                                "description: " + listItemViewModels.select_item.detail + "\n\n";

            string[] parts = listItemViewModels.select_item.image_uri.Split('/');
            string fileName = parts[parts.Length - 1];

            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                            "Picture", 
                            CreationCollisionOption.OpenIfExists);
            var attachmentFile = await folder.GetFileAsync(fileName);
            var stream = RandomAccessStreamReference.CreateFromFile(attachmentFile);

            var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                attachmentFile.Name,
                stream);
            emailMessage.Attachments.Add(attachment);
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
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
                ShareButton.Visibility = Visibility.Collapsed;
            } else {
                TitleBox.Text = listItemViewModels.select_item.Title;
                DetailBox.Text = listItemViewModels.select_item.Detail;
                DatePicker.Date = listItemViewModels.select_item.Date;
                Image.Source = listItemViewModels.select_item.Image;
                DeleteButton.Visibility = Visibility.Visible;
                ShareButton.Visibility = Visibility.Visible;
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
                    if (composite.ContainsKey("Item_Id")) {
                        listItemViewModels.select_item =
                            listItemViewModels.GetListItemByID((int)composite["Item_Id"]);
                        DeleteButton.Visibility = Visibility.Visible;
                        CreateButton.Content = "Update";
                    }
                    ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
                }
            }

            //listItemViewModels.Search(SearchBox.Text);
            //隐藏回退按钮
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            bool suspending = ((App)App.Current).issuspend;
            if (suspending) {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue {
                    ["Title"] = TitleBox.Text,
                    ["Detail"] = DetailBox.Text,
                    ["Date"] = DatePicker.Date.ToString(),
                    ["Image_uri"] = ((BitmapImage)Image.Source).UriSource.ToString()
                };
                //保存当前select_item的ID以恢复
                if (listItemViewModels.select_item != null)
                    composite["Item_Id"] = listItemViewModels.select_item.ID;
                ApplicationData.Current.LocalSettings.Values["MainPage"] = composite;
                ((App)App.Current).issuspend = false;
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

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
            this.Image.Width = 80 + this.slider.Value / 100 * (130 - 80);
            this.Image.Height = 80 + this.slider.Value / 100 * (130 - 80);
            Thickness temp = new Thickness();
            temp.Top = 50 - 50 * slider.Value / 200;
            this.Image.Margin = temp;
        }

        private void Background_Change(object sender, RoutedEventArgs e) {
            var s = sender as MenuFlyoutItem;
            ImageBrush imageBrush = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(BaseUri, "Assets/" + s.Text + ".jpg"))
            };
            this.Main_Grid.Background = imageBrush;
            if (s.Text == "sky" || s.Text == "nepal" || s.Text == "raindrops")
                this.RequestedTheme = ElementTheme.Light;
            else
                this.RequestedTheme = ElementTheme.Dark;
            this.Frame.Background = this.Main_Grid.Background;
        }

        private async void Search(object sender, RoutedEventArgs e) {
            string message = "";
            foreach (var item in listItemViewModels.Allitems) {
                message += "title: " + item.title + "\n" + 
                           "detail: " + item.detail + "\n" +
                           "due date: "+ item.date + "\n\n";
            }
            if(message == "") {
                message = "搜索不到相关条目";
            }
            MessageDialog dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }

        private void SearchAndChange(object sender, TextChangedEventArgs e) {
            listItemViewModels.Search(SearchBox.Text);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            CheckBox checkBox = sender as CheckBox;
            ListItem item = checkBox.DataContext as ListItem;
            Task task = new Task(() => {
                listItemViewModels.UpdateListItem(item);
            });
            task.Start();
        }
    }

}
