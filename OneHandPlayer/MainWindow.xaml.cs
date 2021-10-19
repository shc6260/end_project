using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        //시간 건너뛰기 초기값
        int JumpTime = 2;

        //영상이 정지상태인지 재생상태인지 확인
        //0 상태일때는 재생, 1상태일때는 정지
        int p = 0;

        bool sldrDragStart = false;

        bool RI = false; //반복재생 인덱스

        List<String[]> videoList = new List<string[]>();
        PlayListForm playListForm = null;//리스트 폼
        public MainWindow()
        {
            InitializeComponent();

            //화면 중앙으로
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            double windowWidth = this.Width;

            double windowHeight = this.Height;

            this.Left = (screenWidth / 2) - (windowWidth / 2);

            this.Top = (screenHeight / 2) - (windowHeight / 2);

        }

        // 미디어파일 타임 핸들러
        // 미디어파일의 실행시간이 변경되면 호출된다.
        void TimerTickHandler(object sender, EventArgs e)
        {
            // 미디어파일 실행시간이 변경되었을 때 사용자가 임의로 변경하는 중인지를 체크한다.
            if (sldrDragStart)
                return;

            if (mediaMain.Source == null
                || !mediaMain.NaturalDuration.HasTimeSpan)
            {
                lblPlayTime.Content = "No file selected...";
                return;
            }

            // 미디어 파일 총 시간을 슬라이더와 동기화한다.
            sldrPlayTime.Value = mediaMain.Position.TotalSeconds;
        }
        private void sldrPlayTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            sldrDragStart = true;
            mediaMain.Pause();
        }

        private void sldrPlayTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            // 사용자가 지정한 시간대로 이동하면, 이동한 시간대로 값을 지정한다.
            mediaMain.Position = TimeSpan.FromSeconds(sldrPlayTime.Value);

            // 멈췄던 미디어를 재실행한다.
            mediaMain.Play();
            sldrDragStart = false;
        }

        private void sldrPlayTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaMain.Source == null)
                return;

            // 플레이시간이 변경되면, 표시영역을 업데이트한다.
            lblPlayTime.Content = String.Format("{0}", mediaMain.Position.ToString(@"mm\:ss"));
            lblEndTime.Content = String.Format("{0}", mediaMain.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;

            mediaMain.Play();
            p = 0;

            btnPause.Visibility = Visibility.Visible;
            btnStart.Visibility = Visibility.Hidden;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;
            mediaMain.Stop();
            p = 1;

            btnPause.Visibility = Visibility.Hidden;
            btnStart.Visibility = Visibility.Visible;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;
            
            mediaMain.Pause();
            p = 1;

            btnPause.Visibility = Visibility.Hidden;
            btnStart.Visibility = Visibility.Visible;
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)//영상목록 버튼 클릭 이벤트
        {
            
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); // 새로운 폴더 선택 Dialog 를 생성합니다.
            
            dialog.IsFolderPicker = true; //
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) // 폴더 선택이 정상적으로 되면 아래 코드를 실행합니다.
            {
                GetFileListFromFolderPath(dialog.FileName);
                
                playListForm = new PlayListForm(videoList,this,dialog.FileName);
                playListForm.Top = this.Top;
                playListForm.Left = this.Left - playListForm.Width ;
                playListForm.Show();
            }
        }

        private void GetFileListFromFolderPath(string FolderName)//파일 목록 리스트
        { 
            DirectoryInfo di = new DirectoryInfo(FolderName); // 해당 폴더 정보를 가져옵니다. 
            /*DataTable dt1 = new DataTable(); // 새로운 테이블 작성합니다.(FileInfo 에서 가져오기 원하는 속성을 열로 추가합니다.) 
            dt1.Columns.Add("Folder", typeof(string)); // 파일의 폴더 
            dt1.Columns.Add("FileName", typeof(string)); // 파일 이름(확장자 포함) 
            dt1.Columns.Add("Extension", typeof(string)); // 확장자 
            dt1.Columns.Add("CreationTime", typeof(DateTime)); // 생성 일자 
            dt1.Columns.Add("LastWriteTime", typeof(DateTime)); // 마지막 수정 일자 
            dt1.Columns.Add("LastAccessTime", typeof(DateTime)); // 마지막 접근 일자 
            dt1.Columns.Add("Lenth", typeof(long)); //파일의 크기*/
            videoList.Clear();

            DirectoryInfo[] dirs = di.GetDirectories("*.*", SearchOption.AllDirectories);//폴더 목록 검색
            foreach (DirectoryInfo d in dirs)
            {
                if(d.Name == "thumbnail")
                {//썸네일 폴더 안나오게
                    continue;
                }
                String[] dir = new string[] { d.FullName, d.Name, d.Extension };
                videoList.Add(dir);
            }

            foreach (FileInfo File in di.GetFiles()) // 선택 폴더의 파일 목록을 스캔합니다. 
            {
                //dt1.Rows.Add(File.DirectoryName, File.Name, File.Extension, File.CreationTime, File.LastWriteTime, File.LastAccessTime, File.Length); // 개별 파일 별로 정보를 추가합니다.
                
                String[] video = new string[] { File.DirectoryName, File.Name, File.Extension };
                videoList.Add(video);
            }
            /*if (ch == true) // 하위 폴더 포함될 경우 
            {
                DirectoryInfo[] di_sub = di.GetDirectories(); // 하위 폴더 목록들의 정보 가져옵니다. 
                foreach (DirectoryInfo di1 in di_sub) // 하위 폴더목록을 스캔합니다. 
                {
                    foreach (FileInfo File in di1.GetFiles()) // 선택 폴더의 파일 목록을 스캔합니다. 
                    {
                        dt1.Rows.Add(File.DirectoryName, File.Name, File.Extension, File.CreationTime, File.LastWriteTime, File.LastAccessTime, File.Length); // 개별 파일 별로 정보를 추가합니다. 
                    }
                }
            }*/
            
        }


        /*private void ShowDataFromDataTableToDataGrid(DataTable dt1, DataGrid dgv1)
        {
            //xaml에서 미리 Binding Path를 해두어야합니다.
            dgv1.ItemsSource = dt1.DefaultView;
        }*/

        public void Media_Play(String Source)//영상 경로 받아와서 실행
        {
            // 선택한 파일을 Media Element에 지정하고 초기화한다.
            mediaMain.Source = new Uri(Source);
            mediaMain.Volume = 20;
            mediaMain.SpeedRatio = 1;

            // 동영상 파일의 Timespan 제어를 위해 초기화와 이벤트처리기를 추가한다.
            DispatcherTimer timer = new DispatcherTimer()
            {
                    Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += TimerTickHandler;
            timer.Start();


            // 선택한 파일을 실행
            mediaMain.Play();
            p = 0;
        }

        private void mediaMain_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (RI)//로테이션이 켜져있으면
            {
                //TimeSpan.FromSeconds(0);
                mediaMain.Stop();
                mediaMain.Play();

            }
            else
            {
                // 미디어 중지
                mediaMain.Stop();
                p = 1;
                btnPause.Visibility = Visibility.Hidden;
                btnStart.Visibility = Visibility.Visible;
            }
        
        }

        private void mediaMain_MediaOpened(object sender, RoutedEventArgs e)
        {
            // 미디어 파일이 열리면, 플레이타임 슬라이더의 값을 초기화 한다.
            sldrPlayTime.Minimum = 0;
            sldrPlayTime.Maximum = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;

            // 미디어 파일이 열리면, 볼륨을 볼륨슬라이더가 위치한 자리에 맞춘다.


        }

        private void mediaMain_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // 미디어 파일 실행 오류시
            MessageBox.Show("동영상 재생 실패 : " + e.ErrorException.Message.ToString());
        }

        private void mediaMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //마우스 휠 다운 이벤트(건너뛰기 시간 설정 새창 열기)
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                WpfApp2.TimeJumpSet timeJumpSet = new WpfApp2.TimeJumpSet(JumpTime);
                if (timeJumpSet.ShowDialog() == true)
                {
                    string a = timeJumpSet.jumptime.Text;
                    JumpTime = Convert.ToInt32(a);
                }
            }

         

        }

        private void mediaMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        //화면에서 마우스휠 이동시 시간조정
        private void mediaMain_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //마우스 휠 위로 올릴경우,초기설정 2초
            if (e.Delta > 0)
            {
                mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds + JumpTime);
            }
            //마우스 휠 아래로 내릴경우
            else if (e.Delta < 0)
            {
                mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds - JumpTime);
            }
        }
        //키보드 상하좌우키 누를시 볼륨,시간조정
        private void Main_keydown(object sender, KeyEventArgs e)
        {
            //우측 키다운, 초기설정 2초
            if (e.Key == Key.Right)
            {
                mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds + JumpTime);
            }
            //좌측 키다운
            else if (e.Key == Key.Left)
            {
                mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds - JumpTime);
            }
            //상단 키다운
            if (e.Key == Key.Up)
            {
                volume_bar.Value += 0.1;
                mediaMain.Volume += 0.1;
            }
            //하단 키다운
            if (e.Key == Key.Down)
            {
                volume_bar.Value -= 0.1;
                mediaMain.Volume -= 0.1;
            }
            //스페이스바 - 일시정지 or 이어재생
            if(e.Key == Key.Space)
            {
                if (mediaMain.Source == null)
                    return;

                if (p == 0)
                {
                    mediaMain.Pause();
                    p = 1;
                }
                else if (p == 1)
                {
                    mediaMain.Play();
                    p = 0;
                }
            }

        }

        private void volume_bar_start(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) { }

        private void volume_bar_completed(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) 
        {
            mediaMain.Volume = volume_bar.Value/100;
        }

        private void mediaMain_MouseMove(object sender, MouseEventArgs e)
        {
           
        }

        // 드래그 볼륨 조절 시작
        bool volume_press = false;
        double start_volume;

        private void sound_area_MouseMove(object sender, MouseEventArgs e)//볼륨 조절
        {
            if (volume_press)
            {
                if (mediaMain.Source == null)
                    return;
                Point pos = e.GetPosition((IInputElement)sender);

                double volume_set = ((pos.Y - start_volume) / sound_area.Height)/15;


                volume_bar.Value = (int)(volume_bar.Value - volume_set*100);
                mediaMain.Volume = volume_bar.Value/100 ;
                
            }
        }

        private void sound_area_MouseDown(object sender, MouseButtonEventArgs e)
        {
            volume_press = true;
            
            start_volume = e.GetPosition((IInputElement)sender).Y;
        }

        private void sound_area_MouseUp(object sender, MouseButtonEventArgs e)
        {
            volume_press = false;
        }

        private void sound_area_MouseLeave(object sender, MouseEventArgs e)
        {
            volume_press = false;
        }
        // 볼륨조절 끝

        //시간 드래그 시작

        bool time_press = false;

        private void time_area_MouseDown(object sender, MouseButtonEventArgs e)
        {
            time_press = true;
        }

        private void time_area_MouseUp(object sender, MouseButtonEventArgs e)
        {
            time_press = false;

            if (mediaMain.Source == null)
                return;

            Point pos = e.GetPosition((IInputElement)sender);

            double x = pos.X;
            double l = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;

            double A = l * (x / WindowMain.Width);


            mediaMain.Position = TimeSpan.FromSeconds(A);

            lblJumpTime.Visibility = Visibility.Hidden;
        }

        private void time_area_MouseLeave(object sender, MouseEventArgs e)
        {
            time_press = false;
            lblJumpTime.Visibility = Visibility.Hidden;
        }

        private void time_area_MouseMove(object sender, MouseEventArgs e)
        {
            if (time_press) //클릭 시 마우스 옆 
            {
                if (mediaMain.Source == null)
                    return;
                Point pos = e.GetPosition((IInputElement)sender);

                double x = pos.X;
                double l = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;

                double A = l * (x / WindowMain.Width);

                //시, 분, 초 선언

                int hours, minute, second;

                //시간공식

                hours = (int)A / 3600;//시 공식

                minute = (int)A % 3600 / 60;//분을 구하기위해서 입력되고 남은값에서 또 60을 나눈다.

                second = (int)A % 3600 % 60;//마지막 남은 시간에서 분을 뺀 나머지 시간을 초로 계산함

                String time = hours + ":" + minute + ":" + second;

                lblJumpTime.Visibility = Visibility.Visible;
                lblJumpTime.Margin = new Thickness(pos.X - 500, pos.Y + 250 , 0, 0);

                // 플레이시간이 변경되면, 표시영역을 업데이트한다.
                lblJumpTime.Content = String.Format("{0} / {1}", time, mediaMain.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss"));
            }
        }
        //시간 드래그 끝 


        //전체화면 버튼 클릭시
        private void fill_btn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;
            mediaMain.Pause();

            double playtime = mediaMain.Position.TotalSeconds;
            double volume = mediaMain.Volume;
            Uri u = mediaMain.Source;


            WpfApp2.fill_screen F_S = new WpfApp2.fill_screen(playtime, volume, u, JumpTime , p);
            
            if (F_S.ShowDialog() == true)
            {
                playtime = F_S.mediaMain.Position.TotalSeconds;
                volume = F_S.mediaMain.Volume;
                mediaMain.Position = TimeSpan.FromSeconds(playtime);
                mediaMain.Volume = volume;

                mediaMain.Play();
                Thread.Sleep(1);
                if (F_S.get_p() == 1)
                    mediaMain.Pause();

                
            }


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

        private void volume_bar_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void sldrPlayTime_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void btnleft_Click(object sender, RoutedEventArgs e)
        {
            mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds - 5);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds + 5);
        }

        private void btnRotation_Click(object sender, RoutedEventArgs e)
        {
            
            if (!RI)
            {
                RI = true;
                btnRotation.Background = new ImageBrush(new BitmapImage(new Uri("Images/RotationON.png")));

            }
        }

        bool visible_switch = false;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralTransform generalTransform1 = Option.TransformToAncestor(this);
            Point currentPoint = generalTransform1.Transform(new Point(0, 0));

            if (!visible_switch)
            {
                TestStack.Visibility = Visibility.Visible;
                visible_switch = true;
            }
            else
            {
                TestStack.Visibility = Visibility.Hidden;
                visible_switch = false;
            }
        }


        bool playList_Index = true;
        private void plylistbtn_Click(object sender, RoutedEventArgs e)
        {
            
            if (playListForm == null)
            {
                return;
            }

            if (playList_Index)
            {
                playListForm.Hide();
                playList_Index = false;
            }

            else
            {
                playListForm.Show();
                playList_Index = true;
            }
        }

        private void WindowMain_Closed(object sender, EventArgs e)
        {
            playListForm.Close();
            /*
            DirectoryInfo di = new DirectoryInfo(playListForm._mainFolder + "/thumbnail");
            
            if (di.Exists == true)//썸네일 폴더 삭제
            {
                Directory.Delete(playListForm._mainFolder + "/thumbnail", true);

                di.Delete(true);
            } 보류*/

        }

        private void WindowMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
