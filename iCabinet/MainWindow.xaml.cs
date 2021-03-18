using iCabinet.Comps;
using iCabinet.Core;
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

namespace iCabinet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;

            Log.WriteLog("程序启动");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as System.Windows.Controls.MenuItem).Tag.ToString();
            switch (tag)
            {
                case "Setting":
                    (new Setting()).ShowDialog();
                    break;
                case "About":
                    (new About()).ShowDialog();
                    break;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //this.WindowState = System.Windows.WindowState.Normal;
            //this.WindowStyle = System.Windows.WindowStyle.None;
            //this.ResizeMode = System.Windows.ResizeMode.NoResize;
            //this.Topmost = true;

            //this.Left = 0.0;
            //this.Top = 0.0;
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            switch (tag)
            {
                case "Borrow":
                    this.actionGrid.Visibility = Visibility.Collapsed;
                    this.contentGrid.Visibility = Visibility.Visible;
                    this.contentGrid.Children.Add(new BorrowView());
                    break;
                case "Return":
                    this.actionGrid.Visibility = Visibility.Collapsed;
                    this.contentGrid.Visibility = Visibility.Visible;
                    this.contentGrid.Children.Add(new ReturnView());
                    break;
                case "Back":
                    if (this.contentGrid.Children.Count > 1)
                    {
                        this.contentGrid.Children.RemoveAt(1);
                    }
                    this.contentGrid.Visibility = Visibility.Collapsed;
                    this.actionGrid.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.actionGrid.Visibility = Visibility.Collapsed;
            //this.contentGrid.Visibility = Visibility.Visible;
            //this.contentGrid.Children.Add(new SettingView());
            (new About()).ShowDialog();
        }
    }
}
