using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace playlistTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            this.TvBox.ItemsSource = new MovieData[]{
                
                new MovieData{Title="Movie 1", ImageData=LoadImage("1.jpg"),Time = "00:00"},
                new MovieData{Title="Movie 2", ImageData=LoadImage("2.jpg")},
                new MovieData{Title="Movie 3", ImageData=LoadImage("3.jpg")},
                new MovieData{Title="Movie 4", ImageData=LoadImage("4.jpg")},
                new MovieData{Title="Movie 5", ImageData=LoadImage("5.jpg")},
                new MovieData{Title="Movie 6", ImageData=LoadImage("6.jpg")},
                new MovieData{Title="Movie 7", ImageData=LoadImage("7.jpg")},
                new MovieData{Title="Movie 9", ImageData=LoadImage("8.jpg")},
                new MovieData{Title="Movie 10", ImageData=LoadImage("9.jpg")},
                new MovieData{Title="Movie 11", ImageData=LoadImage("10.jpg")},
                new MovieData{Title="Movie 12", ImageData=LoadImage("11.jpg")},
                new MovieData{Title="Movie 13", ImageData=LoadImage("12.jpg")},
                new MovieData{Title="Movie 14", ImageData=LoadImage("13.jpg")},
                new MovieData{Title="Movie 15", ImageData=LoadImage("14.jpg")},
                new MovieData{Title="text", ImageData=LoadImage("15.jpg")}

            };
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public class MovieData {
            private string _Title;
            public string Title {
                get { return this._Title; }
                set { this._Title = value; }
            }

            private BitmapImage _ImageData;
            public BitmapImage ImageData {
                get { return this._ImageData; }
                set { this._ImageData = value; }
            }

            private String _Time;

            public String Time
            {
                get { return this._Time; }
                set { this._Time = value; }
            }

        }
        private BitmapImage LoadImage(string filename) {
            return new BitmapImage(new Uri("pack://application:,,,/" + "1.jpg"));
        }
    }
}
