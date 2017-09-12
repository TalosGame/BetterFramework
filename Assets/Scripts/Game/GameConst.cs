using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// loading状态
public enum LoadingState
{
	none = -1,          // 无
	checkUpdateRes,     // 检查资源更新
	checkUpdateResErr,  // 检查资源更新异常   
	connectError,       // 链接异常
	startDownLoading,   // 开始下载资源
	downLoading,        // 下载中
	downLoadFailed,     // 下载失败
	downLoadSucess,     // 下载成功
	latestVersion,      // 最新版本
	startResLoading,    // 开始资源加载
	resLoading,         // 资源加载
}

public enum ResLoadingStatus
{
	none = -1,          // 无
	LobbyRes,           // 大厅资源加载
	GameRes,            // 游戏资源加载
	ExitGame,           // 退出游戏加载          
}

public sealed class GameConst
{
    public const int MSG_VERIFY_ID = 1;
    public const int MSG_VERIFY_DATA = 3;

    // lua公共方法名称
    public const string LUA_BROCAST_VIEW_FUNC = "BrocastViewEvent";

    // 游戏中的事件通知消息
    public const string NOTIFY_HANDLE_CONNECT_SUCESS = "handle_connect_sucess";
    public const string NOTIFY_HANDLE_UI_LOAD_PROCESS = "handle_ui_load_process";
    public const string NOTIFY_HANDLE_UI_LOADING_COMPLETE = "handle_ui_loading_complete";
    public const string NOTIFY_HANDLE_BUY_COIN = "notify_handle_buy_coin";

    // PlayerPrefs Const
    public const string SETTING_LANGUAGE_CHOOSE = "SETTING_LANGUAGE_CHOOSE";

    // 普通话
    public const float LANGUAGE_MANDARIN = 1.0f;
    // 方言
    public const float LANGUAGE_DIALECT = 0.0f;

    /// <summary>
    /// 一次下载的线程数
    /// </summary>
    public const int DOWNLOAD_THREADS_COUNT = 4;

    /// <summary>
    /// 下载错误尝试的次数
    /// </summary>
    public const int DOWNLOAD_RETRY_TIME = 2;

    // 资源命名
    public const string RES_AB_SHADER = "shaders";
    public const string RES_AB_LOGIN_ATLAS = "uiloginatlas";

    // string const
    public const string CHECK_UPDATE_RES = "检查资源更新中...";
    public const string LATEST_VERSION = "已是最新版本";
    public const string DOWN_LOAD_RES = "正在下载资源,请稍候...";
    public const string DOWN_LOAD_SUCESS = "下载资源成功";
    public const string DOWN_LOAD_ERROR = "下载资源失败...";
    public const string RESOURCE_LOADING = "资源加载中...";

    // scene
    public const string ENTRY_SCENE = "EntryScene";
    public const string LOADING_SCENE = "LoadingScene";
    public const string LOBBY_SCENE = "LobbyScene";
    public const string GAME_SCENE = "GameScene";

    #region define audio id
    public const int AUDIO_MUSIC_GAME_BG = 0;
    public const int AUDIO_SOUND_FIRE_GUN = 1;
    #endregion
}