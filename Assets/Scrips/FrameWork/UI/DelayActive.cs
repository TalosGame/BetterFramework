/// <summary>
/// 创建者：dyjia.
/// 时间： 2017-7-15 18:49:05
/// 描述: 用于延迟显示物体
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayActive : MonoBehaviour {

    public GameObject target;
    public float delayTime;
    public bool isActive;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnEnable ()
    {
        target.SetActive(!isActive);
        StartCoroutine(WaitActive());
    }

    IEnumerator WaitActive ()
    {
        yield return new WaitForSeconds(delayTime);
        target.SetActive(isActive);
    }
}
