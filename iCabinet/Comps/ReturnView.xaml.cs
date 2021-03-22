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
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Controls;

namespace iCabinet.Comps
{
    /// <summary>
    /// Interaction logic for ReturnView.xaml
    /// </summary>
    public partial class ReturnView : UserControl
    {
        ObservableCollection<ListData> dataList = new ObservableCollection<ListData>();
        SerialPortFactory spCard = new SerialPortFactory();
        SerialPortFactory spCabinet = new SerialPortFactory();

        DispatcherTimer timer = new DispatcherTimer();

        string staffName = "张三";
        string rfID = "";
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);

        public ReturnView()
        {
            InitializeComponent();
            this.Unloaded += ReturnView_Unloaded;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);

            this.listBox.ItemsSource = dataList;
            spCard.DataReceived += spCard_DataReceived;
            spCard.Error += spCard_Error;
            // 打开读卡器监听
            SpConfig spConfig = new SpConfig();
            spConfig.PortName = ConfigurationManager.AppSettings["CardPort"];
            spConfig.BaudRate = 19200;  // 波特率
            spConfig.Parity = System.IO.Ports.Parity.Even; // 偶校验位
            spConfig.DataBits = 8;
            spConfig.StopBits = System.IO.Ports.StopBits.One; // 停止位
            spCard.Open(spConfig);
            // 设置读卡器为主动模式
            string msg = "02 06 00 00 00 03 C9 F8";
            spCard.Write(msg);

            spCabinet.DataReceived += SpCabinet_DataReceived;
            spCabinet.Error += SpCabinet_Error;

            this.GetData();

            string[] cameras = MultimediaUtil.VideoInputNames;//获取摄像头
            if (cameras.Length > 0)
            {
                this.videoPlayer.VideoCaptureSource = cameras[0];
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.spCabinet.Write(timer.Tag.ToString()); // 开：80 01 00 00 01 33 B3, 关：80 01 00 00 00 33 B2
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

            timer.Stop();
            timer.Tick -= Timer_Tick;
        }

