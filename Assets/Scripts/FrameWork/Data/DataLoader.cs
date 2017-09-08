//
// DataLoader.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.数据加载器
//      2.提供了反序列化csv数据
//      3.提供了反序列化pb数据, 并且能支持压缩过的pb数据
//
// Copyright (c) 2017 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Csv.Serialization;
using ProtoBuf;

public sealed class DataLoader<T> where T : class, new()
{
    #region csv loader
    public static List<T> DeserializeFromText(string txt)
    {
        if (txt == null)
            return null;

        var cs = new CsvSerializer<T>()
        {
            UseTextQualifier = true,
            UseLineNumbers = false,
            IgnoreReferenceTypesExceptString = false,
        };

        return (List<T>)cs.DeserializeStream(txt);
    }

    public static T LoadCSV(string name)
    {
        return MLResourceManager.Instance.LoadResource(name, ResourceType.RES_META_DATAS) as T;
    }
	#endregion

	#region protobuf loader
    public static T LoadPBData(string name, bool useZip = false)
    {
		MLResourceManager resMgr = MLResourceManager.Instance;
        TextAsset textAsset = resMgr.LoadResource(name, ResourceType.RES_META_DATAS) as TextAsset;
		if (textAsset == null)
		{
			Debug.LogError("load prototype data error!!!");
            return default(T);
		}

        T t = default(T);
        byte[] datas = textAsset.bytes;
        if(useZip)
        {
            t = DeserializePBClassWithGZip(datas);
        }else
        {
            t = DeserializePBClass(datas);
        }

        resMgr.UnloadResource(name);
        return t;
    }

    public static T DeserializePBClassWithGZip(byte[] datas)
	{
        byte[] zipDatas = CompressUtil.GZipDecode(datas);
        return DeserializePBClass(zipDatas);
	}

    public static T DeserializePBClass(byte[] datas)
    {
		using (MemoryStream ms = new MemoryStream(datas))
		{
			T ret = Serializer.Deserialize<T>(ms);
			return ret;
		}
    }
	#endregion
}
