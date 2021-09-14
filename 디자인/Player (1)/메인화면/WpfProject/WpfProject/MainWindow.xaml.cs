using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfProject
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        bool sldrDragStart = false;
        public MainWindow() {
            InitializeComponent();
        }

        // 미디어파일 타임 핸들러
        // 미디어파일의 실행시간이 변경되면 호출된다.
        void TimerTickHandler(object sender, EventArgs e) {
            // 미디어파일 실행시간이 변경되었을 때 사용자가 임의로 변경하는 중인지를 체크한다.
            if (sldrDragStart)
                return;

            if (mediaMain.Source == null
                || !mediaMain.NaturalDuration.HasTimeSpan) {
                lblPlayTime.Content = "No file selected...";
                return;
            }

            // 미디어 파일 총 시간을 슬라이더와 동기화한다.
            sldrPlayTime.Value = mediaMain.Position.TotalSeconds;
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void sldrPlayTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            sldrDragStart = true;
            mediaMain.Pause();
        }

        private void sldrPlayTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            // 사용자가 지정한 시간대로 이동하면, 이동한 시간대로 값을 지정한다.
            mediaMain.Position = TimeSpan.FromSeconds(sldrPlayTime.Value);

            // 멈췄던 미디어를 재실행한다.
            mediaMain.Play();
            sldrDragStart = false;
        }
        private void sldrPlayTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (mediaMain.Source == null)
                return;

            // 플레이시간이 변경되면, 표시영역을 업데이트한다.
            lblPlayTime.Content = String.Format("{0} / {1}", mediaMain.Position.ToString(@"mm\:ss"), mediaMain.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            if (mediaMain.Source == null)
                return;

            mediaMain.Play();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e) {
            if (mediaMain.Source == null)
                return;

            mediaMain.Stop();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e) {
            if (mediaMain.Source == null)
                return;

            mediaMain.Pause();
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e) {
            // Win32 DLL 을 사용하여 선택할 파일 다이얼로그를 실행한다.
            OpenFileDialog dlg = new OpenFileDialog() {
                DefaultExt = ".avi",
                Filter = "All files (*.*)|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() == true) {
                // 선택한 파일을 Media Element에 지정하고 초기화한다.
                mediaMain.Source = new Uri(dlg.FileName);
                mediaMain.Volume = 0.5;
                mediaMain.SpeedRatio = 1;

                // 동영상 파일의 Timespan 제어를 위해 초기화와 이벤트처리기를 추가한다.
                DispatcherTimer timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += TimerTickHandler;
                timer.Start();


                // 선택한 파일을 실행
                mediaMain.Play();
            }
        }

        private void mediaMain_MediaEnded(object sender, RoutedEventArgs e) {
            // 미디어 중지
            mediaMain.Stop();
        }

        private void mediaMain_MediaOpened(object sender, RoutedEventArgs e) {
            // 미디어 파일이 열리면, 플레이타임 슬라이더의 값을 초기화 한다.
            sldrPlayTime.Minimum = 0;
            sldrPlayTime.Maximum = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void mediaMain_MediaFailed(object sender, ExceptionRoutedEventArgs e) {
            // 미디어 파일 실행 오류시
            MessageBox.Show("동영상 재생 실패 : " + e.ErrorException.Message.ToString());
        }

        private void mediaMain_MouseDown(object sender, MouseButtonEventArgs e) {
            Point pos = e.GetPosition((IInputElement)sender);

            double x = pos.X;
            double l = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;

            double A = l * (x / mediaMain.Width);

            //pLabel.Content = "클릭";

            mediaMain.Position = TimeSpan.FromSeconds(A);
        }

        private void mediaMain_MouseUp(object sender, MouseButtonEventArgs e) {
            //pLabel.Content = "";
        }


    }
}
