using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using App1.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using Windows.Storage;
using Windows.UI.Popups;

namespace App1.ViewModels {

    public class ListItemViewModels {
        public ObservableCollection<ListItem> Allitems { set; get; }
        public ListItem select_item { set; get; }
        private SQLiteConnection conn;

        private ListItemViewModels() {
            string path = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "MyList.sqlite");
            conn = new SQLiteConnection(new SQLitePlatformWinRT(), path);
            conn.CreateTable<ListItem>();

            Allitems = new ObservableCollection<ListItem>();
            List<ListItem> db_item_list = conn.Query<ListItem>("select * from ListItem");
            foreach (var item in db_item_list) {
                Allitems.Add(item);
            }
            select_item = null;
        }

        private static ListItemViewModels instance;
        public static ListItemViewModels get_instance() {
            if (instance == null)
                instance = new ListItemViewModels();
            return instance;
        }

        public async void AddListItem(ListItem item = null) {
            try {
                if(item != null) {
                    conn.Insert(item);
                    Allitems.Add(item);
                } else {
                    conn.Insert(select_item);
                    Allitems.Add(select_item);
                    select_item = null;
                }
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，插入数据失败");
                await dialog.ShowAsync();
            }
        }

        public async void DeleteListItem(ListItem item = null) {
            try {
                if (item != null) {
                    conn.Delete(item);
                    Allitems.Remove(item);
                } else {
                    conn.Delete(select_item);
                    Allitems.Remove(select_item);
                    select_item = null;
                }
                
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，删除数据失败");
                await dialog.ShowAsync();
            }
        }

        public async void UpdateListItem(ListItem item = null) {
            try {
                if (item != null) {
                    conn.Update(item);
                }   else {
                    conn.Update(select_item);
                    select_item = null;
                }
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，更新数据失败");
                await dialog.ShowAsync();
            }
        }

        public ListItem GetListItemByID(int id) {
            foreach(var item in Allitems) {
                if (item.ID == id)
                    return item;
            }
            return null;
        }

    }
}
