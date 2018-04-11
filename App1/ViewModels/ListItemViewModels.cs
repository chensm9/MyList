using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using App1.Models;
using App1.Services;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
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

            List<ListItem> db_item_list = conn.Query<ListItem>("select * from ListItem");
            Allitems = new ObservableCollection<ListItem>(db_item_list);
            UpdateTile();
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
                UpdateTile();
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
                UpdateTile();
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
                UpdateTile();
            } catch {
                MessageDialog dialog = new MessageDialog("数据库异常，更新数据失败");
                await dialog.ShowAsync();
            }
        }

        public List<ListItem> Search(string text) {
            Regex reg = new Regex(text);
            SQLiteCommand cmd = conn.CreateCommand("select * from ListItem");
            List<ListItem> list = cmd.ExecuteQuery<ListItem>();
            List<ListItem> list2 = new List<ListItem>();

            Allitems.Clear();
            foreach (var item in list) {
                if (reg.IsMatch(item.title) || reg.IsMatch(item.detail) || reg.IsMatch(item.date)) {
                    Allitems.Add(item);
                }
            }
            return list2;
         }

        public ListItem GetListItemByID(int id) {
            foreach(var item in Allitems) {
                if (item.ID == id)
                    return item;
            }
            return null;
        }

        private void UpdateTile() {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            // Keep track of the number feed items that get tile notifications.
            int itemCount = 0;

            // Create a tile notification for each feed item.
            foreach (var item in Allitems) {
                XmlDocument tileXml = TileService.CreateTiles(item);
                // Create a new tile notification.
                updater.Update(new TileNotification(tileXml));

                // Don't create more than 5 notifications.
                if (itemCount++ > 5) break;
            }
            TileService.SetBadgeCountOnTile(Allitems.Count);
        }

    }
}
