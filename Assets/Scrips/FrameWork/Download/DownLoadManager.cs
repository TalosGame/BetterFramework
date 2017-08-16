using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

/// <summary>
/// 下载管理器
/// </summary>
public class DownLoadManager : DDOLSingleton<DownLoadManager>
{
    // 下载URL
    private string downLoadURL;
    public string DownLoadURL
    {
        set
        {
            downLoadURL = value;
            Debugger.Log("Down load server url===" + downLoadURL);
        }
    }

    // 下载的文件列表
    private List<ResData> downLoadFiles = new List<ResData>();
    public List<ResData> DownLoadFiles
    {
        get { return downLoadFiles; }
    }

    // 执行中的下载请求
    private Dictionary<string, WWWRequest> processingRequest = new Dictionary<string, WWWRequest>();
    // 成功下载的请求
    private Dictionary<string, WWWRequest> succeedRequest = new Dictionary<string, WWWRequest>();
    // 失败下载的请求
    private Dictionary<string, WWWRequest> failedRequest = new Dictionary<string, WWWRequest>();
    // 等待下载的请求
    private List<WWWRequest> waitingRequests = new List<WWWRequest>();

    // 更新逻辑缓存
    private List<string> newFinisheds = new List<string>();
    private List<string> newFaileds = new List<string>();

    private bool startDownLoad = false;

    public void AddDownLoadFile(ResData data)
    {
        if (DownLoadFiles.Contains(data))
        {
            Debugger.LogWarning("Down load file already exist! name:" + data.name);
            return;
        }

        DownLoadFiles.Add(data);
    }

    /// <summary>
    /// debug下载文件信息
    /// </summary>
    public void DebugDownLoadFiles()
    {
        foreach (ResData data in DownLoadFiles)
        {
            Debugger.Log("Begin down load file name==" + data.name);
        }
    }

    /// <summary>
    /// 下载所需的文件
    /// </summary>
    public void StartDownloadFiles()
    {
        foreach(ResData fileRes in DownLoadFiles)
        {
            Debug.Log("start download file name===" + fileRes.name);
            StartDownloadFile(fileRes);
        }

        startDownLoad = true;
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="name"></param>
    private void StartDownloadFile(ResData data)
    {
        string url = FormatUrl(data.bundleName);
        if (isDownloadingWWW(url) || succeedRequest.ContainsKey(url))
        {
            return;
        }

        WWWRequest request = new WWWRequest();

        request.name = data.name;
        request.path = data.path;
        request.bundleFullPath = data.bundleName.Replace(data.path + "/", "");
        request.url = url;

        addRequestToWaitingList(request);
    }

    /// <summary>
    /// 是否已在下载
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private bool isDownloadingWWW(string url)
    {
        if (isInWaitingList(url))
            return true;

        return processingRequest.ContainsKey(url);
    }

    /// <summary>
    /// 是否在等待下载列表里
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private bool isInWaitingList(string url)
    {
        for (int i = 0; i < waitingRequests.Count; i++)
        {
            WWWRequest request = waitingRequests[i];
            if (request.url.Equals(url))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 添加请求下载列表
    /// </summary>
    /// <param name="request"></param>
    private void addRequestToWaitingList(WWWRequest request)
    {
        if (succeedRequest.ContainsKey(request.url) || isInWaitingList(request.url))
            return;

        waitingRequests.Add(request);
    }

    /// <summary>
    /// 检查是否有资源下载失败
    /// </summary>
    /// <returns></returns>
    public bool CheckDownloadFailed()
    {
        if (failedRequest.Count > 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查下载是否完毕
    /// </summary>
    /// <returns></returns>
    public bool CheckDownloadOver()
    {
        if (DownLoadFiles.Count != succeedRequest.Count)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取下载的进度
    /// </summary>
    /// <param name="bundlefiles"></param>
    /// <returns></returns>
    public float ProgressOfBundles()
    {
        int currentNum = succeedRequest.Count;
        float totalNum = downLoadFiles.Count;

        return (float)currentNum / totalNum;
    }


    /// <summary>
    /// 停止所有下载文件
    /// </summary>
    public void StopAll()
    {
        startDownLoad = false;
        waitingRequests.Clear();        
        ReleaseAllRequest();
    }

    /// <summary>
    /// 释放所有下载的请求资源
    /// </summary>
    /// <param name="url"></param>
    private void ReleaseAllRequest()
    {
        foreach (WWWRequest request in processingRequest.Values)
            request.Dispose();

        for (int i = 0; i < DownLoadFiles.Count; i++)
        {
            ResData resData = DownLoadFiles[i];
            string resName = resData.name;

            if (succeedRequest.ContainsKey(resName))
            {
                succeedRequest[resName].Dispose();
                continue;
            }

            if (failedRequest.ContainsKey(resName))
            {
                failedRequest[resName].Dispose();
            }
        }

        DownLoadFiles.Clear();
        processingRequest.Clear();
        succeedRequest.Clear();
        failedRequest.Clear();
    }

    void Update()
    {
        if (!startDownLoad)
            return;

        newFinisheds.Clear();
        newFaileds.Clear();

        // Check if any WWW is finished or errored
        foreach (WWWRequest request in processingRequest.Values)
        {
            if (request.www.error != null)
            {
                if (request.triedTimes - 1 < GameConst.DOWNLOAD_RETRY_TIME)
                {
                    // Retry download
                    request.ReStartDownLoad();
                }
                else
                {
                    newFaileds.Add(request.name);
                    Debug.LogError("Download " + request.url + " failed for " + request.triedTimes + " times.\nError: " + request.www.error);
                }
            }
            else if (request.www.isDone)
            {
                newFinisheds.Add(request.name);
            }
        }

        // Move complete bundles out of downloading list
        foreach (string finishBundle in newFinisheds)
        {
            if (succeedRequest.ContainsKey(finishBundle))
            {
                int i = 0;
                i++;
            }

            succeedRequest.Add(finishBundle, processingRequest[finishBundle]);

            processingRequest.Remove(finishBundle);
        }

        // Move failed bundles out of downloading list
        foreach (string failedBundle in newFaileds)
        {
            if (!failedRequest.ContainsKey(failedBundle))
                failedRequest.Add(failedBundle, processingRequest[failedBundle]);

            processingRequest.Remove(failedBundle);
        }

        // Start download new bundles
        int waitingIndex = 0;
        while (processingRequest.Count < GameConst.DOWNLOAD_THREADS_COUNT &&
               waitingIndex < waitingRequests.Count)
        {
            WWWRequest curRequest = waitingRequests[waitingIndex++];
            waitingRequests.Remove(curRequest);

            curRequest.StartDownload();

            processingRequest.Add(curRequest.name, curRequest);
        }
    }

    #region 工具方法
    public string FormatUrl(string urlstr)
    {
        Uri url;
        if (!IsAbsoluteUrl(urlstr))
        {
            url = new Uri(new Uri(downLoadURL + '/'), urlstr);
        }
        else
        {
            url = new Uri(urlstr);
        }

        return url.AbsoluteUri;
    }

    private bool IsAbsoluteUrl(string url)
    {
        Uri result;
        return Uri.TryCreate(url, UriKind.Absolute, out result);
    }
    #endregion
}

