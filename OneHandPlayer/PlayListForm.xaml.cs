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
        List<String[]> videoList = new List<string[]>();//받아온 영상목록
        List<MovieData> videoData;

        String mainFolder = ""; // 메인 폴더 경로
        public String _mainFolder
        {
            get { return this.mainFolder; }

        }

        String nowFolder = "";
        String pastFolder = "";// 이전 폴더 경로
        public PlayListForm(List<String[]> videoList , MainWindow mainWindow, String mainFolder)
        {
            InitializeComponent();

            this.Height = mainWindow.Height;

            this.MainWindow = mainWindow;// 값 전달을 위한 메인화면 가져오기

            this.videoList = videoList;//영상목록 받아오기

            this.mainFolder = mainFolder;//메인폴더 경로
            this.nowFolder = mainFolder;
            this.pastFolder = mainFolder;

            DirectoryInfo di = new DirectoryInfo(mainFolder+ "/thumbnail");
            if(di.Exists == false)//썸네일 폴더 생성
            {
                di.Create();

                di.Attributes = FileAttributes.Hidden;

                
            }


            getList();//영상 리스트 만들기


           
        }

        private void getList() //영상리스트 메소드
        {
            


            videoData = new List<MovieData>(); // 영상 리스트

            MovieData md = new MovieData(); // 메인폴더로 가는 버튼
            md.ImageData = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Folder.png"));
            md.Title = "홈";
            md.mediaSource = mainFolder;
            md.type = true;
            videoData.Add(md);



            MovieData pd = new MovieData(); // 이전폴더로 가는 버튼 
            pd.ImageData = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Folder.png"));
            pd.Title = "이전";       
            pd.type = true;
            videoData.Add(pd);

            for (int i = 0; i < videoList.Count; i++)
            {
                MovieData vd = new MovieData();
                if (videoList[i][2] == "")//확장자가 폴더면
                {
                    vd.ImageData = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Folder.png"));
                    vd.Title = videoList[i][1];
                    vd.mediaSource = videoList[i][0];
                    vd.type = true;
                    videoData.Add(vd);
                }

                else if(videoList[i][2].Equals(".mp4") || videoList[i][2].Equals(".m4p") || videoList[i][2].Equals(".avi") || videoList[i][2].Equals(".wmv"))
                {
                    vd.ImageData = LoadImage(videoList[i][0] + "/" + videoList[i][1], videoList[i][1]);
                    vd.Title = videoList[i][1];
                    vd.mediaSource = videoList[i][0] + "/" + videoList[i][1];
                    vd.type = false;
                    videoData.Add(vd);
                }
                
                
                
            }
            
            pd.mediaSource = pastFolder;//이전폴더 경로
            

            TvBox.ItemsSource = videoData;

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

            private bool _type;

            public bool type
            {
                get { return this._type; }
                set { this._type = value; }
            }
        }
        private BitmapImage LoadImage(string file, String filename)//영상경로, 영상 이름
        {
            FileInfo fileInfo = new FileInfo(mainFolder + "/thumbnail/" + filename + ".jpeg");//스크린샷이 있으면 스크린샷 생성 안함
            if (fileInfo.Exists)
            {
                return new BitmapImage(new Uri(mainFolder + "/thumbnail/" + filename + ".jpeg"));
            }


            //MemoryStream stream = new MemoryStream();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(file, mainFolder + "/thumbnail/" + filename + ".jpeg", 37);//동영상 경로, 출력 경로, 시간(초)
            

            return new BitmapImage(new Uri(mainFolder + "/thumbnail/" + filename + ".jpeg"));
            
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

                if (movieData.type)//현재폴더랑 같으면
                {
                    if (nowFolder.Equals(movieData.mediaSource))
                    {
                        MessageBox.Show("현재폴더입니다.");
                        return;
                    }

                    nowFolder = movieData.mediaSource;
                    List<String[]> videoList = new List<string[]>();//받아온 영상목록

                    DirectoryInfo di = new DirectoryInfo(movieData.mediaSource); // 해당 폴더 정보를 가져옵니다. 
                    
                    videoList.Clear();
                    
                    DirectoryInfo[] dirs = di.GetDirectories("*.*", SearchOption.AllDirectories);//폴더 목록 검색
                    foreach (DirectoryInfo d in dirs)
                    {
                        if (d.Name == "thumbnail")
                        {//썸네일 폴더 안나오게
                            continue;
                        }

                        String[] dir = new string[] { d.FullName, d.Name, d.Extension };
                        videoList.Add(dir);
                    }

                    foreach (FileInfo File in di.GetFiles()) // 선택 폴더의 파일 목록을 스캔합니다. 
                    {

                        String[] video = new string[] { File.DirectoryName, File.Name, File.Extension };
                        videoList.Add(video);
                    }
                    this.videoList = videoList;                   
                    

                    getList();

                    pastFolder = movieData.mediaSource;
                }

                else
                {
                    MainWindow.Media_Play(movieData.mediaSource);
                }
                

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
