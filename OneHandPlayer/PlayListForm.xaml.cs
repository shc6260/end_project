using NReco.VideoInfo;
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
using System.Windows.Threading;

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
        Stack<String> folder_parent_List = new Stack<string>(); //상위폴더 스택

        String mainFolder = ""; // 메인 폴더 경로
        public String _mainFolder
        {
            get { return this.mainFolder; }

        }

        String nowFolder = "";
        public PlayListForm(List<String[]> videoList , MainWindow mainWindow, String mainFolder)
        {
            InitializeComponent();

            this.Height = mainWindow.Height;

            this.MainWindow = mainWindow;// 값 전달을 위한 메인화면 가져오기

            this.videoList = videoList;//영상목록 받아오기

            this.mainFolder = mainFolder;//메인폴더 경로
            this.nowFolder = mainFolder;

            DirectoryInfo di = new DirectoryInfo(mainFolder + "/thumbnail");
            if (di.Exists == true)//썸네일 폴더 생성
            {
                try
                {
                    di.Delete(true);
                }
                catch
                {

                }
            }


            di.Create();


            getList();//영상 리스트 만들기

            this.ShowInTaskbar = false;

        }

        private void getList() //영상리스트 메소드
        {

            pathLbl.Content = nowFolder;

            videoData = new List<MovieData>(); // 영상 리스트

            MovieData md = new MovieData(); // 메인폴더로 가는 버튼
            md.ImageData = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Folder.png"));

            md.Title = "홈";
            md.mediaSource = mainFolder;
            md.type = true;
            videoData.Add(md);


            MovieData pd = new MovieData(); // 이전폴더로 가는 버튼 
            pd.ImageData = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Folder.png"));
            pd.Title = "상위 폴더";       
            pd.type = true;
            videoData.Add(pd);

            for (int i = 0; i < videoList.Count; i++)
            {
                MovieData vd = new MovieData();
                if (videoList[i][2] == "")//확장자가 폴더면
                {
                    vd.ImageData = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Folder.png"));

                    String title = ""; // 제목 10글자마다 줄바꿈
                    for (int j = 1; j <= videoList[i][1].Length; j++)
                    {

                        title += videoList[i][1][j - 1];
                        if (j % 11 == 0)
                        {
                            title += "\r\n";
                        }
                    }
                    vd.Title = title;
                    vd.mediaSource = videoList[i][0];
                    vd.type = true;
                    videoData.Add(vd);
                }

                else if(videoList[i][2].Equals(".mp4") || videoList[i][2].Equals(".m4p") || videoList[i][2].Equals(".avi") || videoList[i][2].Equals(".wmv"))
                {
                    
                    String title =""; // 제목 10글자마다 줄바꿈
                    for (int j = 1 ; j <= videoList[i][1].Length ; j++){

                        title += videoList[i][1][j-1];
                        if(j % 11 == 0)
                        {
                            title += "\r\n";
                        }
                    }


                    vd.Title = title;
                    vd.mediaSource = videoList[i][0] + "/" + videoList[i][1];
                    vd.type = false;

                    int time = get_VedioTime(videoList[i][0] + "/" + videoList[i][1]);

                    vd.Time = TimeToString(time);
                    vd.ImageData = LoadImage(videoList[i][0] + "/" + videoList[i][1], videoList[i][1], time);
                    videoData.Add(vd);
                }
                
                
                
            }
            

            if(folder_parent_List.Count <= 0)//최상위 폴더면
            {
                
                pd.mediaSource = mainFolder;
                
            }
            else
            {
                pd.mediaSource = folder_parent_List.Pop();
            }

            
            TvBox.ItemsSource = videoData;

        }

        private void getList(List<String[]> videoList) //검색 시에 사용할 리스트 생성
        {

            videoData.Clear();

            videoData = new List<MovieData>(); // 영상 리스트


            for (int i = 0; i < videoList.Count; i++)
            {
                MovieData vd = new MovieData();
                if (videoList[i][2] == "")//확장자가 폴더면
                {
                    vd.ImageData = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Folder.png"));

                    String title = ""; // 제목 10글자마다 줄바꿈
                    for (int j = 1; j <= videoList[i][1].Length; j++)
                    {

                        title += videoList[i][1][j - 1];
                        if (j % 11 == 0)
                        {
                            title += "\r\n";
                        }
                    }
                    vd.Title = title;
                    vd.mediaSource = videoList[i][0];
                    vd.type = true;
                    videoData.Add(vd);
                }

                else if (videoList[i][2].Equals(".mp4") || videoList[i][2].Equals(".m4p") || videoList[i][2].Equals(".avi") || videoList[i][2].Equals(".wmv"))
                {

                    String title = ""; // 제목 10글자마다 줄바꿈
                    for (int j = 1; j <= videoList[i][1].Length; j++)
                    {

                        title += videoList[i][1][j - 1];
                        if (j % 11 == 0)
                        {
                            title += "\r\n";
                        }
                    }


                    vd.Title = title;
                    vd.mediaSource = videoList[i][0] + "/" + videoList[i][1];
                    vd.type = false;
                    
                    int time = get_VedioTime(videoList[i][0] + "/" + videoList[i][1]);

                    vd.Time = TimeToString(time);
                    vd.ImageData = LoadImage(videoList[i][0] + "/" + videoList[i][1], videoList[i][1], time);
                    videoData.Add(vd);
                }


            }

           


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
        private BitmapImage LoadImage(string file, String filename , int time)//영상경로, 영상 이름, 영상 시간(초)
        {
            FileInfo fileInfo = new FileInfo(mainFolder + "/thumbnail/" + filename + ".jpeg");//스크린샷이 있으면 스크린샷 생성 안함
            if (fileInfo.Exists)
            {
                return new BitmapImage(new Uri(mainFolder + "/thumbnail/" + filename + ".jpeg"));
            }


            //MemoryStream stream = new MemoryStream();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(file, mainFolder + "/thumbnail/" + filename + ".jpeg", time /3);//동영상 경로, 출력 경로, 시간(초)
            

            return new BitmapImage(new Uri(mainFolder + "/thumbnail/" + filename + ".jpeg"));
            
        }

        private int get_VedioTime(string file)//동영생 재생 시간 반환
        {
            FFProbe ffProbe = new FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(file);
            int time = (int)Math.Floor(videoInfo.Duration.TotalSeconds);

            return time;
        }


        private String TimeToString(int time)
        {
            int hour = time / 3600;
            int minutes = time % 3600 / 60;
            int secends = time % 3600 % 60;



            return hour.ToString("00") + ":" + minutes.ToString("00") + ":" + secends.ToString("00");
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

                if (movieData.type)//폴더면
                {
                    if (nowFolder.Equals(movieData.mediaSource))//현재폴더랑 같으면
                    {
                        MessageBox.Show("현재폴더입니다.");
                        return;
                    }
                    
                    if (movieData.Title == "상위 폴더")//상위폴더면
                    {
                        nowFolder = movieData.mediaSource;
                        
                    }
                    else if(movieData.Title == "홈")//홈이면 
                    {
                        nowFolder = mainFolder;
                        folder_parent_List.Clear();
                    }
                    else
                    {
                        folder_parent_List.Push(nowFolder);//스택에 추가 
                        nowFolder = movieData.mediaSource;
                        
                    }
                    pathLbl.Content = nowFolder;
                    
                    List<String[]> videoList = new List<string[]>();//영상목록 리스트

                    DirectoryInfo di = new DirectoryInfo(nowFolder); // 해당 폴더 정보를 가져옵니다. 
                    
                    videoList.Clear();
                    
                    DirectoryInfo[] dirs = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);//폴더 목록 검색
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
        //Thread t1 = null;
        //bool SearchI = false;
        private void Search_Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            

            if (Search_Text.Text == "")//검색창이 비어있으면
            {
                getList();
                return;
            }
            /*
           if (t1 != null)
            {
                t1.Abort();
            }

           t1 = new Thread(
                (search) =>
                {
                    List<String[]> videoList = new List<string[]>();//받아온 영상목록
                    DirectoryInfo di = new DirectoryInfo(nowFolder); // 해당 폴더 정보를 가져옵니다. 
                    foreach (FileInfo File in di.GetFiles("*" + search + "*", SearchOption.AllDirectories)) // 선택 폴더의 파일 목록을 스캔합니다. 
                    {
                        String[] video = new string[] { File.DirectoryName, File.Name, File.Extension };
                        videoList.Add(video);
                        getList(videoList);
                        SearchI = true;
                    }
                    TvBox.Dispatcher.Invoke(DispatcherPriority.DataBind , new Action(
                        () =>
                        {
                            TvBox.ItemsSource = videoData;
                            
                            

                        }
                        ));
                }
                );
            
            t1.Start(Search_Text.Text);


            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                     (ThreadStart)delegate ()
                     {
                         List<String[]> videoList = new List<string[]>();//받아온 영상목록
                         DirectoryInfo di = new DirectoryInfo(nowFolder); // 해당 폴더 정보를 가져옵니다. 
                         foreach (FileInfo File in di.GetFiles("*" + Search_Text.Text + "*", SearchOption.AllDirectories)) // 선택 폴더의 파일 목록을 스캔합니다. 
                         {
                             String[] video = new string[] { File.DirectoryName, File.Name, File.Extension };
                             videoList.Add(video);
                             getList(videoList);
                             SearchI = true;
                             TvBox.ItemsSource = videoData;
                         }
                     }
            );

           if (SearchI) {
                MessageBox.Show("asd");

                TvBox.ItemsSource = videoData;
                SearchI = false;
            }*/
            
        }

        private void test()
        {
            TvBox.ItemsSource = videoData;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Search_btn_Click(object sender, RoutedEventArgs e)
        {
            


             List<String[]> videoList = new List<string[]>();//받아온 영상목록
             DirectoryInfo di = new DirectoryInfo(nowFolder); // 해당 폴더 정보를 가져옵니다. 
             foreach (FileInfo File in di.GetFiles("*" + Search_Text.Text + "*", SearchOption.AllDirectories)) // 선택 폴더의 파일 목록을 스캔합니다. 
             {
                 String[] video = new string[] { File.DirectoryName, File.Name, File.Extension };
                 videoList.Add(video);

             }

            getList(videoList);   

        }

        private void pathLbl_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }
    }


}
