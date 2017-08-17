using UnityEngine;
using System;
using System.Collections;

public class ResourcesManager : ResManagerBase
{
    protected override UnityEngine.Object Load(ResourceInfo info)
    {
        UnityEngine.Object resObj = Resources.Load(resourceDefine.GetResourcePath(info.ResType, info.Name));
        info._assetObj = resObj;
        return resObj;
    }

    public override IEnumerator LoadAsync(ResourceInfo info, Action<UnityEngine.Object> load, Action<float> progress)
    {
        ResourceRequest rr = Resources.LoadAsync(resourceDefine.GetResourcePath(info.ResType, info.Name));
        while (!rr.isDone)
        {
            if (progress != null)
                progress(rr.progress);

            yield return null;
        }

        info._assetObj = rr.asset;

        if (progress != null)
            progress(1.0f);

        if (load != null)
            load(info._assetObj);

        yield return rr;
    }
}

