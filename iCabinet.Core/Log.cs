using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCabinet.Core
{
    public class Log
    {
        public static List<string> LogList = null;
        static Log()
        {
            LogList = new List<string>();
        }
        
        public static void WriteLog(string txt, string subdir = @"\log\", bool flag = false)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + subdir + DateTime.Now.ToString("yyyy-MM") + @"\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path += DateTime.Now.ToString("yyyyMMdd") + ".txt";
                if (!File.Exists(path))
                {
                    File.Create(path);
                }

                string log = DateTime.Now.ToString("HH:mm:ss") + " " + txt;

                FileStream fs;
                StreamWriter sw;
                fs = new FileStream(path, FileMode.Append);
                sw = new StreamWriter(fs, Encoding.Default);
                sw.Write(log + "\r\n");
                sw.Close();
                sw.Dispose();
                fs.Close();
                fs.Dispose();

                LogList.Insert(0, log);
                if (LogList.Count > 150)//保留最新100条记录
                {
                    LogList.RemoveRange(100, 40);
                }
            }
            catch (Exception e)
            {
                if (!flag)
                {
                    WriteLog("程序发生异常（WriteLog）。详情：" + e.Message, @"\log\", true);
                }
            }
        }
    }
}
