using iCabinet.Core;
using iCabinet.Models;
using iCabinet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WPFMediaKit.DirectShow.Controls;

namespace iCabinet.Comps
{
    /// <summary>
    /// Interaction logic for BorrowView.xaml
    /// </summary>
    public partial class BorrowView : UserControl
    {
        ObservableCollection<ResSling> dataList = new ObservableCollection<ResSling>();
        SerialPortFactory spFactory = new SerialPortFactory();

        public BorrowView()
        {
            InitializeComponent();

            this.Unloaded += BorrowView_Unloaded;
            this.dataGrid.ItemsSource = dataList;
            spFactory.DataReceived += SpFactory_DataReceived;
            spFactory.Error += SpFactory_Error;

            this.GetData();

            string[] cameras = MultimediaUtil.VideoInputNames;//获取摄像头
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
                dataList.Add(item);
            });
            this.loading.Visibility = Visibility.Collapsed;
        }



        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var resSling = (sender as StackPanel).Tag as ResSling;
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
