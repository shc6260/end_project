using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
// 이 아래로 추가한 네임스페이스들
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Data;

namespace get_file
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        } //체크박스 체크유무
        bool ch = false;
        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            ch = true;
        }
        private void checkBox1_UnChecked(object sender, RoutedEventArgs e)
        {
            ch = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); // 새로운 폴더 선택 Dialog 를 생성합니다.
            dialog.IsFolderPicker = true; //
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) // 폴더 선택이 정상적으로 되면 아래 코드를 실행합니다.
            { 
                label2.Content = dialog.FileName; // 선택한 폴더 이름을 label2에 출력합니다.
                DataTable dt_filelistinfo = GetFileListFromFolderPath(dialog.FileName); 
                ShowDataFromDataTableToDataGrid(dt_filelistinfo, DataGrid1); 
            }
        }

        /// <summary> 
        /// 선택한 폴더의 파일 목록을 DataTable형식으로 내보냅니다. 
        /// </summary> 
        /// <param name="FolderName">선택한 폴더의 전체 경로를 입력합니다.</param> 
        /// <returns></returns> 
        private DataTable GetFileListFromFolderPath(string FolderName)
        {
            DirectoryInfo di = new DirectoryInfo(FolderName); // 해당 폴더 정보를 가져옵니다. 
            DataTable dt1 = new DataTable(); // 새로운 테이블 작성합니다.(FileInfo 에서 가져오기 원하는 속성을 열로 추가합니다.) 
            dt1.Columns.Add("Folder", typeof(string)); // 파일의 폴더 
            dt1.Columns.Add("FileName", typeof(string)); // 파일 이름(확장자 포함) 
            dt1.Columns.Add("Extension", typeof(string)); // 확장자 
            dt1.Columns.Add("CreationTime", typeof(DateTime)); // 생성 일자 
            dt1.Columns.Add("LastWriteTime", typeof(DateTime)); // 마지막 수정 일자 
            dt1.Columns.Add("LastAccessTime", typeof(DateTime)); // 마지막 접근 일자 
            dt1.Columns.Add("Lenth", typeof(long)); //파일의 크기

            foreach (FileInfo File in di.GetFiles()) // 선택 폴더의 파일 목록을 스캔합니다. 
            {
                dt1.Rows.Add(File.DirectoryName, File.Name, File.Extension, File.CreationTime, File.LastWriteTime, File.LastAccessTime, File.Length); // 개별 파일 별로 정보를 추가합니다. 
            }
            if (ch == true) // 하위 폴더 포함될 경우 
            {
                DirectoryInfo[] di_sub = di.GetDirectories(); // 하위 폴더 목록들의 정보 가져옵니다. 
                foreach (DirectoryInfo di1 in di_sub) // 하위 폴더목록을 스캔합니다. 
                {
                    foreach (FileInfo File in di1.GetFiles()) // 선택 폴더의 파일 목록을 스캔합니다. 
                    {
                        dt1.Rows.Add(File.DirectoryName, File.Name, File.Extension, File.CreationTime, File.LastWriteTime, File.LastAccessTime, File.Length); // 개별 파일 별로 정보를 추가합니다. 
                    }
                }
            }
            return dt1;
        }


        private void ShowDataFromDataTableToDataGrid(DataTable dt1, DataGrid dgv1)
        {
            //xaml에서 미리 Binding Path를 해두어야합니다.
            dgv1.ItemsSource = dt1.DefaultView;
        }
    }
}
