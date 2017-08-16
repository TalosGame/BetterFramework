using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

public class WWWRequest
{
    // bundle 名称
    public string name = string.Empty;
    // bundle 路径
    public string path = string.Empty;    
    // bundle 全路径
    public string bundleFullPath = string.Empty;
    // 下载地址
    public string url = "";
    // 尝试下载的次数
    public int triedTimes = 0;
    // www链接
    public WWW www = null;

    public void ReStartDownLoad()
    {
        StopDownload();
        StartDownload();
    }

    public void StartDownload()
    {
        triedTimes++;

        CoroutineManger.Instance.StartCoroutine(StartDownload(this));
    }

    public void StopDownload()
    {
        CoroutineManger.Instance.StopCoroutine(StartDownload(this));

        this.www.Dispose();
        this.www = null;
    }

    private IEnumerator StartDownload(WWWRequest request)
    {
        //Debug.Log("url=====" + request.url);
        request.www = new WWW(request.url);
        yield return request.www;

        while (!request.www.isDone)
        {
            yield return null;
        }

        string filePath = PathConfiger.GetSandboxFilePath(path);
        Debugger.Log("filePath===" + filePath);

        byte[] datas = request.www.bytes;
        MLFileUtil.SaveFile(filePath, bundleFullPath, datas);
    }

    public void Dispose()
    {
        if (www == null)
            return;

        www.Dispose();
        www = null;
    }
}