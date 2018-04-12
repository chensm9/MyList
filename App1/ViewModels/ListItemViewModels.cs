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

        public void Search(string text) {
            string sql_like = '%' + text + '%';
            List<ListItem> list = conn.Query<ListItem>("select * from ListItem " +
                                                        "where title like ? " +
                                                        "or detail like ? " +
                                                        "or date like ?", sql_like, sql_like, sql_like);
            Allitems.Clear();
            foreach(var item in list) {
                Allitems.Add(item);
            }

        }

        public ListItem GetListItemByID(int id) {
            foreach(var item in Allitems) {
                if (item.ID == id)
                    return item;
            }
            return null;
        }

        private void UpdateTile() {
            System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(() => {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueue(true);
                updater.Clear();

                List<ListItem> item_list = conn.Query<ListItem>("select * from ListItem");
                int itemCount = 0;

                foreach (var item in item_list) {
                    XmlDocument tileXml = TileService.CreateTiles(item);
                    updater.Update(new TileNotification(tileXml));
                    itemCount++;
                    if (itemCount >= 5) break;
                }
                TileService.SetBadgeCountOnTile(item_list.Count);
            });
            task.Start();
        }

    }
}
