using iCabinet.Comps;
using iCabinet.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Threading;

namespace iCabinet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            this.tbTitle.Text = ConfigurationManager.AppSettings["SysTitle"];

            Log.WriteLog("程序启动。");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.tbTime.Text = DateTime.Now.ToString();
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
#if !DEBUG
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
#endif
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();

            if (this.contentGrid.Children.Count > 1)
            {
                (this.contentGrid.Children[1] as ICompView).CleanUp();
                this.contentGrid.Children.RemoveAt(1);
            }
            e.Cancel = false;
            Log.WriteLog("程序退出。");
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
                case "Store":
                    this.actionGrid.Visibility = Visibility.Collapsed;
                    this.contentGrid.Visibility = Visibility.Visible;
                    this.contentGrid.Children.Add(new StoreView());
                    break;
                case "Back":
                    if (this.contentGrid.Children.Count > 1)
                    {
                        if ((this.contentGrid.Children[1] as ICompView).CheckOpen())
                        {
                            MessageBoxResult result = System.Windows.MessageBox.Show("存在未关闭的柜门，请确认是否已经全部关闭！", "确认？", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                            //关闭窗口
                            if (result == MessageBoxResult.OK)
                            {
                                (this.contentGrid.Children[1] as ICompView).CleanUp();
                                this.contentGrid.Children.RemoveAt(1);
                                this.contentGrid.Visibility = Visibility.Collapsed;
                                this.actionGrid.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            (this.contentGrid.Children[1] as ICompView).CleanUp();
                            this.contentGrid.Children.RemoveAt(1);
                            this.contentGrid.Visibility = Visibility.Collapsed;
                            this.actionGrid.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        this.contentGrid.Visibility = Visibility.Collapsed;
                        this.actionGrid.Visibility = Visibility.Visible;
                    }
                    break;
                default:
                    MessageBox.Show("功能未开放。", "提示");
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
