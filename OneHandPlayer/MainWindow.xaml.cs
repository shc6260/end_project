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


        PlayListForm playListForm = null;//리스트 폼
        BookmarkForm bookmarkForm = null;
        public PlayListForm _playListForm
        {
            set { playListForm = value; }
        }

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

            this.Hide();


            StartWindow startWindow = new StartWindow(playListForm, this);
            startWindow.ShowDialog();


            try//창이 닫혔으면 창을 띄우지 않음
            {
                this.Show();
            }
            catch(System.InvalidOperationException)
            {
                this.Close();
            }
            //북마크 폼 초기화

            bookmarkForm = new BookmarkForm();
            bookmarkForm.Show();
            bookmarkForm.Visibility = Visibility.Hidden;
            bookmarkForm.Height = this.Height;
            bookmarkForm.Top = this.Top;

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

        

        public void Media_Play(String Source)//영상 경로 받아와서 실행
        {
            // 선택한 파일을 Media Element에 지정하고 초기화한다.
            mediaMain.Source = new Uri(Source);
            mediaMain.Volume = volume_bar.Value/100;
            mediaMain.SpeedRatio = 1;

            // 동영상 파일의 Timespan 제어를 위해 초기화와 이벤트처리기를 추가한다.
            DispatcherTimer timer = new DispatcherTimer()
            {
                    Interval = TimeSpan.FromSeconds(1)
            };
            timer.Stop();
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

     

      


        //전체화면 버튼 클릭시
        private void fill_btn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.Source == null)
                return;
            mediaMain.Pause();

            double playtime = mediaMain.Position.TotalSeconds;
            double volume = mediaMain.Volume;
            Uri u = mediaMain.Source;

            this.Visibility = Visibility.Hidden;
            playListForm.Visibility = Visibility.Hidden;
            bookmarkForm.Visibility = Visibility.Hidden;

    
            WpfApp2.fill_screen F_S = new WpfApp2.fill_screen(playtime, volume, u, JumpTime , p);
            F_S._JimpTime = this.JumpTime;


            F_S.ShowDialog();
            playtime = F_S.mediaMain.Position.TotalSeconds;
            volume = F_S.mediaMain.Volume;
            this.JumpTime = F_S._JimpTime;
            mediaMain.Position = TimeSpan.FromSeconds(playtime);
            mediaMain.Volume = volume;

            mediaMain.Play();
            Thread.Sleep(1);
            if (F_S.get_p() == 1)
                mediaMain.Pause();
            
            
            this.Visibility = Visibility.Visible;
            if (playList_Index) {
                playListForm.Visibility = Visibility.Visible;
            }
            if (bookmark_Index) {
                bookmarkForm.Visibility = Visibility.Visible;
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
            
        }//닫기 버튼 클릭

        private void volume_bar_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void sldrPlayTime_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void btnleft_Click(object sender, RoutedEventArgs e)//시간 되감기
        {
            mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds - 10);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)//시간 건너뛰기
        {
            mediaMain.Position = TimeSpan.FromSeconds(mediaMain.Position.TotalSeconds + 10);
        }

        private void btnRotation_Click(object sender, RoutedEventArgs e)//반복재생 버튼
        {

            if (!RI)//꺼져있으면
            {
                RI = true;
                btnRotation.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\RotationON.png")));

            }
            else
            {
                RI = false;
                btnRotation.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Rotation.png")));
            }
        }

        bool visible_switch = false;
        private void Button_Click(object sender, RoutedEventArgs e)//설정버튼 클릭
        {
           //GeneralTransform generalTransform1 = Option.TransformToAncestor(this);
           //Point currentPoint = generalTransform1.Transform(new Point(0, 0));
           
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
        private void plylistbtn_Click(object sender, RoutedEventArgs e)//플레이 리스트 버튼 클릭
        {
            
            if (playListForm == null)
            {
                return;
            }

            if (playList_Index)
            {
                playListForm.Hide();
                playList_Index = false;

                if (fullI)//전체화면 시 리스트 끄면 화면 키우기
                {
                    if (bookmark_Index == false)
                    {
                        this.Height = System.Windows.SystemParameters.WorkArea.Height;
                        this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                        this.Left = 0;
                        this.Top = 0;
                    }
                    else
                    {
                        this.Height = System.Windows.SystemParameters.WorkArea.Height;
                        this.Width = System.Windows.SystemParameters.PrimaryScreenWidth - bookmarkForm.Width;
                        this.Left = bookmarkForm.Width;
                        this.Top = 0;
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - bookmarkForm.Width;
                    }
                }
                else//전체화면 아닐때
                {
                    if (bookmark_Index)//북마크가 켜져있으면
                    {
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - bookmarkForm.Width;
                    }
                }

            }

            else // 리스트 꺼져있을때 리스트 키기
            {
                
                if (fullI)//전체화면 상태면
                {
                    if (bookmark_Index == false)//북마크 폼이 꺼져있으면
                    {
                        this.Height = System.Windows.SystemParameters.WorkArea.Height;
                        this.Width = System.Windows.SystemParameters.PrimaryScreenWidth - playListForm.Width;
                        this.Left = playListForm.Width;
                        this.Top = 0;
                        playListForm.Top = this.Top;
                        playListForm.Left = this.Left - playListForm.Width;
                    }
                    else//북마크 폼이 켜져있으면
                    {
                        this.Height = System.Windows.SystemParameters.WorkArea.Height;
                        this.Width = System.Windows.SystemParameters.PrimaryScreenWidth - playListForm.Width - bookmarkForm.Width;
                        this.Left = playListForm.Width + bookmarkForm.Width;
                        this.Top = 0;
                        playListForm.Top = this.Top;
                        playListForm.Left = this.Left - playListForm.Width;
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = 0;
                    }
                }
                else
                {
                    if(bookmark_Index == false)//북마크가 꺼져있으면
                    {
                        //그냥 내려가서 출력
                    }
                    else//북마크가 켜져있으면
                    {
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - bookmarkForm.Width - playListForm.Width;
                    }
                }


                playListForm.Show();
                playList_Index = true;

            }
        }

        bool bookmark_Index = false;
        private void bookMarkBtn_Click(object sender, RoutedEventArgs e)//북마크 버튼
        {
            if (bookmarkForm == null)
            {
                return;
            }

            if (bookmark_Index)
            {
                bookmarkForm.Hide();
                bookmark_Index = false;

                if (fullI)//전체화면 시 북마크 끄면 화면 키우기
                {
                    if (playList_Index == false)//플레이 리스트가 꺼져있으면
                    {
                        this.Height = System.Windows.SystemParameters.WorkArea.Height;
                        this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                        this.Left = 0;
                        this.Top = 0;
                    }
                    else//플레이 리스트가 켜져있으면
                    {
                        this.Width = this.Width + bookmarkForm.Width;
                        this.Left = playListForm.Width;
                    }
                }

            }

            else // 북마크 꺼져있을때 북마크 키기
            {
                

                

                if (fullI)//전체화면 상태면
                {
                    if (playList_Index == false)//플레이 리스트가 꺼져있으면
                    {
                        this.Height = System.Windows.SystemParameters.WorkArea.Height;
                        this.Width = System.Windows.SystemParameters.PrimaryScreenWidth - bookmarkForm.Width;
                        this.Left = bookmarkForm.Width;
                        this.Top = 0;
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - bookmarkForm.Width;
                    }
                    else//플레이 리스트가 켜져있으면
                    {
                        this.Height = System.Windows.SystemParameters.WorkArea.Height;
                        this.Width = System.Windows.SystemParameters.PrimaryScreenWidth - bookmarkForm.Width - playListForm.Width;
                        this.Left = bookmarkForm.Width + playListForm.Width;
                        this.Top = 0;
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - bookmarkForm.Width - playListForm.Width;
                    }
                }

                else
                {
                    if (playList_Index == false)//플레이 리스트가 꺼져있으면
                    {
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - bookmarkForm.Width;
                    }
                    else//플레이 리스트가 켜져있으면
                    {
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - bookmarkForm.Width - playListForm.Width;
                    }
                }

                bookmarkForm.Show();
                bookmark_Index = true;

            }
        }

        private void WindowMain_Closed(object sender, EventArgs e)
        {
            if (playListForm != null)
            {
                playListForm.Close();
                bookmarkForm.Close();
            }
            /*
            DirectoryInfo di = new DirectoryInfo(playListForm._mainFolder + "/thumbnail");
            
            if (di.Exists == true)//썸네일 폴더 삭제
            {
                Directory.Delete(playListForm._mainFolder + "/thumbnail", true);

                di.Delete(true);
            } 보류*/

        }//창 닫을때

        private void WindowMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void WindowMain_SizeChanged(object sender, SizeChangedEventArgs e)//크기가 변경되었을때
        {
            if (playListForm != null)//창이 떠있으면 조정
            {
                playListForm.Height = this.Height;
                playListForm.Top = this.Top;
                playListForm.Left = this.Left - playListForm.Width;
            }
        }

        //전체화면 전 화면 값 저장 변수
        double postH;
        double postW;
        double postL;
        double postT;
        bool fullI = false;
        private void fullScreen_Click(object sender, RoutedEventArgs e)//최대화 버튼 클릭 이벤트
        {
            if (!fullI)//전체화면 상태가 아니면
            {
                //현재값 저장
                postH = this.Height;
                postW = this.Width;
                postL = this.Left;
                postT = this.Top;
                
                fullI = true;

                if (playList_Index)//리스트가 떠있으면
                {
                    this.Height = System.Windows.SystemParameters.WorkArea.Height;
                    this.Width = System.Windows.SystemParameters.PrimaryScreenWidth - playListForm.Width;
                    this.Left = playListForm.Width;
                    this.Top = 0;
                    playListForm.Height = System.Windows.SystemParameters.WorkArea.Height;
                    playListForm.Top = this.Top;
                    playListForm.Left = this.Left - playListForm.Width;

                }
                else//리스트가 꺼져있으면
                {
                    //전체화면 만들기
                    this.Height = System.Windows.SystemParameters.WorkArea.Height;
                    this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                    this.Left = 0;
                    this.Top = 0;
                }
            }
            else//전체화면 상태면
            {
                fullI = false;
                this.Width = postW;
                this.Height = postH;
                this.Top = postT;
                this.Left = postL;
                playListForm.Top = this.Top;
                playListForm.Left = this.Left - playListForm.Width;
            }

            
        }

        private void homeBtn_Click(object sender, RoutedEventArgs e)//홈 클릭 이벤트
        {
            playListForm.Close();
            playListForm = null;
            this.Hide();
            

            StartWindow startWindow = new StartWindow(playListForm, this);
            startWindow.ShowDialog();

            

            try//창이 닫혔으면 창을 띄우지 않음
            {
                this.Show();
            }
            catch (System.InvalidOperationException)
            {
                this.Close();
            }
        }

        private void miniBtn_Click(object sender, RoutedEventArgs e)//최소화 버튼 클릭 이벤트
        {
            this.WindowState = WindowState.Minimized;
            playListForm.WindowState = WindowState.Minimized;
            bookmarkForm.WindowState = WindowState.Minimized;
        }



        private void WindowMain_StateChanged(object sender, EventArgs e)//최소화 헤제 시 발생 이벤트 
        {
            if (this.WindowState == WindowState.Normal)//최소화 해제시 발생 이벤트
            {
                playListForm.WindowState = WindowState.Normal;
                playListForm.Top = this.Top;
                playListForm.Left = this.Left - playListForm.Width;

                bookmarkForm.WindowState = WindowState.Normal;
                if (playList_Index)//플레이 리스트가 켜져있으면
                {
                    bookmarkForm.Top = this.Top;
                    bookmarkForm.Left = this.Left - playListForm.Width - bookmarkForm.Width;
                }
                else//플레이 리스트가 꺼져있으면
                {
                    bookmarkForm.Top = this.Top;
                    bookmarkForm.Left = this.Left - bookmarkForm.Width;
                }

                


            }
        }

        private void WindowMain_LocationChanged(object sender, EventArgs e)//창 위치 변경
        {
            if (playListForm != null)
            {
                //플레이 리스트는 안보여도 따라다님
                playListForm.Top = this.Top;
                playListForm.Left = this.Left - playListForm.Width;

                if (bookmark_Index)
                {
                    if (playList_Index)
                    {
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left - playListForm.Width - bookmarkForm.Width;
                    }
                    else
                    {
                        bookmarkForm.Top = this.Top;
                        bookmarkForm.Left = this.Left- bookmarkForm.Width;
                    }
                }
            }
        }

        double pastVol = 0;
        private void btnVol_Click(object sender, RoutedEventArgs e)//볼륨 버튼 클릭 이벤트 음소거
        {
            if(volume_bar.Value == 0)//음소거 상태면
            {

                volume_bar.Value = pastVol;
                mediaMain.Volume = volume_bar.Value/100;
                
            }
            else
            {

                pastVol = volume_bar.Value;
                volume_bar.Value = 0;
                mediaMain.Volume = 0;
            }
        }

        bool volume_press = false; //볼륨 드레그 변수
        double start_volume; //볼륨 조절 시작 위치

        bool time_press = false;//시간 드레그 변수


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
            //마우스 클릭 시간, 볼륨 조절
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                if (e.GetPosition((IInputElement)sender).Y > (mediaMain.ActualHeight - 80))//시간 조절
                {
                    time_press = true;
                }
                else if(e.GetPosition((IInputElement)sender).X > mediaMain.ActualWidth -100)//볼륨 조절
                {
                    volume_press = true;

                    start_volume = e.GetPosition((IInputElement)sender).Y;
                }
            }

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

                double volume_set = ((pos.Y - start_volume) / (mediaMain.ActualHeight - 80)) / 15;


                volume_bar.Value = (int)(volume_bar.Value - volume_set * 100);
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


        private void volume_bar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volume_bar.Value == 0)
            {
                btnVol.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\VolX.png")));
            }
            else
            {
                btnVol.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"..\..\Images\Vol.png")));
            }
        }

    }
}
