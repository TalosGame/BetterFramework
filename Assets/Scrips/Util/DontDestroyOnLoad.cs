using UnityEngine;
using System.Collections;

/// <summary>
/// 不销毁的作用只针对初始化时候的全部子物体，不包括后加入的子物体
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour {

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
