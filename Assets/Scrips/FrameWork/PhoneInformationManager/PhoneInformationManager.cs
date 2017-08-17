using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneInformationManager : DDOLSingleton<PhoneInformationManager> {

    /// <summary>
    /// 获取剪切板内容
    /// </summary>
    /// <returns></returns>
    public string GetClipboarFromPhone() {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        return jo.Call<string>("GetClipboar");
#elif UNITY_IPHONE && !UNITY_EDITOR

#endif
        return "";
    }
    public void SetClipboar(string str)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.Call("SetClipboar",str);
        return;
#elif UNITY_IPHONE  && !UNITY_EDITOR

#endif

    }
    /// <summary>
    /// 获取手机震动
    /// </summary>
    /// <param name="times">振动Date</param>
    public void GetVibrateStrength(int times) {
#if UNITY_ANDROID  && !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.Call("VibrateStrength", times);
        return;
#elif UNITY_IPHONE && !UNITY_EDITOR

#endif
    }
    /// <summary>
    /// 获取wifi强度  1-5     返回-1证明不是wifi
    /// </summary>
    /// <returns></returns>
    public int GetWifiStrength()
    {
#if UNITY_ANDROID  && !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        return  jo.Call<int>("WifiPower");
#elif UNITY_IPHONE  && !UNITY_EDITOR
        
#endif
        return -1;
    }
    /// <summary>
    /// 获取电池电量
    /// </summary>
    /// <returns></returns>
    public int GetElectricityStrength()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
                string CapacityString = System.IO.File.ReadAllText("/sys/class/power_supply/battery/capacity");
                return int.Parse(CapacityString);
         }
        catch (System.Exception e)
        {
            Debug.Log("Failed to read battery power; " + e.Message);
        }
#elif UNITY_IPHONE && !UNITY_EDITOR

#endif
        return -2;
    }

    /// <summary>
    /// 拉入房间方法
    /// </summary>
    /// <returns></returns>
    public string GetRoomId() {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        return jo.Call<string>("GetRoomId");
#elif UNITY_IPHONE && !UNITY_EDITOR

#endif
        return "";

    }
}
