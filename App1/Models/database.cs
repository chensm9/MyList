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
        [PrimaryKey]
        public int ID { set; get; }

        public string Date { set; get; }
        public string Title { set; get; }
        public string Detail { set; get; }
        public string Image_url { set; get; }

        public db_item(Item item) {
            ID = item.ID;
            Title = item.Title;
            Detail = item.Detail;
            Date = item.Date.ToString();
            Image_url = ((BitmapImage)item.Image).UriSource.ToString();
        }
        public db_item() { }
    }

    public class database {
        string path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "my_list.sqlite");
        SQLiteConnection conn;

        private database () {
            conn = new SQLiteConnection(new SQLitePlatformWinRT(), path);
            conn.CreateTable<db_item>();
        }

        private static database instance;
        public static database get_instance() {
            if (instance == null)
                instance = new database();
            return instance;
        }

        public ObservableCollection<Item> GetAllItems() {
            ObservableCollection<Item> Items = new ObservableCollection<Item>();
            List<db_item> db_item_list = conn.Query<db_item>("select * from db_item");
            foreach (var item in db_item_list) {
                Items.Add(new Item(item));
            }
            return Items;
        }

        public async void Insert_Item(Item item) {
            try {
                conn.Insert(new db_item(item));
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，插入数据失败");
                await dialog.ShowAsync();
            }
        }

        public async void Delete_Item(Item item) {
            try {
                //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                //StorageFile imageFile = await storageFolder.GetFileAsync(((BitmapImage)item.Image).UriSource);
                //await imageFile.DeleteAsync();
                conn.Delete(new db_item(item));
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，删除数据失败");
                await dialog.ShowAsync();
            }
        }

        public async void Update_Item(Item item) {
            try {
                conn.Update(new db_item(item));
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，更新数据失败");
                await dialog.ShowAsync();
            }
        }

    }
}
