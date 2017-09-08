using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 创建者：dyjia
/// Date： 2017-8-10 22:48:03
/// 描述： 用于管理网络下载图片资源
/// </summary>
public class WWWTextureManager : DDOLSingleton<WWWTextureManager>
{
    Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

    public IEnumerator LoadTexture(string url, Action<Texture2D> callback)
    {
        if (textures.ContainsKey(url))
        {
            if (callback != null)
            {
                callback(textures[url]);
            }
        }
        else
        {
            WWW www = new WWW(url);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                if (textures.ContainsKey(url))
                {
                    textures[url] = www.texture;
                }
                else
                {
                    textures.Add(url, www.texture);
                }
                if (callback != null)
                {
                    callback(textures[url]);
                }
            }
            www.Dispose();
        }
    }

    public void UnLoadAllTextures ()
    {
        foreach (Texture2D t in textures.Values)
        {
            if (t != null)
            {
                GameObject.DestroyImmediate(t);
            }
        }
        textures.Clear();
    }

    public void UnLoadTexture (string key)
    {
        if (textures.ContainsKey(key))
        {
            GameObject.DestroyImmediate(textures[key]);
            textures.Remove(key);
        }
    }
}
