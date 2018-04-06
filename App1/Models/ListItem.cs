using App1.ViewModels;
using SQLite.Net.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace App1.Models {
    public class ListItem: INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public ListItem() {
            ID = DateTime.Now.GetHashCode();
            complete = false;
            Line_Visibility = Visibility.Collapsed;
        }

        [PrimaryKey]
        public int id { get; set; }
        public string image_uri { get; set; }
        public string detail { get; set; }
        public string date { get; set; }
        public string title { get; set; }
        public Boolean? complete { get; set; }

        private Visibility line_visibility;

        [Ignore]
        public int ID {
            get { return id; }
            set {
                id = value;
            }
        }

        [Ignore]
        public DateTime Date {
            get { return DateTime.Parse(date); }
            set {
                date = value.ToString();
            }
        }

        [Ignore]
        public string Detail {
            get { return detail; }
            set {
                detail = value;
            }
        }

        [Ignore]
        public string Title {
            get { return title; }
            set {
                title = value;
                NotifyPropertyChanged("Title");
            }
        }

        
        private ImageSource image;

        [Ignore]
        public ImageSource Image {
            get {
                if (image != null)
                    return image;
                else {
                    image = new BitmapImage(new Uri(image_uri));
                    return image;
                }
            }
            set {
                image_uri = ((BitmapImage)value).UriSource.ToString();
                image = value;
                NotifyPropertyChanged("Image");
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Ignore]
        public Visibility Line_Visibility {
            get { return this.line_visibility; }
            set {
                line_visibility = value;
                NotifyPropertyChanged("Line_Visibility");
            }
        }

        [Ignore]
        public Boolean? Complete {
            set {
                complete = value;
                if (complete == true)
                    Line_Visibility = Visibility.Visible;
                else
                    Line_Visibility = Visibility.Collapsed;

                NotifyPropertyChanged("Complete");
                ListItemViewModels.get_instance().UpdateListItem(this);
            }
            get {
                return complete;
            }
        }
    }
}
