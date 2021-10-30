using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// StartWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StartWindow : Window
    {
        PlayListForm playListForm = null;
        MainWindow mainWindow = null;
        List<String[]> videoList = new List<string[]>();

        public StartWindow(PlayListForm playListForm, MainWindow mainWindow)
        {
            InitializeComponent();
            this.playListForm = playListForm;
            this.mainWindow = mainWindow;

            //화면 중앙으로
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            double windowWidth = this.Width;

            double windowHeight = this.Height;

            this.Left = (screenWidth / 2) - (windowWidth / 2);

            this.Top = (screenHeight / 2) - (windowHeight / 2);
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
            mainWindow.Close();
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); // 새로운 폴더 선택 Dialog 를 생성합니다.

            dialog.IsFolderPicker = true; //
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) // 폴더 선택이 정상적으로 되면 아래 코드를 실행합니다.
            {
                GetFileListFromFolderPath(dialog.FileName);

                playListForm = new PlayListForm(videoList, mainWindow, dialog.FileName);
                playListForm.Top = mainWindow.Top;
                playListForm.Left = mainWindow.Left - playListForm.Width;
                mainWindow._playListForm = playListForm;
                playListForm.Show();
            }

            this.Close();
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
    }
}
