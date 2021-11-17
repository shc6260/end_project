using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Permissions;
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
    /// fill_screen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class fill_screen : Window
    {
        MainWindow mainWindow;
        double playtime;
        int JumpTime = 2;
        public int _JimpTime
        {
            get { return JumpTime; }
            set { JumpTime = value;  }
        }

        int ssum;

        //영상이 정지상태인지 재생상태인지 확인
        //0 상태일때는 재생, 1상태일때는 정지
        int p;
        bool sldrDragStart = false;

        PlayListForm playListForm = null;

        //받아올 값 -> 진행된 시간:a / 현재 볼륨크기:b / 틀어져있던 영상:u / 점프타임:j / 영상정지상태유무:p
        public fill_screen(double playtime, double volume, Uri u , int j , int p , PlayListForm playListForm, int ssum, MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            mediaMain.Source = u;
            mediaMain.Volume = volume;
            volume_bar.Value = volume * 100;
            mediaMain.SpeedRatio = 1;
            mediaMain.Width = this.Width;
            mediaMain.Height = this.Height;
            this.playtime = playtime;
            this.p = p;
            this.ssum = ssum;

            this.playListForm = playListForm;

            Thumbnail_Create(playListForm.mediaSource);

            //점프타임 받아오기
            JumpTime = j;

            // 동영상 파일의 Timespan 제어를 위해 초기화와 이벤트처리기를 추가한다.
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            
            timer.Tick += TimerTickHandler;
            timer.Start();


            mediaMain.Play();
            if (mainWindow.thumbnail_CB.IsChecked == false) {
                thumOnBtn.Visibility = Visibility.Hidden;
                thumGrid.Visibility = Visibility.Hidden;
                thumOffBtn.Visibility = Visibility.Hidden;
            }
            else if (ssum == 1)
            {
                thumOnBtn.Visibility = Visibility.Visible;
                thumGrid.Visibility = Visibility.Hidden;
                thumOffBtn.Visibility = Visibility.Hidden;
            }

            if (mainWindow.RI)//로테이션이 켜져있으면
            {
                btnRotation.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\RotationON.png")));

            }

        }
        public int get_p()
        {
            return p;
        }
        public int get_ssum()
        {
            return ssum;
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
            lblPlayTime.Content = String.Format("{0}", mediaMain.Position.ToString(@"hh\:mm\:ss"));
            lblEndTime.Content = String.Format("{0}", mediaMain.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss"));
        }


        private void mediaMain_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (mainWindow.RI)//로테이션이 켜져있으면
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

        }//영상 종료시
        private void mediaMain_MediaOpened(object sender, RoutedEventArgs e)
        {
            mediaMain.Position = TimeSpan.FromSeconds(playtime);
            //전체화면으로 전환 이전에 영상이 정지상태였다면 일시정지
            mediaMain.Play();
            Thread.Sleep(1);
            if (p == 1)
            {
                mediaMain.Pause();
                btnPause.Visibility = Visibility.Hidden;
                btnStart.Visibility = Visibility.Visible;
            }
            else
            {
                btnPause.Visibility = Visibility.Visible;
                btnStart.Visibility = Visibility.Hidden;
            }
            sldrPlayTime.Minimum = 0;
            sldrPlayTime.Maximum = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;
        }
        private void mediaMain_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // 미디어 파일 실행 오류시
            MessageBox.Show("동영상 재생 실패 : " + e.ErrorException.Message.ToString());
        }

        private void volume_bar_start(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) { }

        private void volume_bar_completed(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mediaMain.Volume = volume_bar.Value / 100;
        }

        //화면에서 마우스휠 이동시 시간조정
        private void mediaMain_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (mainWindow.mouseWheel_CB.IsChecked == false)
            {
                return;
            }
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
        

        //키보드 이벤트
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //esc 키다운
            if (e.Key == Key.Escape)
            {
                this.DialogResult = true;
                Window.GetWindow(this).Close();
            }

            //우측 키다운
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
                if (mediaMain.Volume < 0)
                {
                    volume_bar.Value = 0;
                    mediaMain.Volume = 0;
                }
                else if (mediaMain.Volume > 1)
                {
                    volume_bar.Value = 100;
                    mediaMain.Volume = 1;
                }
                else
                {
                    volume_bar.Value += 5;
                    mediaMain.Volume += 0.05;
                }
            }
            //하단 키다운
            if (e.Key == Key.Down)
            {
                if (mediaMain.Volume < 0)
                {
                    volume_bar.Value = 0;
                    mediaMain.Volume = 0;
                }
                else if (mediaMain.Volume > 1)
                {
                    volume_bar.Value = 100;
                    mediaMain.Volume = 1;
                }
                else
                {
                    volume_bar.Value -= 5;
                    mediaMain.Volume -= 0.05;
                }
            }
            //스페이스바 - 일시정지 or 이어재생
            if (e.Key == Key.Space)
            {
                if (mediaMain.Source == null)
                    return;

                if (btnPause.Visibility == Visibility.Visible)
                {
                    btnPause_Click(btnPause, e);
                }
                else
                {
                    btnStart_Click(btnStart, e);
                }
            }
        }
        //마우스 이동시 상하단바

        DispatcherTimer timer2 = new DispatcherTimer();
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            timer2.Stop();
            BottomBar.Visibility = Visibility.Visible;
            TopBar.Visibility = Visibility.Visible;
            this.Cursor = Cursors.Arrow;
            timer2.Interval = TimeSpan.FromMilliseconds(3000);
            timer2.Tick += timer2_Tick;
            timer2.Start();
        }
        // 마우스 이동시 상하단바 타임 핸들러
        void timer2_Tick(object sender, EventArgs e)
        {
            BottomBar.Visibility = Visibility.Hidden;
            TopBar.Visibility = Visibility.Hidden;
            this.Cursor = Cursors.None;
        }




        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Window.GetWindow(this).Close();

        }

       



        private void TopBar_MouseMove(object sender, MouseEventArgs e)
        {
            TopBar.Visibility = Visibility.Visible;
        }

        private void BottomBar_MouseMove(object sender, MouseEventArgs e)
        {
            BottomBar.Visibility = Visibility.Visible;
        }

        private void sldrPlayTime_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }


        bool volume_press = false; //볼륨 드레그 변수
        double start_volume; //볼륨 조절 시작 위치

        bool time_press = false;//시간 드레그 변수


        private void mediaMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //마우스 휠 다운 이벤트(건너뛰기 시간 설정 새창 열기)
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                if (mainWindow.mouseWheel_CB.IsChecked == false)
                {
                    return;
                }
                WpfApp2.TimeJumpSet timeJumpSet = new WpfApp2.TimeJumpSet(JumpTime);
                timeJumpSet.Top = this.Top;
                timeJumpSet.Left = this.Left;
                if (timeJumpSet.ShowDialog() == true)
                {


                    string a = timeJumpSet.jumptime.Text;
                    JumpTime = Convert.ToInt32(a);
                }
            }
            //마우스 클릭 시간, 볼륨 조절
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                if (e.GetPosition((IInputElement)sender).Y > (mediaMain.ActualHeight - 100))//시간 조절
                {
                    if (mainWindow.mouseTime_CB.IsChecked == false)
                    {
                        return;
                    }
                    time_press = true;
                }
                else if (e.GetPosition((IInputElement)sender).X > mediaMain.ActualWidth - 120)//볼륨 조절
                {
                    if (mainWindow.mouseVol_CB.IsChecked == false)
                    {
                        return;
                    }
                    volume_press = true;

                    start_volume = e.GetPosition((IInputElement)sender).Y;
                }

                else if (e.ClickCount == 2)//전체화면 끝
              {
                    if (mainWindow.mouseFill_CB.IsChecked == false)
                    {
                        return;
                    }
                    this.DialogResult = true;
                    Window.GetWindow(this).Close();
                }

                else//일시정지
                {  
                    if (mainWindow.mousePause_CB.IsChecked == false)
                    {
                        return;
                    }
                    if (btnPause.Visibility == Visibility.Visible)
                    {
                        btnPause_Click(btnPause, e);
                    }
                    else
                    {
                        btnStart_Click(btnStart, e);
                    }


                }
            }

        }




        private void mediaMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (time_press) //클릭 시 마우스 옆에 시간
            {

                if (mediaMain.Source == null)
                    return;
                Point pos = e.GetPosition((IInputElement)sender);

                double x = pos.X;
                double l = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;

                double A = l * (x / mediaMain.ActualWidth);

                //시, 분, 초 선언

                int hours, minute, second;

                //시간공식

                hours = (int)A / 3600;//시 공식

                minute = (int)A % 3600 / 60;//분을 구하기위해서 입력되고 남은값에서 또 60을 나눈다.

                second = (int)A % 3600 % 60;//마지막 남은 시간에서 분을 뺀 나머지 시간을 초로 계산함

                String time = hours + ":" + minute + ":" + second;

                lblJumpTime.Visibility = Visibility.Visible;
                lblJumpTime.FontSize = 30;

                // 플레이시간이 변경되면, 표시영역을 업데이트한다.
                lblJumpTime.Content = String.Format("{0} / {1}", time, mediaMain.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss"));
            }
            else if (volume_press)//볼륨 영역이 클릭 됐을 경우
            {
                if (mediaMain.Source == null)
                    return;

                Point pos = e.GetPosition((IInputElement)sender);

                double volume_set = (pos.Y - start_volume) / 20;


                volume_bar.Value = (int)(volume_bar.Value - volume_set);
                mediaMain.Volume = volume_bar.Value / 100;

                lblJumpTime.FontSize = 50;
                lblJumpTime.Visibility = Visibility.Visible;
                lblJumpTime.Content = volume_bar.Value;

            }
        }
        private void mediaMain_MouseUp(object sender, MouseButtonEventArgs e)
        {


            if (mediaMain.Source == null)
                return;

            if (time_press)
            {
                Point pos = e.GetPosition((IInputElement)sender);

                double x = pos.X;
                double l = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;

                double A = l * (x / mediaMain.ActualWidth);


                mediaMain.Position = TimeSpan.FromSeconds(A);

                lblJumpTime.Visibility = Visibility.Hidden;

                time_press = false;
            }
            else if (volume_press)
            {
                lblJumpTime.Visibility = Visibility.Hidden;
                volume_press = false;
            }
        }

        private void mediaMain_MouseLeave(object sender, MouseEventArgs e)
        {
            if (time_press == true)
            {
                time_press = false;
                lblJumpTime.Visibility = Visibility.Hidden;
            }
            else if (volume_press)
            {
                volume_press = false;
                lblJumpTime.Visibility = Visibility.Hidden;
            }
        }

        private void btnRotation_Click(object sender, RoutedEventArgs e)//반복재생 버튼
        {

            if (!mainWindow.RI)//꺼져있으면
            {
                mainWindow.RI = true;
                btnRotation.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\RotationON.png")));

            }
            else
            {
                mainWindow.RI = false;
                btnRotation.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Rotation.png")));
            }
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

            btnPause.Visibility = Visibility.Visible;
            btnStart.Visibility = Visibility.Hidden;
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

        private void btnleft_Click(object sender, RoutedEventArgs e)//시간 되감기
        {
            mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds - 10);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)//시간 건너뛰기
        {
            mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds + 10);
        }

        double pastVol = 0;
        private void btnVol_Click(object sender, RoutedEventArgs e)//볼륨 버튼 클릭 이벤트 음소거
        {
            if (volume_bar.Value == 0)//음소거 상태면
            {

                volume_bar.Value = pastVol;
                mediaMain.Volume = volume_bar.Value / 100;

            }
            else
            {

                pastVol = volume_bar.Value;
                volume_bar.Value = 0;
                mediaMain.Volume = 0;
            }
        }

        private void small_Btn_Click(object sender, RoutedEventArgs e)//최소화 아이콘 클릭
        {
            this.DialogResult = true;
            Window.GetWindow(this).Close();
        }


        private void thumOffBtn_Click(object sender, RoutedEventArgs e)
        {
            thumOnBtn.Visibility = Visibility.Visible;
            thumGrid.Visibility = Visibility.Hidden;
            thumOffBtn.Visibility = Visibility.Hidden;
            ssum = 1;
        }
        private void thumOnBtn_Click(object sender, RoutedEventArgs e)
        {
            thumOnBtn.Visibility = Visibility.Hidden;
            thumGrid.Visibility = Visibility.Visible;
            thumOffBtn.Visibility = Visibility.Visible;
            ssum = 0;
        }

        public void Thumbnail_Create(String Source)
        {
            int t = Image.get_VedioTime(Source) / 5;

            BitmapImage[] bi = new BitmapImage[4];

            for (int i = 1; i < 5; i++)
            {
                bi[i - 1] = Image.LoadImage(Source, i, t * i);
            }

            thumImage1.Source = bi[0];
            thumImage2.Source = bi[1];
            thumImage3.Source = bi[2];
            thumImage4.Source = bi[3];

            thumImage1.Tag = t * 1;
            thumImage2.Tag = t * 2;
            thumImage3.Tag = t * 3;
            thumImage4.Tag = t * 4;

        }

        private void thumImage1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                System.Windows.Controls.Image img = (System.Windows.Controls.Image)sender;

                mediaMain.Position = TimeSpan.FromSeconds(Convert.ToDouble(img.Tag.ToString()));
            }
        }

        private void thumImage1_MouseEnter(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = (System.Windows.Controls.Image)sender;
            img.Opacity = 0.5;

        }

        private void thumImage1_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Image img = (System.Windows.Controls.Image)sender;
            img.Opacity = 1;
        }

    }
}
