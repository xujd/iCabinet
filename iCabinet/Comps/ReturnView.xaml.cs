using iCabinet.Core;
using iCabinet.Models;
using iCabinet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
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
    /// Interaction logic for ReturnView.xaml
    /// </summary>
    public partial class ReturnView : UserControl
    {
        ObservableCollection<ResSling> dataList = new ObservableCollection<ResSling>();
        SerialPortFactory spCard = new SerialPortFactory();
        SerialPortFactory spCabinet = new SerialPortFactory();
        public ReturnView()
        {
            InitializeComponent();
            this.Unloaded += ReturnView_Unloaded;

            this.dataGrid.ItemsSource = dataList;
            spCard.DataReceived += spCard_DataReceived;
            spCard.Error += spCard_Error;
            // 打开读卡器监听
            SpConfig spConfig = new SpConfig();
            spConfig.PortName = ConfigurationManager.AppSettings["CardPort"];
            spConfig.BaudRate = 19200;  // 波特率
            spConfig.Parity = System.IO.Ports.Parity.Even; // 偶校验位
            spConfig.StopBits = System.IO.Ports.StopBits.One; // 停止位
            spCard.Open(spConfig);
            // 设置读卡器为主动模式
            spCard.Write("02 06 00 00 00 03 C9 F8");

            spCabinet.DataReceived += SpCabinet_DataReceived;
            spCabinet.Error += SpCabinet_Error;

            this.GetData();

            string[] cameras = MultimediaUtil.VideoInputNames;//获取摄像头
            if (cameras.Length > 0)
            {
                this.videoPlayer.VideoCaptureSource = cameras[0];
            }
        }

        private void ReturnView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.videoPlayer.Close();

            this.spCard.Close();
            spCard.DataReceived -= spCard_DataReceived;
            spCard.Error -= spCard_Error;

            this.spCabinet.Close();
            spCabinet.DataReceived -= SpCabinet_DataReceived;
            spCabinet.Error -= SpCabinet_Error;
        }

        private void spCard_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("读卡失败！请重试。");
        }

        private void spCard_DataReceived(DataReceivedEventArgs e)
        {
            if (e.data == "02 06 00 00 00 03 C9 F8") // 设置主动发送回复
            {

            }
            else
            {
                // 读卡成功，获取RFID
                var rfID = "RF02";
                // 查询存放位置
                this.GetCabinetByID(rfID);
            }
        }

        private async void GetCabinetByID(string rfID)
        {
            var data = await Service.GetSlingGrid(rfID);
            if (data != null && data[0] != "")
            {
                OpenCabinet(data[0], data[1]);
            }
            else
            {
                MessageBox.Show("ERROR：未找到存放位置信息！");
            }
        }

        private void SpCabinet_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("开锁失败！请重试。");
        }

        private void SpCabinet_DataReceived(DataReceivedEventArgs e)
        {
            MessageBox.Show("开锁成功！");
        }

        private void OpenCabinet(string cabinetGrid, string cabinetId)
        {
            // 发送开锁信号，通知柜门开锁
            if (spCabinet.IsOpen) // 已打开，直接发送消息
            {
                spCabinet.Write("hello");
            }
            else // 未打开，先打开端口
            {
                SpConfig spConfig = new SpConfig();
                spConfig.PortName = ConfigurationManager.AppSettings["CabinetPort"];
                // 打开
                spCabinet.Open(spConfig);
                // 发消息
                spCabinet.Write("hello");
            }
        }

        private async void GetData()
        {
            var list = await Service.GetBorrowedSlingsByStaff("张三");
            list.ForEach(item =>
            {
                dataList.Add(item);
            });
            this.loading.Visibility = Visibility.Collapsed;
        }
    }
}
