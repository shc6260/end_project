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
        double playtime;
        int JumpTime;
        //영상이 정지상태인지 재생상태인지 확인
        //0 상태일때는 재생, 1상태일때는 정지
        int p;

        //받아올 값 -> 진행된 시간:a / 현재 볼륨크기:b / 틀어져있던 영상:u / 점프타임:j / 영상정지상태유무:p
        public fill_screen(double playtime, double volume, Uri u , int j , int p)
        {
            InitializeComponent();
            mediaMain.Source = u;
            mediaMain.Volume = volume;
            mediaMain.SpeedRatio = 1;
            mediaMain.Width = this.Width;
            mediaMain.Height = this.Height;
            this.playtime = playtime;
            this.p = p;

            //점프타임 받아오기
            JumpTime = j;


            mediaMain.Play();
        }

        public int get_p()
        {
            return p;
        }


        private void mediaMain_MediaEnded(object sender, RoutedEventArgs e)
        {
            // 미디어 중지
            mediaMain.Stop();
        }

        private void mediaMain_MediaOpened(object sender, RoutedEventArgs e)
        {
            mediaMain.Position = TimeSpan.FromSeconds(playtime);
            //전체화면으로 전환 이전에 영상이 정지상태였다면 일시정지
            mediaMain.Play();
            Thread.Sleep(1);
            if (p == 1)
            {
                mediaMain.Pause();
            }
        }

        private void mediaMain_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // 미디어 파일 실행 오류시
            MessageBox.Show("동영상 재생 실패 : " + e.ErrorException.Message.ToString());
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

        //키보드 이벤트
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //esc 키다운
            if (e.Key == Key.Escape)
            {
                this.DialogResult = true;
                Window.GetWindow(this).Close();
            }
            if (e.Key == Key.Space)
            {
                mediaMain.Pause();
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
                mediaMain.Volume += 0.1;
            }
            //하단 키다운
            if (e.Key == Key.Down)
            {
                mediaMain.Volume -= 0.1;
            }
            //스페이스바 - 일시정지 or 이어재생
            if (e.Key == Key.Space)
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
    }
}
