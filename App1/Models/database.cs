using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.WinRT;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace App1.Models {
    public class db_item {
        //[PrimaryKey, AutoIncrement]
        public string Date { set; get; }
        public string Title { set; get; }
        public string Detail { set; get; }
        public string Image_url { set; get; }
    }

    public class database {
        string path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "MyList.sqlite");
        SQLiteConnection conn;

        public database () {
            conn = new SQLiteConnection(new SQLitePlatformWinRT(), path);
            conn.CreateTable<db_item>();
        }

        public ObservableCollection<Item> GetAllItems() {
            ObservableCollection<Item> Items = new ObservableCollection<Item>();
            List<db_item> db_item_list = conn.Query<db_item>("select * from db_item");
            foreach (var item in db_item_list) {
                Items.Add(new Item {
                    Title = item.Title,
                    Detail = item.Detail,
                    Date = DateTime.Parse(item.Date),
                    Image = new BitmapImage(new Uri(item.Image_url))
                });
            }
            return Items;
        }

        public async void Insert_Item(Item item) {
            try {
                conn.Insert(new db_item() {
                    Title = item.Title,
                    Detail = item.Detail,
                    Date = item.Date.ToString(),
                    Image_url = ((BitmapImage)item.Image).UriSource.ToString()
                });
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，插入数据失败");
                await dialog.ShowAsync();
            }
        }

        public async void Delete_Item(Item item) {
            try {
                conn.Execute("delete from db_item where Date = '" + item.Date.ToString() + "'");
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，删除数据失败");
                await dialog.ShowAsync();
            }
        }

        public async void Update_Item(Item old_item, Item new_item) {
            try {
                conn.Execute("delete from db_item where Date = '" + old_item.Date.ToString() + "'");
                conn.Insert(new db_item() {
                    Title = new_item.Title,
                    Detail = new_item.Detail,
                    Date = new_item.Date.ToString(),
                    Image_url = ((BitmapImage)new_item.Image).UriSource.ToString()
                });

            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，更新数据失败");
                await dialog.ShowAsync();
            }
        }

    }
}
