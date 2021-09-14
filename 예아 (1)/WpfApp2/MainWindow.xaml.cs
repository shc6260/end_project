using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
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
            lblPlayTime.Content = String.Format("{0} / {1}", mediaMain.Position.ToString(@"mm\:ss"), mediaMain.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;

            mediaMain.Play();
            p = 0;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;

            mediaMain.Stop();
            p = 1;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;
            
            mediaMain.Pause();
            p = 1;
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Win32 DLL 을 사용하여 선택할 파일 다이얼로그를 실행한다.
            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = ".avi",
                Filter = "All files (*.*)|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() == true)
            {
                // 선택한 파일을 Media Element에 지정하고 초기화한다.
                mediaMain.Source = new Uri(dlg.FileName);
                mediaMain.Volume = 0.5;
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
        }

        private void mediaMain_MediaEnded(object sender, RoutedEventArgs e)
        {
            // 미디어 중지
            mediaMain.Stop();
            p = 1;
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
            mediaMain.Volume = volume_bar.Value;
        }

        private void mediaMain_MouseMove(object sender, MouseEventArgs e)
        {
           
        }

        // 볼륨 조절 시작
        bool volume_press = false;
        double start_volume;

        private void sound_area_MouseMove(object sender, MouseEventArgs e)//볼륨 조절
        {
            if (volume_press)
            {
                if (mediaMain.Source == null)
                    return;
                Point pos = e.GetPosition((IInputElement)sender);

                double volume_set = ((pos.Y - start_volume) / sound_area.Height)/20;


                volume_bar.Value = volume_bar.Value - volume_set;
                mediaMain.Volume = volume_bar.Value ;
                
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

            double A = l * (x / mediaMain.Width);


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

                double A = l * (x / mediaMain.Width);

                //시, 분, 초 선언

                int hours, minute, second;

                //시간공식

                hours = (int)A / 3600;//시 공식

                minute = (int)A % 3600 / 60;//분을 구하기위해서 입력되고 남은값에서 또 60을 나눈다.

                second = (int)A % 3600 % 60;//마지막 남은 시간에서 분을 뺀 나머지 시간을 초로 계산함

                String time = hours + ":" + minute + ":" + second;

                lblJumpTime.Visibility = Visibility.Visible;
                lblJumpTime.Margin = new Thickness(pos.X - 20, pos.Y + 230, 0, 0);

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
    }
}
