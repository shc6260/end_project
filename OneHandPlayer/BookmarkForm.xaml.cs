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
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// BookmarkForm.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BookmarkForm : Window
    {
        MainWindow MainWindow;
        Memo memo = null;
        List<BookMark> bookmarks = null;
        string[] times = new string[10];
        Uri uri = null;
        double min_time;
        double sec_time;
        double total_time;
        string[] set_times = new string[2]; 
        public BookmarkForm(MainWindow mainWindow)
        {
            InitializeComponent();
            this.ShowInTaskbar = false;//작업표시줄에 표시 안함
            this.MainWindow = mainWindow; 
        }

        public void BookMarkSet(Uri uri)
        {
            this.uri = uri;
            memo = new Memo();
            times = memo.bookMark_Output(uri);
            bookmarks = new List<BookMark>();
            for (int i = 0; i < times.Length ; i++)
            {
                if (times[i] == null)
                {
                    break;
                }
                bookmarks.Add(new BookMark() { time = times[i] });

            }
            bookmarkCountLbl.Content = bookmarks.Count() + " / 10";//북마크 갯수 표시 하단바
            bookmarks = bookmarks.OrderBy(x => x.time).ToList();
            bookMark_show.ItemsSource = bookmarks;
            memo = null;
        }

        private void add_btn_Click(object sender, RoutedEventArgs e)
        {
            memo = new Memo();
            memo.bookMark_Inut(uri, MainWindow.mediaMain.Position.ToString(@"mm\:ss"));
            if (memo.overlap == false)
            {
                bookmarks.Add(new BookMark() { time = MainWindow.mediaMain.Position.ToString(@"mm\:ss") });
                
            }
            bookmarkCountLbl.Content = bookmarks.Count() + " / 10";//북마크 갯수 표시 하단바
            bookmarks = bookmarks.OrderBy(x => x.time).ToList();
            bookMark_show.ItemsSource = bookmarks;
            bookMark_show.Items.Refresh();
            memo = null;
        }

        private void delete_btn_Click(object sender, RoutedEventArgs e)
        {
            memo = new Memo();
            foreach (BookMark item in bookMark_show.SelectedItems)
            {
                bookmarks.Remove(item);
                memo.bookMark_Delete(uri, item.time);
            }
            bookmarkCountLbl.Content = bookmarks.Count() + " / 10";//북마크 갯수 표시 하단바
            bookMark_show.Items.Refresh();
            memo = null;
        }
        private void bookMark_show_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                foreach (BookMark item in bookMark_show.SelectedItems)
                {
                    set_times = item.time.Split(':');
                    min_time = Convert.ToDouble(set_times[0]) * 60;
                    sec_time = Convert.ToDouble(set_times[1]);
                    total_time = min_time + sec_time;
                    MainWindow.mediaMain.Position = TimeSpan.FromSeconds(total_time);
                }
            }
        }
        public class BookMark { public string time { get; set; } }
    }
}
