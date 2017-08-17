/// <summary>
/// 创建者：dyjia
/// Date： 2017-7-19 21:49:35
/// 描述： 用于显示隐藏节点时控制动画播放
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablePlayTween : MonoBehaviour {

    public UITweener ut;
    public float time = -1;
    public float delayTime = 0;
	
    void OnEnable ()
    {
        ut.ResetToBeginning();
        ut.enabled = false;
        if (time != -1)
        {
            StartCoroutine(DelayTime());
        }
        else
        {
            ut.enabled = true;
            ut.Play();
        }
    }

    IEnumerator DelayTime ()
    {
        yield return new WaitForSeconds(delayTime);
        ut.enabled = true;
        ut.Play();
        yield return new WaitForSeconds(time);
        ut.enabled = false;
    }
}
