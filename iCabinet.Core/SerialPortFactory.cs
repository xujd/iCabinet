using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace iCabinet.Core
{
    public class SerialPortFactory
    {
        // 串口实例
        private SerialPort sp = null;
        private SpConfig config = null;
        private DispatcherTimer timer = null;
        private List<byte> recvBuffer = new List<byte>();

        public event DataReceivedEventHandler DataReceived;
        public event SerialErrorReceivedEventHandler Error;

        public bool IsOpen
        {
            get
            {
                return this.sp != null && this.sp.IsOpen;
            }
        }

        public SerialPortFactory()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(500);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 超出数据接收间隔
            timer.Stop();

            // 解析数据
            var tempData = new byte[recvBuffer.Count];
            recvBuffer.CopyTo(tempData);
            recvBuffer.Clear();

            //字符转换
            var tempStrs = new List<string>();
            for(var i = 0; i < tempData.Length; i++)
            {
                tempStrs.Add(tempData[i].ToString("X2"));
            }
            string readString = string.Join(" ", tempStrs);
            Log.WriteLog("INFO-" + this.config.PortName + "：收到内容-" + readString);
            //触发整条记录的处理
            DataReceived?.Invoke(new DataReceivedEventArgs(readString));
        }

        public string Open(SpConfig config)
        {
            this.config = config;
            if (!SerialPort.GetPortNames().Contains(config.PortName))
            {
                var err = "ERROR-" + config.PortName + "：端口不存在！";
                Log.WriteLog(err);
                return err;
            }
            // 创建新串口
            try
            {
                if (this.sp != null && this.sp.IsOpen) // 如果打开状态，则先关闭一下
                {
                    this.sp.DiscardInBuffer();
                    this.sp.DiscardOutBuffer();

                    this.sp.Close();
                    // 注销事件订阅
                    this.sp.DataReceived -= Sp_DataReceived;
                    this.sp.ErrorReceived -= Sp_ErrorReceived;
                }

                this.sp = new SerialPort();
                this.sp.DtrEnable = true;
                this.sp.RtsEnable = true;
                //设置数据读取超时为1秒
                this.sp.ReadTimeout = 1000;
                this.sp.DataReceived += Sp_DataReceived;
                this.sp.ErrorReceived += Sp_ErrorReceived;

                //设置串口号
                this.sp.PortName = config.PortName;
                //设置各“串口设置”

                this.sp.BaudRate = config.BaudRate;       //波特率
                if (config.DataBits > 0)
                {
                    this.sp.DataBits = config.DataBits;       //数据位
                }
                this.sp.StopBits = config.StopBits;       //停止位
                this.sp.Parity = config.Parity;             //校验位

                this.sp.Open();     //打开串口

                Log.WriteLog("INFO-" + config.PortName + "：打开串口成功！");
            }
            catch (System.Exception ex)
            {
                Log.WriteLog("ERROR-" + config.PortName + "：" + ex.Message);
            }

            return "";
        }

        private void Sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Log.WriteLog("ERROR-" + this.config.PortName + "：" + e.EventType.ToString());
            Error?.Invoke(sender, e);
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (this.sp.BytesToRead > 0)
            {
                byte[] readBuffer = new byte[this.sp.ReadBufferSize + 1];
                int count = this.sp.Read(readBuffer, 0, this.sp.ReadBufferSize);
                this.recvBuffer.AddRange(readBuffer.Take(count));
            }

            // 启动定时器，检测数据接收
            if (timer.IsEnabled) // 重置定时器
            {
                timer.Stop();
            }
            timer.Start();
        }

        public void WriteStr(string data)
        {
            if (sp != null)
            {
                if (!sp.IsOpen) sp.Open();
                
                sp.Write(data);
            }
            else
            {
                Log.WriteLog("ERROR-" + this.config.PortName + "：连接未建立！");
            }
        }

        public void Write(string data)
        {
            if (sp != null)
            {
                if (!sp.IsOpen) sp.Open();

                string[] ss = data.Split(' ');
                byte[] message = new byte[ss.Length];
                for (var i = 0; i < ss.Length; i++)
                {
                    message[i] = Convert.ToByte(Convert.ToInt32(ss[i], 16));
                }
                sp.Write(message, 0, message.Length);
            }
            else
            {
                Log.WriteLog("ERROR-" + this.config.PortName + "：连接未建立！");
            }
        }

        public void Write(byte[] message)
        {
            if (sp != null)
            {
                if (!sp.IsOpen) sp.Open();
                sp.Write(message, 0, message.Length);
            }
            else
            {
                Log.WriteLog("ERROR-" + this.config.PortName + "：连接未建立！");
            }
        }

        public void Write(byte[] message, int offset, int count)
        {
            if (sp != null)
            {
                if (!sp.IsOpen) sp.Open();
                sp.Write(message, offset, count);
            }
            else
            {
                Log.WriteLog("ERROR-" + this.config.PortName + "：连接未建立！");
            }
        }

        public void Close()
        {
            if (this.sp != null && this.sp.IsOpen)//如果打开状态，则先关闭一下
            {
                this.sp.Close();
                Log.WriteLog("INFO-" + this.config.PortName + "：关闭串口成功！");
                // 注销事件订阅
                this.sp.DataReceived -= Sp_DataReceived;
                this.sp.ErrorReceived -= Sp_ErrorReceived;
            }
            timer.Stop();
            timer.Tick -= Timer_Tick;
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public string data;
        public DataReceivedEventArgs(string data)
        {
            this.data = data;
        }
    }

    public delegate void DataReceivedEventHandler(DataReceivedEventArgs e);
}
