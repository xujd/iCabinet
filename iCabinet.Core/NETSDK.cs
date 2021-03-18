using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace iCabinet.Core
{
    public class NETSDK
    {
        public NETSDK()
        {
            // 构造函数
        }

        // 宏定义
        public const int SIRIUS_NAME_LEN = 256;
        public const int SIRIUS_PATH_LEN = 256;
        public const int SIRIUS_IP_LEN = 32;
        public const int SIRIUS_TIME_LEN = 32;
        public const int MAX_FACE_FEATURE_SIZE = 32 * 1024;
        public const int MAX_FACE_PIC_SIZE = 4 * 1024 * 1024;
        public const int ANUBIS_MAGIC_NUMBER = 1986;

        // 消息定义
        public const int MESSAGE_CAMERA_ONLINE = 2001;		// 摄相机上线
        public const int MESSAGE_CAMERA_OFFLINE = 2002;		// 摄像机掉线
        public const int MESSAGE_SERVER_ONLINE = 2003;		// 服务器上线
        public const int MESSAGE_SERVER_OFFLINE = 2004;		// 服务器掉线
        public const int MESSAGE_FACE_MATCH = 2020; 	// 人脸匹配信息
        public const int MESSAGE_FACE_CAPTURE = 2021;   // 人脸抓拍信息
        public const int MESSAGE_FACE_SEARCH = 2022;        // 以图搜图信息
        public const int MESSAGE_FACE_ADD_DONE = 2023;      // 人脸添加消息
        public const int MESSAGE_FACE_UPDATE_DONE = 2024;   // 人脸更新消息
        public const int MESSAGE_FACE_DEL_DONE = 2025;		// 人脸删除消息
        public const int MESSAGE_AUTO_ADD_FACE = 2026;     // 自动入库消息

        // 错误定义
        public const int SERR_OK = 0;	// 无错误
        public const int SERR_FAIL = 1;	// 失败
        public const int SERR_TIMEOUT = 2;	// 超时
        public const int SERR_CONNECT_FAIL = 3;	// 连接失败
        public const int SERR_INVALID_PARAMETER = 4;	// 参数错误
        public const int SERR_INVALID_COMMAND = 5;	// 命令错误
        public const int SERR_CHANNEL_BUSY = 6;	// 通道占用
        public const int SERR_CHANNEL_EMPTY = 7;	// 通道空闲
        public const int SERR_NO_INITITAL = 8;	// 未初始化		
        public const int SERR_NO_MEMORY = 9;	// 内存不足
        public const int SERR_NO_SESSION = 10;	// 找不到连接
        public const int SERR_NO_FILE = 11;	// 找不到文件
        public const int SERR_SAME_NAME = 12;	// 人脸库重名
        public const int SERR_NO_NAME = 13;	// 找不到人脸
        public const int SERR_AUTH_CHECK_FAIL = 14;	// 授权检查失败
        public const int SERR_NO_USER = 15;	// 用户不存在
        public const int SERR_WRONG_PASSWORD = 16;	// 密码错误
        public const int SERR_NOT_PERMIT = 17;	// 无操作权限
        public const int SERR_NO_UNIQUE_FACE = 18;  // 没有唯一人脸	
        public const int SERR_AUTH_FILE_WRONG = 19;	// 授权文件错误
        public const int SERR_AUTH_OUT_OF_DATE = 20;	// 授权文件过期
        public const int SERR_AUTH_PIN_MISMATCH = 21;   // PIN验证错误
        public const int SERR_SERVER_BUSY = 22;	// 服务器忙
        public const int SERR_UPLOAD_TO_REMOTEF_FAIL = 30;	// 上传图片到设备失败
        public const int SERR_UPLOAD_FORMAT_WRONG = 31;	// 上传图片格式错误
        public const int SERR_UPLOAD_IMAGE_FAIL = 32;	// 加载上传图片失败
        public const int SERR_UPLOAD_LARGE_PIC = 33;	// 上传图片过大
        public const int SERR_UPLOAD_CREATE_VFD = 34;	// 创建VFD句柄失败
        public const int SERR_UPLOAD_VFD_RUN = 35;	// VFD内部运行时报错
        public const int SERR_UPLOAD_VFD_ERROR = 36;	// VFD返回错误
        public const int SERR_UPLOAD_FACE_MORE = 37;	// 上传图片检测到多于一个人脸
        public const int SERR_UPLOAD_RECT_INVALID = 38;	// 上传图片中未发现人脸
        public const int SERR_UPLOAD_FACE_SMALL = 39;	// 人脸小于112 * 96
        public const int SERR_UPLOAD_OPEN_FILE_FAIL = 40;	// 打开输出文件失败
        public const int SERR_UPLOAD_QUALITY_LOW = 41;	// 上传图片质量过低

        // 编码类型
        public const int ENCODE_TYPE_H264 = 0;
        public const int ENCODE_TYPE_H265 = 1;
        public const int ENCODE_TYPE_YUV420 = 2;
        public const int ENCODE_TYPE_YUV420_USB = 3;
        public const int ENCODE_TYPE_JPEG = 4;
        public const int ENCODE_TYPE_BMP = 5;

        // 摄像机类型
        public const int COLLECTOR_TYPE_INVALIDE = 0;     //无设备
        public const int COLLECTOR_TYPE_USB_CAMERA = 1;     //USB摄像头
        public const int COLLECTOR_TYPE_H264_FILE = 2;      //H264文件
        public const int COLLECTOR_TYPE_HK = 3;     //海康摄像头
        public const int COLLECTOR_TYPE_XM = 4;		//雄迈摄像头
        public const int COLLECTOR_TYPE_DH = 5;     //大华摄像头
        public const int COLLECTOR_TYPE_MHZ_E3 = 6;     //MHZ-E3人脸一体机

        // 摄像头状态类型
        public const int CAMERA_STATUS_OFFLINE = 0;
        public const int CAMERA_STATUS_ONLINE = 1;
        public const int CAMERA_STATUS_PLAYING = 2;

        // 性别类型
        public const int GENDER_TYPE_UNKNOW = 0;
        public const int GENDER_TYPE_MALE = 1;
        public const int GENDER_TYPE_FEMALE = 2;

        // 身份类型
        public const int IDENTITY_TYPE_WHITE = 0;
        public const int IDENTITY_TYPE_RED = 1;
        public const int IDENTITY_TYPE_BLUE = 2;
        public const int IDENTITY_TYPE_BLACK = 3;
        public const int IDENTITY_TYPE_GRAY = 4;
        public const int IDENTITY_TYPE_UNKNOWN = 5;
        public const int IDENTITY_TYPE_MAX = 6;

        // 系统参数类型
        public const int SETTING_TYPE_DETECT = 0;
        public const int SETTING_TYPE_PASSWORD = 1;
        public const int SETTING_TYPE_RELAY = 2;
        public const int SETTING_TYPE_SERVER = 3;
        public const int SETTING_TYPE_SAVE = 4;
        public const int SETTING_TYPE_WEBSERVER = 5;

        // 人脸检测模式
        public const int DETECT_TYPE_CAPTURE = 0;   //抓拍模式
        public const int DETECT_TYPE_TRACK = 1; //跟踪模式

        // 到期时间模式
        public const int DEAD_TIME_MODE_FOREVER = 0;    //永不过期
        public const int DEAD_TIME_MODE_DEADLINE = 1;   //到期生效+过期失效

        // 有效时间模式
        public const int VALID_TIME_MODE_ALWAYS = 0;    //一直有效
        public const int VALID_TIME_MODE_DAILY = 1; //每天指定时间有效
        public const int VALID_TIME_MODE_WEEKLY = 2;    //每周指定时间有效

        // 平台类型
        public const int PLATFORM_WIN32 = 0;
        public const int PLATFORM_WIN64 = 1;
        public const int PLATFORM_LINUX32 = 2;
        public const int PLATFORM_LINUX64 = 3;
        public const int PLATFORM_IOS32 = 4;
        public const int PLATFORM_IOS64 = 5;

        // 引擎类型
        public const int ENGINE_TYPE_ARCHV1 = 0;
        public const int ENGINE_TYPE_ARCHV2 = 1;
        public const int ENGINE_TYPE_ARCHV2_PRO = 2;
        public const int ENGINE_TYPE_ARCHV3 = 3;
        public const int ENGINE_TYPE_ARCHV3_PRO = 4;

        // 语言类型
        public const int LANGUAGE_TYPE_SIMPLE_CHINESE = 0;
        public const int LANGUAGE_TYPE_ENGLISH = 1;
        public const int LANGUAGE_TYPE_TRADITIONAL_CHINESE = 2;

        // 登陆信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct USER_LOGIN_INFO
        {
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_IP_LEN)]
            public string szIPAddr;
            public int iPort;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szUserName;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szPassWord;
        }

        // 设备信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct DEV_LOGIN_INFO
        {
            public int eType;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_IP_LEN)]
            public string szIPAddr;
            public int iPort;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szUserName;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szPassWord;
            public int iStreamIndex;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szDevName;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct CAMERA_INFO
        {
            public int eType;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szName;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_IP_LEN)]
            public string szIP;
            public int iPort;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szUserName;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szPassWord;
            public int iStreamIndex;
            public int eStatus;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct REAL_DATA
        {
            public int index;       //摄像机编码
            public int type;        //编码格式
            public int width;       //图片宽度
            public int height;      //图片高度
            public int channel;     //通道数
            public int size;		//图片数据长度
        }

        // 人脸位置结构
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct FACE_POS
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        // 人脸信息结构
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct FACE_INFO
        {
            public int MagicNum;        // 8612
            public int Version;         // 0001
            public int eEngineType;     // 引擎编码
            public FACE_POS face_pos;   // 人脸位置
            public Single fYaw;         // 人脸偏航角，左右转头
            public Single fRoll;        // 人脸横滚角，左右靠肩
            public Single fPitch;       // 人脸俯仰角，前后点头
            public int Orient;			// 人脸方向
            public int FaceID;          // 人脸编码
            public Single fScore;		// 人脸评分
            public Single fLiveness;    // 活体评分 
            public int eGender;         // 性别			
            public int iAge;            // 年龄
            public int iFeatureSize;	// 特征值长度
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = NETSDK.MAX_FACE_FEATURE_SIZE)]
            public byte[] FeatureData;	// 人脸特征值
        }

        // 人脸权限
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct FACE_AUTHORITY
        {
            public int iLinkChannel;                // 联动权限

            public int eDeadTimeMode;               // 到期时间
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = NETSDK.SIRIUS_TIME_LEN)]
            public byte[] szDeadTimeStart;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = NETSDK.SIRIUS_TIME_LEN)]
            public byte[] szDeadTimeStop;

            public int eValidTimeMode;          // 有效时间
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] iDailyStart;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] iDailyStop;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 21)]
            public int[] iWeeklyStart;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 21)]
            public int[] iWeeklyStop;
        }

        // 人员信息结构
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct PEOPLE_INFO
        {
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_TIME_LEN)]
            public string szTime;   // 入库时间
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szName;	// 姓名
            public int eGender;     // 性别
            public int iAge;        // 年龄
            public int eIdentity;	// 身份信息
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szVoice;	// 语音模板
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szNotes;  // 备注信息
            public FACE_AUTHORITY stAuthority;	// 权限信息
        }

        // 匹配信息结构
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct FACE_MATCH_INFO
        {
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_TIME_LEN)]
            public string szTime;               // 匹配时间
            public int iCameraIndex;            // 摄像机编码
            public PEOPLE_INFO stPeopleInfo;    // 匹配人员信息
            public Single fSimilarity;          // 匹配度
            public FACE_INFO stCaptureFaceInfo; // 抓拍人脸信息
            public FACE_INFO stMatchFaceInfo;   // 匹配人脸信息
            public int iCapturePicOffset;       // 抓拍图片偏移
            public int iCapturePicSize;         // 抓拍图片长度
            public int iMatchPicOffset;         // 匹配图片偏移
            public int iMatchPicSize;			// 匹配图片长度
        }

        // 继电器信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct LINK_INFO
        {
            public int type;					// Link类型 0-串口 1-网络
            public int iComPort;				// 串口号
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_IP_LEN)]
            public string szIP;	                // IP地址
            public int iPort;					// 端口号
            public int iOutIndex;				// 联动口(1-4)
            public int iLastTime;				// 持续时间
        }

        // 人脸检测信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct DETECT_INFO
        {
            public int eDetectMode;	            //工作模式
            public int iMinFaceWidth;			//最小人脸宽度 pixel
            public int iMinFaceHeight;			//最小人脸高度 pixel
            public int iScoreThresh;			//人脸质量门限
            public int iLivenessThresh;		    //活体检测门限
            public int iROILeft;				//人脸检测区域左边界
            public int iROITop;				    //人脸检测区域上边界
            public int iROIRight;				//人脸检测区域右边界
            public int iROIBottom;              //人脸检测区域下边界
            public int iYawThresh;              //人脸侧偏角门限
            public int iPitchThresh;            //人脸俯仰角门限
            public int iRollThresh;				//人脸横滚角门限
            public int iTrackLevel;			    //去重强度0-100
            public int iMinTrackCount;			//确认帧数
            public int iMaxMissCount;			//丢失帧数
            public int iLoopCount;				//重复帧数

            public int iMatchThresh;			//人脸引擎匹配门限 0--100
            public int iMatchInterval;			//匹配间隔
            public int iCapInterval;			//抓拍间隔
            public int bFaceAttrEnable;         //使能探测人脸属性
            public int bLivenessEnable;			//使能活体检测
        }

        // 搜索条件
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SEARCH_COND
        {
            public int Index;           // 起始条目,从0开始计数
            public int PageNum;         // 查询数量
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_TIME_LEN)]
            public string szStartTime;	// 起始时间，为空表示不限制
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_TIME_LEN)]
            public string szStopTime;   // 结束时间，为空表示不限制
            public int iCameraIndex;	// 摄像机编码，为0-3以外的数字表示不限制
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szName;       // 姓名，为空表示不限制
            public int iMinSim;     // 匹配阈值下限，为0表示不限制
            public int iMaxSim;		// 匹配阈值下限，为0表示不限制
        }

        // 人脸模板查询结果
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct MODEL_FACE_ITEM
        {
            public PEOPLE_INFO stPeopleInfo;	// 人员信息
            public FACE_INFO stFaceInfo;		// 人脸信息
        }

        // 人脸匹配查询结果
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct MATCH_FACE_ITEM
        {
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_NAME_LEN)]
            public string szID;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = NETSDK.SIRIUS_TIME_LEN)]
            public string szTime;
            public int iCameraIndex;
            public PEOPLE_INFO stPeopleInfo;
            public int iSimilarity;
        }

        // 人脸模板以图搜图结果
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct MODEL_FACE_ITEM_EXT
        {
            public PEOPLE_INFO stPeopleInfo;	// 人员信息
            public FACE_INFO stFaceInfo;		// 人脸信息
            public float fSimilarity;	    // 人脸相似度
        }

        // 授权信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct LICENSE_INFO_S
        {
            public int iMagicNum;		//起始标志
            public int iStartYear;		//授权开始日期
            public int iStartMonth;
            public int iStartDay;
            public int iStopYear;		//授权结束日期
            public int iStopMonth;
            public int iStopDay;
            public int iCameraNum;		//接入摄像头总数
            public int iFaceNum;		//注册人脸总数
        }

        // 检测参数
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct DETECT_SETTING
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public DETECT_INFO[] staDetectInfo;
        }

        // 密码信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct PASSWORD_SETTING
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SIRIUS_NAME_LEN)]
            public string szUserName;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SIRIUS_NAME_LEN)]
            public string szOldPassWord;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SIRIUS_NAME_LEN)]
            public string szNewPassWord;
        }

        // 继电器信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct RELAY_SETTING
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public LINK_INFO[] staLinkInfo;
        }

        // 服务器信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SERVER_SETTING
        {
            public int ePlatform;       // 平台类型
            public int eEngineType;     // 引擎类型
            public int iEngineNum;      // 引擎数量
        }

        // 存储信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SAVE_SETTING
        {
            public int iStopLimit;      // 最小剩余空间
            public int iUsedLimit;      // 最大使用空间
            public int bSaveMisMatch;   // 保存未匹配图
            public int bSaveOrigin;		// 保存抓拍原图
            public int bAutoRegister;   // 自动入库
        }

        // WebServer信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct WEB_SERVER_SETTING
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SIRIUS_IP_LEN)]
            public string szIP;
            public int iPort;
        }

        // 事件回调函数
        public delegate void SIRIUS_pfnMessageCallBack(uint UserID, uint ulMessage, IntPtr pData, int iSize, IntPtr pUser);

        // 码流回调函数
        public delegate void SIRIUS_pfnRealDataCallBack(uint UserID, ref REAL_DATA pRealData, IntPtr pData, IntPtr pUser);

        // SDK初始化函数
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_Init();

        // SDK清理函数
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_Cleanup();

        // 注册消息回调函数
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_SetMessageCallBack(SIRIUS_pfnMessageCallBack pfnMessageCallBack, IntPtr pUser);

        // 注册码流回调函数
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_SetRealDataCallBack(SIRIUS_pfnRealDataCallBack pfnRealDataCallBack, IntPtr pUser);

        // 登陆服务器
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_Login(ref NETSDK.USER_LOGIN_INFO pstLoginInfo, ref uint pUserID);

        // 注销服务器
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_Logout(uint UserID);

        // 启动码流预览
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_StartRealPlay(uint UserID, int iCameraIndex);

        // 停止码流预览
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_StopRealPlay(uint UserID, int iCameraIndex);

        // 获取设备信息
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_GetCameraInfo(uint UserID, int iCameraIndex, ref CAMERA_INFO pCameraInfo);

        // 设置设备设备
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_SetCameraInfo(uint UserID, int iCameraIndex, ref CAMERA_INFO pCameraInfo);

        // 添加设备
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_AddCamera(uint UserID, int iCameraIndex, ref DEV_LOGIN_INFO pLoginInfo);

        // 删除设备
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_DeleteCamera(uint UserID, int iCameraIndex);

        // 添加人脸
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_AddFace(uint UserID, ref PEOPLE_INFO pPeopleInfo, string szPicPath, bool bSync = false);

        // 更新人脸
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_UpdateFace(uint UserID, ref PEOPLE_INFO pPeopleInfo, bool bSync = false);

        // 删除人脸
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_DeleteFace(uint UserID, string szName, bool bSync = false);

        // 查找人脸模板
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_FindModelFace(uint UserID, ref SEARCH_COND pSearchCond, IntPtr pModelFaceList, ref int pItemNum, ref int pTotalNum);

        // 查找匹配记录
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_FindMatchFace(uint UserID, ref SEARCH_COND pSearchCond, IntPtr pMatchFaceList, ref int pItemNum, ref int pTotalNum);

        // 获取人脸信息
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_GetModelFace(uint UserID, string szName, ref FACE_INFO pFaceInfo, string szPicPath);

        // 获取人脸信息
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_GetMatchFace(uint UserID, string szID, ref FACE_INFO pFaceInfo, string szPicPath);

        // 获取系统参数
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_GetConfig(uint UserID, int eType, IntPtr pBuffer, int iSize);

        // 配置系统参数
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_SetConfig(uint UserID, int eType, IntPtr pBuffer, int iSize);

        // 导入授权文件
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_RegisterLicense(uint UserID, string szLicensePath);

        // 获取授权信息
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_GetLicenseInfo(uint UserID, ref LICENSE_INFO_S pLicenseInfo);

        // 以图搜图
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_SearchModelFaceByPic(uint UserID, string szPicPath, ref SEARCH_COND pSearchCond);

        // 推送图片
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_PushImage(uint UserID, string szPicPath, int iCameraIndex);

        // 初始化继电器
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_InitRelay(ref LINK_INFO pLinkInfo);

        // 销毁继电器
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_CleanupRelay();

        // 打开继电器端口
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_TurnOnRelay(int iOutIndex);

        // 关闭继电器端口
        [DllImport(@"NetSDK.dll")]
        public static extern uint SIRIUS_TurnOffRelay(int iOutIndex);
    }
}
