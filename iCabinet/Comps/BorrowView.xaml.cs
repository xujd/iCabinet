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
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Controls;

namespace iCabinet.Comps
{
    /// <summary>
    /// Interaction logic for BorrowView.xaml
    /// </summary>
    public partial class BorrowView : UserControl
    {
        ObservableCollection<ListData> dataList = new ObservableCollection<ListData>();
        SerialPortFactory spCabinet = new SerialPortFactory();
        string staffName = "张三";
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        DispatcherTimer timer = new DispatcherTimer();
        ResSling curSling = null;

        public BorrowView()
        {
            InitializeComponent();

            this.Unloaded += BorrowView_Unloaded;
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);

            this.listBox.ItemsSource = dataList;
            spCabinet.DataReceived += SpFactory_DataReceived;
            spCabinet.Error += SpFactory_Error;
            // 获取登记数据
            this.GetData();

            string[] cameras = MultimediaUtil.VideoInputNames; // 获取摄像头
            if (cameras.Length > 0)
            {
                this.videoPlayer.VideoCaptureSource = cameras[0];
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            this.spCabinet.Write(timer.Tag.ToString()); // 开：80 01 00 00 01 33 B3, 关：80 01 00 00 00 33 B2
        }

        private void BorrowView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.videoPlayer.Close();

            this.spCabinet.Close();
            spCabinet.DataReceived -= SpFactory_DataReceived;
            spCabinet.Error -= SpFactory_Error;

            timer.Stop();
            timer.Tick -= Timer_Tick;
        }

        private void SpFactory_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            var msg = "智能锁通信失败！请重试。";
            this.ShowMessageInfo(msg, this.redBrush);
            Log.WriteLog("ERROR：" + msg);
        }

        private void SpFactory_DataReceived(DataReceivedEventArgs e)
        {
            var temps = e.data.Split(' ');
            if (temps.Length == 5 && temps[0] == "8A")
            {
                if (temps[3] == "11")
                {
                    var msginfo = string.Format("{0}号柜门已打开！请取走设备，并关闭柜门。", Convert.ToInt32(temps[2], 16));
                    this.ShowMessageInfo(msginfo, this.greenBrush);
                    Log.WriteLog("INFO：" + msginfo);
                    // 只要打开柜门，就记录设备已取走
                    this.TakeSling();
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
                    timer.Stop();
                    Log.WriteLog(string.Format("{0}号柜门已关闭！操作人员：{1}", Convert.ToInt32(temps[1], 16), this.staffName));
                    var msg = string.Format("{0}号柜门已关闭！", Convert.ToInt32(temps[1], 16));
                    this.ShowMessageInfo(msg, this.greenBrush);
                }
            }
        }

        private async void TakeSling()
        {
            var flag = await Service.TakeSling(this.staffName, this.curSling.RfID);
            if (flag)
            {
                Log.WriteLog(string.Format("INFO：取出记录入库成功！取出人员：{0}-RFID：{1}", this.staffName, this.curSling.RfID));
                // 重新获取列表
                this.GetData();
            }
            else
            {
                Log.WriteLog(string.Format("INFO：取出记录入库失败，请检查日志！取出人员：{0}-RFID：{1}", this.staffName, this.curSling.RfID));
            }

            this.curSling = null;
        }

        private void ShowMessageInfo(string msg, Brush brush)
        {
            this.tbInfo.Foreground = brush;
            this.tbInfo.Text = msg;
        }

        private async void GetData()
        {
            var list = await Service.GetBorrowingSlingsByStaff(this.staffName);
            var index = 1;
            list.ForEach(item =>
            {
                dataList.Add(new ListData() { Index = index++, Data = item });
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
            var resSling = ((sender as FrameworkElement).Tag as ListData).Data as ResSling;
            this.curSling = resSling;
            // 发送开锁信号，通知柜门开锁
            if (spCabinet.IsOpen) // 已打开，直接发送消息
            {
                var msg = string.Format("8A 01 {0} 11", Convert.ToInt32(resSling.CabinetGrid).ToString("X2"));
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
                var msg = string.Format("8A 01 {0} 11", Convert.ToInt32(resSling.CabinetGrid).ToString("X2"));
                spCabinet.Write(string.Format("{0} {1}", msg, BCC.CheckXOR(msg)));
            }
        }
    }
}
