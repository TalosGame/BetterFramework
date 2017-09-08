using UnityEngine;
using System;
using System.Collections;
using LuaInterface;

#region delegate 框架内全局定义等委托函数
// 对象状态改变委托
public delegate void StateChangeEvent(object obj, ObjectStateType newState, ObjectStateType oldState);

public delegate void UUIEventHandle(GameObject listener, object arg, params object[] _params);

// http消息接收委托
public delegate void HttpMessageReceive(int messageId, string json);

// 更新事件委托
public delegate void UpdateEventHandle(float deltaTime);

// 按钮点击事件委托
public delegate void OnButtonClick();

// 通用委托
public delegate bool BoolDelegate();
public delegate void VoidDelegate();

#endregion

public class FrameWorkConst
{
	// 一次下载的线程数
	public const int DOWNLOAD_THREADS_COUNT = 1;

	// 下载错误尝试的次数
	public const int DOWNLOAD_RETRY_TIME = 2;

    // 音效默认预加载个数
    public const int SOUND_PRELOAD_AMOUNT = 2;
}

/// <summary>
/// 窗口类型
/// </summary>
public enum UIWindowType : byte
{
	Normal, // 正常可推出窗口
	Fixed,  // 固定窗口
	PopUp,  // 弹出窗口
    Custom, // 自定义窗口
}

/// <summary>
/// 窗口显示类型
/// </summary>
public enum UIWindowShowMode : byte
{
	DoNothing = 0,
	HideOtherWindow,
	DestoryOtherWindow,
}

public enum UIWindowColliderMode : byte
{
	None,      // No BgTexture and No Collider
	Normal,    // Collider with alpha 0.001 BgTexture
	WithBg,    // Collider with alpha 1 BgTexture
}

public enum UIWindowNavigationMode
{
	IgnoreNavigation = 0,
	NormalNavigation,
}

/// <summary>
/// 窗口显示样式
/// </summary>
public enum WindowShowStyle
{
	Normal,
	CenterToBig,
	FromTop,
	FromDown,
	FromLeft,
	FromRight,
}

/// <summary>
/// 场景类型
/// </summary>
public enum SceneType
{
    versionCheckScene,
    loadingScene,
    gameInlet,
    loginScene,
    gameScene,
}

// ui 事件类型
public enum UIEventType
{
	onClick,  
	onDown,
	onUp,
	onEnter, 
	onExit,
}

// 对象状态类型
public enum ObjectStateType
{
    none,
    initial,
    loading,
    ready,
    disable,
    close
}

// 声音类型
public enum AudioType
{
    None = -1,
    Music,      // 音乐
    Sound       // 音效
}

//Note: this is the provided BuildTarget enum with some entries removed as they are invalid in the dropdown
public enum ValidBuildTarget
{
    //NoTarget = -2,        --doesn't make sense
    //iPhone = -1,          --deprecated
    //BB10 = -1,            --deprecated
    //MetroPlayer = -1,     --deprecated
    StandaloneOSXUniversal = 2,
    StandaloneOSXIntel = 4,
    StandaloneWindows = 5,
    WebPlayer = 6,
    WebPlayerStreamed = 7,
    IOS = 9,
    PS3 = 10,
    XBOX360 = 11,
    Android = 13,
    StandaloneLinux = 17,
    StandaloneWindows64 = 19,
    WebGL = 20,
    WSAPlayer = 21,
    StandaloneLinux64 = 24,
    StandaloneLinuxUniversal = 25,
    WP8Player = 26,
    StandaloneOSXIntel64 = 27,
    BlackBerry = 28,
    Tizen = 29,
    PSP2 = 30,
    PS4 = 31,
    PSM = 32,
    XboxOne = 33,
    SamsungTV = 34,
    N3DS = 35,
    WiiU = 36,
    tvOS = 37,
    Switch = 38
}

/// <summary>
/// 资源配置表
/// </summary>
public class ABConfiger
{
	/// <summary>
	/// asset manifset 名称
	/// </summary>
	public const string ASSET_MANIFEST_NAME = "GameRes";

	/// <summary>
	/// bundle后缀名
	/// </summary>
	public const string BUNDLE_SUFFIX = "assetbundle";

    // persistentData AB & File 路径
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    public static string PACKAGE_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
#elif UNITY_IOS
    public static string PACKAGE_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
#elif UNITY_ANDROID
    public static string PACKAGE_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
#endif

    // streamingAssets AB 路径
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    public static string PACKAGE_STREAMING_AB_DATA_PATH = Application.streamingAssetsPath + "/";
#elif UNITY_IOS
    public static string PACKAGE_STREAMING_AB_DATA_PATH = Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
    public static string PACKAGE_STREAMING_AB_DATA_PATH = Application.dataPath + "!assets/";
#endif

    // streamingAssets file 路径
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    public static string PACKAGE_STREAMING_DATA_PATH = "file://" + Application.streamingAssetsPath + "/";
#elif UNITY_IOS
    public static string PACKAGE_STREAMING_DATA_PATH = "file://" + Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
    public static string PACKAGE_STREAMING_DATA_PATH = "jar:file://" + Application.dataPath + "!/assets/";
#endif

    /// <summary>
    /// 获取本地资源文件路径
    /// </summary>
    /// <param name="assetBundlName"></param>
    /// <returns></returns>
    public static string GetABFilePath(string fileName)
    {
        string filePath = GetSandboxABFilePath(fileName);
        if (MLFileUtil.CheckFileExits(filePath))
        {
            Debug.Log("Find sandbox ABFile path====" + filePath);
            return filePath;
        }

        // 获取stream assets 路径的资源不判定文件是否存在
        // android不支持文件判定
        filePath = GetLocalABFilePath(fileName);

#if !UNITY_ANDROID
        if (MLFileUtil.CheckFileExits(filePath))
        {
            Debug.Log("Find local ABFile path====" + filePath);
            return filePath;
        }
#else
        //Debugger.Log("Find local ABFile path====" + filePath);
        return filePath;
#endif
        return filePath;
    }

    // 获取沙盒AB文件路径
    private static string GetSandboxABFilePath(string assetBundlName)
    {
        string filePath = string.Format("{0}{1}", PACKAGE_PERSISTENT_DATA_PATH, assetBundlName);
        return filePath;
    }

    // 获取本地AB文件路径
    private static string GetLocalABFilePath(string assetBundlName)
    {
        string localFilePath = string.Format("{0}{1}", PACKAGE_STREAMING_AB_DATA_PATH, assetBundlName);
        return localFilePath;
    }

    public static string GetSandboxFilePath(string fileName)
    {
        string filePath = string.Format("{0}{1}", PACKAGE_PERSISTENT_DATA_PATH, fileName);
        return filePath;
    }
}