        private void spCard_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            var msg = "读卡失败！请重试。";
            this.ShowMessageInfo(msg, this.redBrush);
            Log.WriteLog("ERROR：" + msg);
        }

        private void spCard_DataReceived(DataReceivedEventArgs e)
        {
            if (e.data == "02 06 00 00 00 03 C9 F8") // 设置主动发送回复
            {
                Log.WriteLog("INFO：设置读卡器主动发送成功！");
            }
            else
            {
                // 读卡成功，获取RFID
                if (!string.IsNullOrEmpty(e.data) && e.data.Split(' ').Length > 12)
                {
                    var rfID = Convert.ToInt64(string.Join("", e.data.Split(' ').Skip(5).Take(5)), 16).ToString();
                    this.rfID = rfID;
                    var msg = string.Format("检测到{0}归还请求。", rfID);
                    Log.WriteLog("INFO：" + msg);
                    // 查询存放位置
                    this.GetCabinetByID(rfID);
                }
            }
        }

        private async void GetCabinetByID(string rfID)
        {
            var msg1 = string.Format("正在获取{0}存放位置信息...", rfID);
            this.ShowMessageInfo(msg1, this.greenBrush);

            var data = await Service.GetSlingGrid(rfID);
            if (data != null && data[0] != "")
            {
                OpenCabinet(data[0], data[1]);
            }
            else
            {
                var msg = string.Format("未找到{0}存放位置信息！", rfID);
                this.ShowMessageInfo(msg, this.redBrush);
                Log.WriteLog("ERROR：" + msg);
            }
        }

        private void SpCabinet_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            var msg = "智能锁通信失败！请重试。";
            this.ShowMessageInfo(msg, this.redBrush);
            Log.WriteLog("ERROR：" + msg);
        }

        private void SpCabinet_DataReceived(DataReceivedEventArgs e)
        {
            var temps = e.data.Split(' ');
            if (temps.Length == 5 && temps[0] == "8A")
            {
                if (temps[3] == "11")
                {
                    var msginfo = string.Format("{0}号柜门已打开！请放入设备，并关闭柜门。", Convert.ToInt32(temps[2], 16));
                    this.ShowMessageInfo(msginfo, this.greenBrush);
                    // 启动轮询定时器查询锁状态
                    if (timer.IsEnabled)
                    {
                        timer.Stop();
                    }
                    var msg = string.Format("80 01 {0} 33", temps[2]);
                    timer.Tag = string.Format("{0} {1}", msg, BCC.CheckXOR(msg));
                    timer.Start();
                }
                else
                {
                    var msg = string.Format("{0}号柜门打开失败，请重试！", Convert.ToInt32(temps[2], 16));
                    this.ShowMessageInfo(msg, this.redBrush);
                    Log.WriteLog("ERROR：" + msg);
                }
            }
            else if (temps.Length == 5 && temps[0] == "80")
            {
                if (temps[4] == "01")
                {
                    // MessageBox.Show(string.Format("{0}号柜门已打开！", Convert.ToInt32(temps[1], 16)));
                }
                else if (temps[4] == "00")
                {
                    this.timer.Stop();
                    Log.WriteLog(string.Format("{0}号柜门已关闭！操作人员：{1}", Convert.ToInt32(temps[1], 16), this.staffName));

                    this.ReturnSling();
                    
                    var msg= string.Format("{0}号柜门已关闭！", Convert.ToInt32(temps[1], 16));
                    this.ShowMessageInfo(msg, this.greenBrush);
                }
            }
        }

        private void ShowMessageInfo(string msg, Brush brush)
        {

            this.tbInfo.Foreground = brush;
            this.tbInfo.Text = msg;
        }

        private async void ReturnSling()
        {
            var flag = await Service.ReturnSling(this.staffName, this.rfID);
            if (flag)
            {
                Log.WriteLog(string.Format("INFO：归还记录入库成功！归还人员：{0}-RFID：{1}", this.staffName, this.rfID));
                // 重新获取列表
                this.GetData();
            }
            else
            {
                Log.WriteLog(string.Format("INFO：归还记录入库失败，请检查日志！归还人员：{0}-RFID：{1}", this.staffName, this.rfID));
            }
        }

        private void OpenCabinet(string cabinetGrid, string cabinetId)
        {
            // 发送开锁信号，通知柜门开锁
            if (spCabinet.IsOpen) // 已打开，直接发送消息
            {
                var msg = string.Format("8A 01 {0} 11", Convert.ToInt32(cabinetGrid).ToString("X2"));
                spCabinet.Write(string.Format("{0} {1}", msg, BCC.CheckXOR(msg)));
            }
            else // 未打开，先打开端口
            {
                SpConfig spConfig = new SpConfig();
                spConfig.PortName = ConfigurationManager.AppSettings["CabinetPort"];
                spConfig.BaudRate = 9600;  // 波特率
                spConfig.Parity = System.IO.Ports.Parity.None; // 偶校验位
                spConfig.DataBits = 8;
                spConfig.StopBits = System.IO.Ports.StopBits.One; // 停止位
                // 打开
                spCabinet.Open(spConfig);
                // 发开锁消息
                var msg = string.Format("8A 01 {0} 11", Convert.ToInt32(cabinetGrid).ToString("X2"));
                spCabinet.Write(string.Format("{0} {1}", msg, BCC.CheckXOR(msg)));
            }
        }

        private async void GetData()
        {
            dataList.Clear();

            var list = await Service.GetBorrowedSlingsByStaff(this.staffName);
            var index = 1;
            list.ForEach(item =>
            {
                dataList.Add(new ListData() { Index = index++, Data = item });
            });
            this.loading.Visibility = Visibility.Collapsed;
        }
    }
}
