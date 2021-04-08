using iCabinet.Core;
using iCabinet.Models;
using iCabinet.Services;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace iCabinet.Comps
{
    /// <summary>
    /// Interaction logic for BorrowView.xaml
    /// </summary>
    public partial class BorrowView : UserControl, ICompView
    {
        ObservableCollection<ListData> dataList = new ObservableCollection<ListData>();
        SerialPortFactory spCabinet = new SerialPortFactory();
        string staffName = "";
        int staffId = 0;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        DispatcherTimer timer = new DispatcherTimer();
        ResSling curSling = null;

        FaceIDUtil faceIdUtil = null;
        public BorrowView()
        {
            InitializeComponent();
            
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);

            faceIdUtil = new FaceIDUtil();
            faceIdUtil.FaceSearchCompleted += FaceIdUtil_FaceSearchCompleted;

            string err = "";
            if ((err = faceIdUtil.Init()) != "")
            {
                this.tbFaceError.Text = err;
            }

            this.listBox.ItemsSource = dataList;
            spCabinet.DataReceived += SpFactory_DataReceived;
            spCabinet.Error += SpFactory_Error;

            //string[] cameras = MultimediaUtil.VideoInputNames; // 获取摄像头
            //if (cameras.Length > 0)
            //{
            //    this.videoPlayer.VideoCaptureSource = cameras[0];
            //}
            //this.videoPlayer.MediaOpened += VideoPlayer_MediaOpened;

            new Thread(() =>
            {
                Thread.Sleep(100);
                faceIdUtil.OpenRealData();
            }).Start();
        }

        private void FaceIdUtil_FaceSearchCompleted(FaceSearchEventArgs e)
        {
            if (e.data.Count > 0 && e.data[0].Similarity > 0.8) // 识别成功
            {
                if (txtAction.Tag.ToString() == "FACE" || contentGrid.Visibility == Visibility.Visible) // 当前不是人脸识别状态
                {
                    return;
                }
                var id = -1;
                if(!int.TryParse(e.data[0].Name, out id))
                {
                    Log.WriteLog(string.Format("ERROR-BOW：人脸识别成功，但照片命名规则错误--{0}。", e.data[0].Name));
                    return;
                }
                if(this.staffId != id)
                {
                    this.staffId = id;
                    Log.WriteLog(string.Format("INFO-BOW：人脸识别成功，匹配人员-{0}，相似度-{1}。", this.staffName, e.data[0].Similarity));
                    this.GetStaffData(id, true);
                }
            }
        }

        private async void GetStaffData(int id, bool isFaceID)
        {
            // 获取员工名字
            this.staffName = await Service.GetStaffName(id);
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.txtStaffID.IsEnabled = true;
                if (string.IsNullOrEmpty(this.staffName))
                {
                    this.tbFaceError.Text = "员工未登记，请联系管理员！";
                    return;
                }
                if (this.imgPhoto.Source != null)
                {
                    (this.imgPhoto.Source as BitmapImage).UriSource = null;
                    this.imgPhoto.Source = null;
                }
                this.tbHello.Text = string.Format("你好，{0}！", this.staffName);
                this.imgPhoto.Source = BmpUtil.GetBitmapImage(isFaceID ? "pack://siteoforigin:,,,/model.jpg" : "pack://application:,,,/Images/person.png");

                this.contentGrid.Visibility = Visibility.Visible;
                this.faceGrid.Visibility = Visibility.Collapsed;
                // 查询用户数据
                this.GetData();
            }));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.spCabinet.Write(timer.Tag.ToString()); // 开：80 01 00 00 01 33 B3, 关：80 01 00 00 00 33 B2
        }

        public void CleanUp()
        {
            this.spCabinet.Close();
            spCabinet.DataReceived -= SpFactory_DataReceived;
            spCabinet.Error -= SpFactory_Error;

            timer.Stop();
            timer.Tick -= Timer_Tick;

            this.faceIdUtil.Destroy();
        }

        private void SpFactory_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            var msg = "智能锁通信失败！请重试。";
            this.ShowMessageInfo(msg, this.redBrush);
            Log.WriteLog("ERROR-BOW：" + msg);
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
                    Log.WriteLog("INFO-BOW：" + msginfo);
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
                    Log.WriteLog("ERROR-BOW：" + msg);
                }
            }
            else if (temps.Length > 5 && temps[0] == "80")
            {
                if (temps[4] == "01")
                {
                    // MessageBox.Show(string.Format("{0}号柜门已打开！", Convert.ToInt32(temps[1], 16)));
                }
                else if (temps[4] == "00")
                {
                    timer.Stop();
                    Log.WriteLog(string.Format("INFO-BOW：{0}号柜门已关闭！操作人员：{1}", Convert.ToInt32(temps[1], 16), this.staffName));
                    var msg = string.Format("{0}号柜门已关闭！", Convert.ToInt32(temps[1], 16));
                    this.ShowMessageInfo(msg, this.greenBrush);
                }
            }
        }

        private async void TakeSling()
        {
            var flag = await Service.TakeSling(this.staffId, this.curSling.RfID);
            if (flag)
            {
                Log.WriteLog(string.Format("INFO-BOW：取出记录入库成功！取出人员：{0}-RFID：{1}", this.staffName, this.curSling.RfID));
                // 重新获取列表
                this.GetData();
            }
            else
            {
                Log.WriteLog(string.Format("ERROR-BOW：取出记录入库失败，请检查日志！取出人员：{0}-RFID：{1}", this.staffName, this.curSling.RfID));
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
            dataList.Clear();

            var list = await Service.GetBorrowingSlingsByStaff(this.staffId);
            var index = 1;
            list.ForEach(item =>
            {
                dataList.Add(new ListData() { Index = index++, Data = item });
            });

            this.imgNull.Visibility = dataList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            this.loading.Visibility = Visibility.Collapsed;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var resSling = ((sender as FrameworkElement).Tag as ListData).Data as ResSling;
            this.curSling = resSling;
            this.tbInfo.Text = "";

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
                string err = "";
                if((err = spCabinet.Open(spConfig)) != "")
                {
                    this.ShowMessageInfo(err, this.redBrush);
                }
                // 发开锁消息
                var msg = string.Format("8A 01 {0} 11", Convert.ToInt32(resSling.CabinetGrid).ToString("X2"));
                spCabinet.Write(string.Format("{0} {1}", msg, BCC.CheckXOR(msg)));
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int id = -1;
            if (int.TryParse(txtStaffID.Text, out id))
            {
                this.staffId = id;
                this.txtStaffID.IsEnabled = false;
                this.GetStaffData(id, false);
            }
            else
            {
                this.ShowMessageInfo("输入的工号不正确！", this.redBrush);
            }
        }

        private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (txtAction.Tag.ToString() == "ID")
            {
                txtAction.Tag = "FACE";
                txtAction.Text = "使用人脸识别";
                this.imgAction.Source = BmpUtil.GetBitmapImage("pack://application:,,,/Images/faceid.png");
                this.faceAni.Visibility = Visibility.Collapsed;
                this.spIdLogin.Visibility = Visibility.Visible;
                this.txtStaffID.IsEnabled = true;
            }
            else
            {
                txtAction.Tag = "ID";
                txtAction.Text = "使用工号登录";
                this.imgAction.Source = BmpUtil.GetBitmapImage("pack://application:,,,/Images/people.png");
                this.faceAni.Visibility = Visibility.Visible;
                this.spIdLogin.Visibility = Visibility.Collapsed;
            }
        }
    }
}
