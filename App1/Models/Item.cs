using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace App1.Models {
    public class Item: INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public Item() {
            ID = DateTime.Now.GetHashCode();
            Line_Visibility = Visibility.Collapsed;
        }

        public Item(db_item item) {
            ID = item.ID;
            Title = item.Title;
            Detail = item.Detail;
            Date = DateTime.Parse(item.Date);
            Image = new BitmapImage(new Uri(item.Image_url));
            Line_Visibility = Visibility.Collapsed;
        }

        public int ID { get; set; }

        private string title;
        public string Title {
            get { return title; }
            set {
                title = value;
                NotifyPropertyChanged();
            }
        }

        private ImageSource image;
        public ImageSource Image {
            get { return image; }
            set {
                image = value;
                NotifyPropertyChanged();
            }
        }

        public string Detail { get; set; }
        public DateTime Date { get; set; }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Visibility line_visibility { get; set; }
        public Visibility Line_Visibility {
            get { return this.line_visibility; }
            set {
                line_visibility = value;
                NotifyPropertyChanged();
            }
        }

        public bool If_ckecked { get; set; }
        public void Check_box() {
            if (If_ckecked == false) {
                If_ckecked = true;
                Line_Visibility = Visibility.Visible;
            } else {
                If_ckecked = false;
                Line_Visibility = Visibility.Collapsed;
            }
        }
    }

    public struct Info {
        public Item item;
        public string option;
    }
}
