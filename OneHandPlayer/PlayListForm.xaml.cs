using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// PlayListForm.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlayListForm : Window
    {

        MainWindow MainWindow;

        public PlayListForm(List<String[]> videoList , MainWindow mainWindow)
        {
            InitializeComponent();

            this.Height = mainWindow.Height;





            this.MainWindow = mainWindow;// 값 전달을 위한 메인화면 가져오기

            List <MovieData> videoData = new List<MovieData>();

            for (int i = 0; i < videoList.Count; i++)
            {
                MovieData vd = new MovieData();
                vd.ImageData = LoadImage(videoList[1-i][0] + "/" + videoList[1-i][1], videoList[1-i][1]);
                vd.Title = videoList[1-i][1];
                vd.mediaSource = videoList[1 - i][0] + "/" + videoList[1 - i][1];

                videoData.Add(vd);
                
            }

            TvBox.ItemsSource = videoData;



            /*this.TvBox.ItemsSource = new MovieData[]{


                
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
                new MovieData { Title = "text", ImageData = LoadImage("15.jpg") }

            };*/
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

        public class MovieData
        {
            private string _Title;
            public string Title
            {
                get { return this._Title; }
                set { this._Title = value; }
            }

            private BitmapImage _ImageData;
            public BitmapImage ImageData
            {
                get { return this._ImageData; }
                set { this._ImageData = value; }
            }

            private String _Time;

            public String Time
            {
                get { return this._Time; }
                set { this._Time = value; }
            }

            private String _mediaSource;

            public String mediaSource
            {
                get { return this._mediaSource; }
                set { this._mediaSource = value; }
            }
        }
        private BitmapImage LoadImage(string file, String filename)
        {
            
            //MemoryStream stream = new MemoryStream();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(file, @"..\..\thumbnail\"+ filename + ".jpeg", 37);//동영상 경로, 출력 경로, 시간(초)
            
            return new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\thumbnail\" + filename + ".jpeg"));
            
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        

        private void TvBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(TvBox.SelectedItems.Count == 1)
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;

                while ((dep != null) && !(dep is ListViewItem))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep == null) return;

                MovieData movieData = new MovieData();
                movieData = (MovieData)TvBox.ItemContainerGenerator.ItemFromContainer(dep);

                MainWindow.Media_Play(movieData.mediaSource);

            }
        }
    }
}
