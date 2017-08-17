/// <summary>
/// 创建者：dyjia
/// Date： 2017-7-15 21:24:07
/// 描述： 用于给Sprite播放序列帧动画
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SequenceSprite : MonoBehaviour {

    public UISprite sp;
    public List<string> names;
    public float time;
    public Action callback;
    public bool loop = false;
    public float loopTime = 0;

    IEnumerator PlayEffect (float time, List<string> name, UISprite sp)
    {
        sp.gameObject.SetActive(true);

        float speed = time / name.Count;
        for (int i = 0; i < name.Count; i++)
        {
            sp.spriteName = name[i];
            yield return new WaitForSeconds(speed);
        }
        sp.gameObject.SetActive(false);
        if (callback != null)
        {
            callback();
        }
    }

    IEnumerator PlayEffect (float time, List<string> name, UISprite sp, float totleTime)
    {
        sp.gameObject.SetActive(true);

        float speed = time / name.Count;
        float curTime = 0;
        int i = 0;
        while (loop)
        {
            i = i % name.Count;
            sp.spriteName = name[i];
            yield return new WaitForSeconds(speed);
            curTime += speed;
            i++;
            if (totleTime != -1 && curTime >= totleTime)
            {
                sp.gameObject.SetActive(false);
                if (callback != null)
                {
                    callback();
                }
                yield break;
            }
        }
    }

    void OnEnable ()
    {
        if (loop)
        {
            StartCoroutine(PlayEffect(time, names, sp, loopTime));
        }
        else
        {
            StartCoroutine(PlayEffect(time, names, sp));
        }
    }
}
