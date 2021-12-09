using iCabinet.Core;
using iCabinet.Models;
using iCabinet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
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
    /// Interaction logic for ReturnView.xaml
    /// </summary>
    public partial class ReturnView : UserControl, ICompView
    {
        ObservableCollection<ListData> dataList = new ObservableCollection<ListData>();
        SerialPortFactory spCard = new SerialPortFactory();
        SerialPortFactory spCabinet = new SerialPortFactory();

        DispatcherTimer timer = new DispatcherTimer();

        string staffName = "";
        int staffId = 0;
        string rfID = "";
        string curRFID = "";
        int gridNo = 0;
        bool isOpening = false;
        bool isLogined = false; // 用户是否验证通过
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.LightGreen);

        FaceIDUtil faceIdUtil = null;

        Dictionary<int, string> grid2RFID = new Dictionary<int, string>();

        public ReturnView()
        {
            InitializeComponent();

            txtStaffID.AddHandler(TextBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(txtStaffID_MouseLeftButtonDown), true);

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
            var flag = spCard.Write(msg);
            if (!flag)
            {
                Log.WriteLog(string.Format("ERROR-RET：主动模式设置失败，{0}", msg));
            }

            spCabinet.DataReceived += SpCabinet_DataReceived;
            spCabinet.Error += SpCabinet_Error;

            //string[] cameras = MultimediaUtil.VideoInputNames;//获取摄像头
            //if (cameras.Length > 0)
            //{
            //    this.videoPlayer.VideoCaptureSource = cameras[0];
            //}

            new Thread(() =>
            {
                Thread.Sleep(100);
                faceIdUtil.OpenRealData();
            }).Start();
        }

        private void FaceIdUtil_FaceSearchCompleted(FaceSearchEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (e.data.Count > 0 && e.data[0].Similarity > 0.6) // 识别成功
                {
                    if (txtAction.Tag.ToString() == "FACE" || contentGrid.Visibility == Visibility.Visible) // 当前不是人脸识别状态
                    {
                        return;
                    }

                    Log.WriteLog(string.Format("INFO-BOW：人脸识别成功，识别信息--{0}-{1}。", e.data[0].Name, e.data[0].Similarity));
                    var id = -1;
                    if (!int.TryParse(e.data[0].Name, out id))
                    {
                        Log.WriteLog(string.Format("ERROR-BOW：人脸识别成功，但照片命名规则错误--{0}。", e.data[0].Name));
                        return;
                    }
                    if (this.staffId != id)
                    {
                        this.staffId = id;
                        Log.WriteLog(string.Format("INFO-BOW：人脸识别成功，匹配人员ID-{0}，相似度-{1}。", this.staffId, e.data[0].Similarity));
                        this.GetStaffData(id, true);
                    }
                }
            }));
        }

        private async void GetStaffData(int id, bool isFaceID)
        {
            // 获取员工名字
            this.staffName = await Service.GetStaffName(id);

            Log.WriteLog(string.Format("INFO-BOW：读取人员信息成功，匹配人员--{0}。", this.staffName));

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
            this.isLogined = true;
            // 查询用户数据
            this.GetData();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.spCabinet.Write(timer.Tag.ToString()); // 开：80 01 00 00 01 33 B3, 关：80 01 00 00 00 33 B2
        }
        public bool CheckOpen()
        {
            return this.isOpening;
        }
        public void CleanUp()
        {
            txtStaffID.RemoveHandler(TextBox.MouseLeftButtonDownEvent, (MouseButtonEventHandler)txtStaffID_MouseLeftButtonDown);

            this.faceIdUtil.Destroy();

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
            Log.WriteLog("ERROR-RET：" + msg);
        }

        private void spCard_DataReceived(DataReceivedEventArgs e)
        {
            if (e.data == "02 06 00 00 00 03 C9 F8") // 设置主动发送回复
            {
                Log.WriteLog("INFO-RET：设置读卡器主动发送成功！");
            }
            else
            {
                if (!isLogined) // 未登录时
                {
                    return;
                }
                if (isOpening) // 柜门未关闭。
                {
                    var msg1 = string.Format("{0}号柜门未关闭，请先关闭...", this.gridNo);
                    this.ShowMessageInfo(msg1, this.greenBrush);
                    return;
                }
                // 读卡成功，获取RFID
                if (!string.IsNullOrEmpty(e.data) && e.data.Split(' ').Length > 12)
                {
                    var rfID = "086" + Convert.ToInt64(string.Join("", e.data.Split(' ').Skip(5).Take(5)), 16).ToString().PadLeft(12, '0');
                    // 重复检测 START
                    if (this.curRFID == rfID)
                    {
                        var msg1 = string.Format("检测到{0}的5秒内归还重复请求。", rfID);
                        Log.WriteLog("INFO-RET：" + msg1);
                        return;
                    }

                    this.rfID = this.curRFID = rfID;
                    DispatcherTimer t = new DispatcherTimer();
                    t.Interval = TimeSpan.FromSeconds(5);
                    EventHandler handler = null;
                    t.Tick += handler = (s, a) =>
                    {
                        t.Tick -= handler;
                        t.Stop();
                        this.curRFID = "";
                    };
                    t.Start();
                    // 重复检测 END

                    var msg = string.Format("检测到{0}归还请求。", rfID);
                    Log.WriteLog("INFO-RET：" + msg);
                    // 查询存放位置
                    this.GetCabinetByID(rfID);
                }
            }
        }

        private async void GetCabinetByID(string rfID)
        {
            this.gridNo = 0;
            var msg1 = string.Format("正在获取{0}存放位置信息...", rfID);
            this.ShowMessageInfo(msg1, this.greenBrush);

            var data = await Service.GetSlingGrid(rfID);
            if (data != null && data[0] != "")
            {
                this.gridNo = int.Parse(data[0]);
                grid2RFID[int.Parse(data[0])] = rfID; // 记录格子对应的设备
                // 打开柜门
                OpenCabinet(data[0], data[1]);
            }
            else
            {
                var msg = string.Format("未找到{0}存放位置信息！", rfID);
                this.ShowMessageInfo(msg, this.redBrush);
                Log.WriteLog("ERROR-RET：" + msg);
            }
        }

        private void SpCabinet_Error(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            var msg = "智能锁通信失败！请重试。";
            this.ShowMessageInfo(msg, this.redBrush);
            Log.WriteLog("ERROR-RET：" + msg);
        }

        private void SpCabinet_DataReceived(DataReceivedEventArgs e)
        {
            var temps = e.data.Split(' ');
            if (temps.Length == 5 && temps[0] == "8A")
            {
                if (temps[3] == "11")
                {
                    isOpening = true;
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
                    Log.WriteLog("ERROR-RET：" + msg);
                }
            }
            else if (temps.Length > 4 && temps[0] == "80")
            {
                if (temps[3] == "01")
                {
                    // MessageBox.Show(string.Format("{0}号柜门已打开！", Convert.ToInt32(temps[1], 16)));
                }
                else if (temps[3] == "00")
                {
                    this.timer.Stop();
                    Log.WriteLog(string.Format("INFO-RET：{0}号柜门已关闭！操作人员：{1}", this.gridNo, this.staffName));

                    if (grid2RFID.ContainsKey(this.gridNo))
                    {
                        this.ReturnSling(grid2RFID[this.gridNo]);
                    }

                    var msg = string.Format("{0}号柜门已关闭！", this.gridNo);
                    isOpening = false;
                    this.ShowMessageInfo(msg, this.greenBrush);
                }
            }
        }

        private void ShowMessageInfo(string msg, Brush brush)
        {
            this.tbInfo.Foreground = brush;
            this.tbInfo.Text = msg;
        }

        private async void ReturnSling(string rfID)
        {
            var flag = await Service.ReturnSling(this.staffId, this.staffName, rfID);
            if (flag)
            {
                Log.WriteLog(string.Format("INFO-RET：归还记录入库成功！归还人员：{0}-RFID：{1}", this.staffName, rfID));
                // 重新获取列表
                this.GetData();
            }
            else
            {
                Log.WriteLog(string.Format("ERROR-RET：归还记录入库失败，请检查日志！归还人员：{0}-RFID：{1}", this.staffName, rfID));
            }
        }

        private void OpenCabinet(string cabinetGrid, string cabinetId)
        {
            // 发送开锁信号，通知柜门开锁
            if (spCabinet.IsOpen) // 已打开，直接发送消息
            {
                var msg = string.Format("8A 01 {0} 11", Convert.ToInt32(cabinetGrid).ToString("X2"));
                var flag = spCabinet.Write(string.Format("{0} {1}", msg, BCC.CheckXOR(msg)));
                if (!flag)
                {
                    Log.WriteLog(string.Format("ERROR-RET：开锁消息发送失败，{0}", msg));
                }
            }
            else // 未打开，先打开端口
            {
                SpConfig spConfig = new SpConfig();
                spConfig.PortName = ConfigurationManager.AppSettings["CabinetPort"];
                spConfig.BaudRate = 9600;  // 波特率
                spConfig.Parity = System.IO.Ports.Parity.None; // 偶校验位
                spConfig.DataBits = 8;
                spConfig.StopBits = System.IO.Ports.StopBits.One; // 停止位
                try
                {
                    // 打开
                    spCabinet.Open(spConfig);
                    // 发开锁消息
                    var msg = string.Format("8A 01 {0} 11", Convert.ToInt32(cabinetGrid).ToString("X2"));
                    var flag = spCabinet.Write(string.Format("{0} {1}", msg, BCC.CheckXOR(msg)));
                    if (!flag)
                    {
                        Log.WriteLog(string.Format("ERROR-RET：开锁消息发送失败，{0}", msg));
                    }
                }
                catch (Exception ex)
                {
                    this.ShowMessageInfo("开锁失败，请重试！", this.redBrush);
                    Log.WriteLog(string.Format("ERROR-RET：开锁失败，失败信息：{0}", ex.Message));
                }
            }
        }

        private async void GetData()
        {
            dataList.Clear();

            var list = await Service.GetBorrowedSlingsByStaff(this.staffId);
            var index = 1;
            list.ForEach(item =>
            {
                dataList.Add(new ListData() { Index = index++, Data = item });
            });
            this.imgNull.Visibility = dataList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            this.loading.Visibility = Visibility.Collapsed;
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

        private void txtStaffID_LostFocus(object sender, RoutedEventArgs e)
        {
            TabTipUtil.Close();
        }

        private void txtStaffID_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabTipUtil.Open();
        }
    }
}
