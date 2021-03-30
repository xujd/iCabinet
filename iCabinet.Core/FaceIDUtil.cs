using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iCabinet.Core
{
    public class FaceIDUtil
    {
        string CONN_STR = "Host=127.0.0.1;Username=admin;Password=admin";
        public event FaceSearchEventHandler FaceSearchCompleted;
        uint UserID = 0;
        IntPtr pWaitFlag;

        NETSDK.SIRIUS_pfnMessageCallBack mycall = null;
        NETSDK.SIRIUS_pfnRealDataCallBack realDataCall = null;
        IntPtr IntPtrOffset(IntPtr pbase, int offset)
        {
            if (4 == IntPtr.Size)
            {
                return (IntPtr)(pbase.ToInt32() + offset);
            }
            return (IntPtr)(pbase.ToInt64() + offset);
        }

        void MessageCallBack(uint UserID, uint ulMessage, IntPtr pData, int iSize, IntPtr pUser)
        {
            switch (ulMessage)
            {
                case NETSDK.MESSAGE_FACE_MATCH:
                    {
                        var result = Marshal.PtrToStructure(pData, typeof(NETSDK.FACE_MATCH_INFO));
                        var stuFaceMatch = (NETSDK.FACE_MATCH_INFO)result;
                        Console.WriteLine("{0:s} 人脸匹配！第{1:d}路, {2:s} fSimilarity = {3:f}"
                            , stuFaceMatch.szTime
                            , stuFaceMatch.iCameraIndex + 1
                            , stuFaceMatch.stPeopleInfo.szName
                            , stuFaceMatch.fSimilarity
                            );

                        // 保存抓拍图片
                        IntPtr pPicBuffer = IntPtrOffset(pData, stuFaceMatch.iCapturePicOffset);
                        byte[] pPicBuffer2 = new byte[stuFaceMatch.iCapturePicSize];
                        Marshal.Copy(pPicBuffer, pPicBuffer2, 0, stuFaceMatch.iCapturePicSize);
                        FileStream fs = new FileStream(stuFaceMatch.stPeopleInfo.szName + ".jpg", FileMode.OpenOrCreate);
                        BinaryWriter binWriter = new BinaryWriter(fs);
                        binWriter.Write(pPicBuffer2, 0, stuFaceMatch.iCapturePicSize);
                        binWriter.Close();
                        fs.Close();

                        // 保存匹配图片
                        pPicBuffer = IntPtrOffset(pData, stuFaceMatch.iMatchPicOffset);
                        pPicBuffer2 = new byte[stuFaceMatch.iMatchPicSize];
                        Marshal.Copy(pPicBuffer, pPicBuffer2, 0, stuFaceMatch.iMatchPicSize);
                        fs = new FileStream("model.jpg", FileMode.OpenOrCreate);
                        binWriter = new BinaryWriter(fs);
                        binWriter.Write(pPicBuffer2, 0, stuFaceMatch.iMatchPicSize);
                        binWriter.Close();
                        fs.Close();

                        var list = new List<FaceSearchData>();
                        list.Add(new FaceSearchData() { Name = stuFaceMatch.stPeopleInfo.szName, Similarity = stuFaceMatch.fSimilarity, PhotoName = "model.jpg" });
                        FaceSearchCompleted?.Invoke(new FaceSearchEventArgs(list));

                        break;
                    }
                case NETSDK.MESSAGE_FACE_CAPTURE:
                    {
                        var result = Marshal.PtrToStructure(pData, typeof(NETSDK.FACE_MATCH_INFO));
                        var stuFaceMatch = (NETSDK.FACE_MATCH_INFO)result;
                        Console.WriteLine("{0:s} 人脸抓拍！第{1:d}路, {2:s} fSimilarity = {3:f}"
                            , stuFaceMatch.szTime
                            , stuFaceMatch.iCameraIndex + 1
                            , stuFaceMatch.stPeopleInfo.szName//System.Text.Encoding.Default.GetString(stuFaceMatch.stPeopleInfo.szName).TrimEnd('\0')
                            , stuFaceMatch.fSimilarity
                            );
                        break;
                    }
                case NETSDK.MESSAGE_FACE_SEARCH:
                    {// 人脸搜索完成通知消息

                        var ItemNum = Marshal.PtrToStructure(pData, typeof(int));
                        var TotalNum = Marshal.PtrToStructure(pData + 4, typeof(int));

                        int iItemNum = Convert.ToInt32(ItemNum);
                        List<FaceSearchData> result = new List<FaceSearchData>();
                        for (int i = 0; i < iItemNum; i++)
                        {
                            var stuModelFace = (NETSDK.MODEL_FACE_ITEM_EXT)Marshal.PtrToStructure(pData + 8 + i * Marshal.SizeOf(typeof(NETSDK.MODEL_FACE_ITEM_EXT)), typeof(NETSDK.MODEL_FACE_ITEM_EXT));

                            result.Add(new FaceSearchData() { Name = stuModelFace.stPeopleInfo.szName, Similarity = stuModelFace.fSimilarity });
                            Console.WriteLine("相似度{0:f} 姓名{1:s}"
                            , stuModelFace.fSimilarity
                            , stuModelFace.stPeopleInfo.szName
                            );
                        }

                        Marshal.WriteInt32(pUser, 0);

                        var data = result.OrderByDescending(item => item.Similarity).ToList();
                        FaceSearchCompleted?.Invoke(new FaceSearchEventArgs(data));
                        break;
                    }
                case NETSDK.MESSAGE_CAMERA_ONLINE:
                    {
                        var iCameraIndex = Marshal.PtrToStructure(pData, typeof(int));
                        Console.WriteLine("通道{0:d}的设备上线", iCameraIndex);
                        break;
                    }
                case NETSDK.MESSAGE_CAMERA_OFFLINE:
                    {
                        var iCameraIndex = Marshal.PtrToStructure(pData, typeof(int));
                        Console.WriteLine("通道{0:d}的设备掉线", iCameraIndex);
                        break;
                    }
                case NETSDK.MESSAGE_SERVER_OFFLINE:
                    {
                        Console.WriteLine("注销或服务器掉线!");
                        break;
                    }
                default:
                    {
                        Console.WriteLine("未处理信息{0:d}!", ulMessage);
                        break;
                    }
            }
        }

        public int SearchByPic(string picPath)
        {
            uint lResult;

            pWaitFlag = Marshal.AllocHGlobal(sizeof(uint));//声明一个同样大小的空间
            Marshal.WriteInt32(pWaitFlag, 1);
            int iWaitFlag = Marshal.ReadInt32(pWaitFlag);

            mycall = new NETSDK.SIRIUS_pfnMessageCallBack(this.MessageCallBack);
            lResult = NETSDK.SIRIUS_SetMessageCallBack(mycall, pWaitFlag);
            if (0 != lResult)
            {
                Console.WriteLine("注册回调函数失败！错误编码:{0:d}", lResult);
                return -1;
            }
            Console.WriteLine("注册回调函数成功！");

            // 登录服务器
            NETSDK.USER_LOGIN_INFO stLoginInfo;
            stLoginInfo.szIPAddr = "127.0.0.1";
            stLoginInfo.iPort = 8612;
            stLoginInfo.szUserName = "admin";
            stLoginInfo.szPassWord = "admin";
            lResult = NETSDK.SIRIUS_Login(ref stLoginInfo, ref UserID);
            if (0 != lResult)
            {
                Console.WriteLine("登录服务器[{0:s}:{1:d}]失败！错误编码:{2:d}"
                    , stLoginInfo.szIPAddr
                    , stLoginInfo.iPort
                    , lResult
                    );
                return -1;
            }
            Console.WriteLine("登录服务器[{0:s}:{1:d}]成功！", stLoginInfo.szIPAddr, stLoginInfo.iPort);

            // 条件对象
            NETSDK.SEARCH_COND stSearchCond;
            stSearchCond.Index = 0;
            stSearchCond.PageNum = 10;
            stSearchCond.szStartTime = "";
            stSearchCond.szStopTime = "";
            stSearchCond.iCameraIndex = 0;
            stSearchCond.szName = "";
            stSearchCond.iMinSim = 0;
            stSearchCond.iMaxSim = 0;

            lResult = NETSDK.SIRIUS_SearchModelFaceByPic(UserID, picPath, ref stSearchCond);
            if (0 != lResult)
            {
                Console.WriteLine("以图搜图失败！错误编码:{0:d}", lResult);
                lResult = NETSDK.SIRIUS_Logout(UserID);

                Thread.Sleep(1 * 1000); // Notes:等待注销消息
                return -1;
            }
            else
            {
                Console.WriteLine("以图搜图成功！");
            }

            int iDuration = 3000;
            while (iDuration-- > 0)
            {
                iWaitFlag = Marshal.ReadInt32(pWaitFlag);
                if (iWaitFlag == 0)
                {
                    Console.WriteLine("收到反馈消息，等待结束");
                    break;
                }
                else
                {
                    if (iDuration % 10 == 0)
                        Console.WriteLine("等待消息...");
                    Thread.Sleep(1 * 100);
                }
            }

            // 注销服务器
            lResult = NETSDK.SIRIUS_Logout(UserID);
            if (0 != lResult)
            {
                Console.WriteLine("注销服务器失败！错误编码:{2:d}", lResult);
                return -2;
            }
            Console.WriteLine("注销服务器成功！");
            Thread.Sleep(1 * 1000); // Notes:等待注销消息

            UserID = 0;
            return 0;
        }

        void RealDataCallBack(uint UserID, ref NETSDK.REAL_DATA pRealData, IntPtr pData, IntPtr pUser)
        {
            using (var file = File.OpenWrite("test.h264"))
            {
                var buffer = new byte[pRealData.size];
                Marshal.Copy(pData, buffer, 0, pRealData.size);
                file.Write(buffer, 0, pRealData.size);
            }
        }

        public int OpenRealData()
        {
            uint lResult;

            pWaitFlag = Marshal.AllocHGlobal(sizeof(uint));//声明一个同样大小的空间
            Marshal.WriteInt32(pWaitFlag, 1);
            int iWaitFlag = Marshal.ReadInt32(pWaitFlag);

            mycall = new NETSDK.SIRIUS_pfnMessageCallBack(this.MessageCallBack);
            lResult = NETSDK.SIRIUS_SetMessageCallBack(mycall, pWaitFlag);
            if (0 != lResult)
            {
                Console.WriteLine("注册回调函数失败！错误编码:{0:d}", lResult);
                return -1;
            }
            Console.WriteLine("注册回调函数成功！");

            // 添加摄像机
            int iCameraIndex = 0;
            // 先删除一次
            lResult = NETSDK.SIRIUS_DeleteCamera(UserID, iCameraIndex);

            NETSDK.DEV_LOGIN_INFO stDevLoginInfo;
            stDevLoginInfo.eType = NETSDK.COLLECTOR_TYPE_USB_CAMERA;
            stDevLoginInfo.szIPAddr = "";
            stDevLoginInfo.iPort = 0;
            stDevLoginInfo.szUserName = "";
            stDevLoginInfo.szPassWord = "";
            stDevLoginInfo.iStreamIndex = 0;
            stDevLoginInfo.szDevName = "USB摄像机";

            lResult = NETSDK.SIRIUS_AddCamera(UserID, iCameraIndex, ref stDevLoginInfo);
            if (0 != lResult)
            {
                Console.WriteLine("添加摄像机失败！错误编码:{0:d}", lResult);
                return -1;
            }
            Console.WriteLine("添加摄像机成功！");
            //// 注册码流回调函数
            //realDataCall = new NETSDK.SIRIUS_pfnRealDataCallBack(this.RealDataCallBack);
            //lResult = NETSDK.SIRIUS_SetRealDataCallBack(realDataCall, IntPtr.Zero);
            //if (0 != lResult)
            //{
            //    Console.WriteLine("注册码流回调函数失败！错误编码:{0:d}", lResult);
            //    return -1;
            //}
            //Console.WriteLine("注册码流回调函数成功！\n");

            //// 启动码流预览
            //lResult = NETSDK.SIRIUS_StartRealPlay(UserID, 0);
            //if (0 != lResult)
            //{
            //    Console.WriteLine("启动码流预览失败！错误编码:{0:d}", lResult);
            //    return -1;
            //}
            //Console.WriteLine("启动码流预览成功！\n");


            return 0;
        }

        private FaceServerInfo GetServerInfo()
        {
            FaceServerInfo server = new FaceServerInfo();

            CONN_STR = AESUtil.AESDecrypt(ConfigurationManager.ConnectionStrings["FaceID"].ToString());
            var strs = CONN_STR.Split(';');
            foreach (var item in strs)
            {
                var index = item.IndexOf('=');
                if (item.Substring(0, index) == "Host")
                {
                    server.Host = item.Substring(index + 1);
                }
                else if (item.Substring(0, index) == "Port")
                {
                    server.Port = int.Parse(item.Substring(index + 1));
                }
                else if (item.Substring(0, index) == "Username")
                {
                    server.Username = item.Substring(index + 1);
                }
                else if (item.Substring(0, index) == "Password")
                {
                    server.Password = item.Substring(index + 1);
                }
            }

            return server;
        }

        public string Init()
        {
            uint lResult;

            // 初始化SDK
            lResult = NETSDK.SIRIUS_Init();
            if (0 != lResult)
            {
                var msg = string.Format("初始化SDK失败！错误编码:{0:d}。", lResult);
                Log.WriteLog("ERROR-FACEID：" + msg);
            }
            Console.WriteLine("初始化SDK成功！");

            // 登录服务器
            var serverInfo = this.GetServerInfo();
            NETSDK.USER_LOGIN_INFO stLoginInfo;
            stLoginInfo.szIPAddr = serverInfo.Host;
            stLoginInfo.iPort = serverInfo.Port;
            stLoginInfo.szUserName = serverInfo.Username;
            stLoginInfo.szPassWord = serverInfo.Password;
            lResult = NETSDK.SIRIUS_Login(ref stLoginInfo, ref UserID);
            if (0 != lResult)
            {
                var msg = string.Format("登录FaceID服务器失败！错误编码:{2:d}。"
                    , stLoginInfo.szIPAddr
                    , stLoginInfo.iPort
                    , lResult
                    );
                Log.WriteLog("ERROR-FACEID：" + msg);

                return msg;
            }
            else
            {
                Log.WriteLog(string.Format("INFO-FACEID：登录FaceID服务器成功！", stLoginInfo.szIPAddr, stLoginInfo.iPort));
            }

            return "";
        }

        public void Destroy()
        {
            uint lResult = 0;
            //lResult = NETSDK.SIRIUS_StopRealPlay(UserID, 0);
            //if (0 != lResult)
            //{
            //    Console.WriteLine("停止码流预览失败！错误编码:{0:d}", lResult);
            //    return;
            //}
            //Console.WriteLine("停止码流预览成功！");
            int iCameraIndex = 0;
            lResult = NETSDK.SIRIUS_DeleteCamera(UserID, iCameraIndex);
            if (0 != lResult)
            {
                Console.WriteLine("删除摄像机失败！错误编码:{0:d}", lResult);
            }
            else
            {
                Console.WriteLine("删除摄像机成功！");
            }

            if (UserID > 0)
            {
                // 注销服务器
                lResult = NETSDK.SIRIUS_Logout(UserID);
                if (0 != lResult)
                {
                    var msg = string.Format("注销FaceID服务器失败！错误编码:{2:d}", lResult);
                    Log.WriteLog("ERROR-FACEID：" + msg);
                }
                else
                {
                    Log.WriteLog("ERROR-FACEID：注销FaceID服务器成功！");
                    Thread.Sleep(1 * 1000); // Notes:等待注销消息

                    UserID = 0;
                }
            }
            // 销毁SDK
            lResult = NETSDK.SIRIUS_Cleanup();
            if (0 != lResult)
            {
                Console.WriteLine("销毁SDK失败！错误编码:%d", lResult);
            }
            else
            {
                Console.WriteLine("销毁SDK成功！");
            }
        }
    }

    public class FaceServerInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public FaceServerInfo()
        {
            this.Host = "127.0.0.1";
            this.Port = 8612;
            this.Username = "admin";
            this.Password = "admin";
        }
    }

    public class FaceSearchData
    {
        // 姓名
        public string Name { get; set; }
        // 相似度
        public float Similarity { get; set; }
        // 照片
        public string PhotoName { get; set; }
    }

    public class FaceSearchEventArgs : EventArgs
    {
        public List<FaceSearchData> data;
        public FaceSearchEventArgs(List<FaceSearchData> data)
        {
            this.data = data;
        }
    }

    public delegate void FaceSearchEventHandler(FaceSearchEventArgs e);
}
