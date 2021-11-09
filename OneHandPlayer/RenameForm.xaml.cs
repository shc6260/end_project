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
    /// RenameForm.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RenameForm : Window
    {
        PlayListForm playListForm;

        public RenameForm(PlayListForm playListForm)
        {
            InitializeComponent();
            this.playListForm = playListForm;

            renameTxt.Text = playListForm.reName;
            playListForm.reName = "";
            extenLbl.Content = playListForm.extension;
        }

        private void noBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            playListForm.reName = renameTxt.Text;
            this.Close();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed ) //드래그 이벤트
            {
                DragMove();
            }
        }
    }
}
