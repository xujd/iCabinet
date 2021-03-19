using iCabinet.Core;
using iCabinet.Models;
using iCabinet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFMediaKit.DirectShow.Controls;

namespace iCabinet.Comps
{
    /// <summary>
    /// Interaction logic for BorrowView.xaml
    /// </summary>
    public partial class BorrowView : UserControl
    {
        ObservableCollection<ListData> dataList = new ObservableCollection<ListData>();
        SerialPortFactory spFactory = new SerialPortFactory();

        public BorrowView()
        {
            InitializeComponent();

            this.Unloaded += BorrowView_Unloaded;
            this.listBox.ItemsSource = dataList;
            spFactory.DataReceived += SpFactory_DataReceived;
            spFactory.Error += SpFactory_Error;

            this.GetData();

            string[] cameras = MultimediaUtil.VideoInputNames; // 获取摄像头
            if (cameras.Length > 0)
            {
                this.videoPlayer.VideoCaptureSource = cameras[0];
            }
        }
        

        private void BorrowView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.videoPlayer.Close();

            this.spFactory.Close();
            spFactory.DataReceived -= SpFactory_DataReceived;
            spFactory.Error -= SpFactory_Error;
        }

        private void SpFactory_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("开锁失败！请重试。");
        }

        private void SpFactory_DataReceived(DataReceivedEventArgs e)
        {
            MessageBox.Show("开锁成功！");
        }

        private async void GetData()
        {
            var list = await Service.GetBorrowingSlingsByStaff("张三");
            list.ForEach(item =>
            {
                dataList.Add(new ListData() { Index = 1, Data = item});
                var new1 = item.Clone();
                dataList.Add(new ListData() { Index = 2, Data = new1 });
                var new2 = item.Clone();
                dataList.Add(new ListData() { Index = 3, Data = new2 });
            });
            this.loading.Visibility = Visibility.Collapsed;
        }

        private void SaveImage ()
        {
            // 调用默认摄像头
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)videoPlayer.ActualWidth, (int)videoPlayer.ActualHeight, 96, 96, PixelFormats.Default);
            bmp.Render(videoPlayer);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            // 命名格式
            string now = DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second;
            // 保存路径D盘根目录
            string filename = "D:\\" + now + ".jpg";
            FileStream fstream = new FileStream(filename, FileMode.Create);
            encoder.Save(fstream);
            fstream.Close();
        }
        
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var resSling = (sender as FrameworkElement).Tag as ResSling;
            // 发送开锁信号，通知柜门开锁
            if (spFactory.IsOpen) // 已打开，直接发送消息
            {
                spFactory.Write("hello");
            }
            else // 未打开，先打开端口
            {
                SpConfig spConfig = new SpConfig();
                spConfig.PortName = ConfigurationManager.AppSettings["CabinetPort"];
                // 打开
                spFactory.Open(spConfig);
                // 发消息
                spFactory.Write("hello");
            }
        }
    }
}
