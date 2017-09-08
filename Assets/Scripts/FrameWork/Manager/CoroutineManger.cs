using UnityEngine;
using System.Collections;

/// <summary>
/// 该类设计目的
/// 1. 可以使非MonoBehaviour类使用携程
/// 2. 可以使非MonoBehaviour类调用Update方法
/// </summary>
public class CoroutineManger : DDOLSingleton<CoroutineManger>
{
    // 非继承monobehavior类可以调用
    public event UpdateEventHandle updateEventHandle;

    public void RegisterUpdateEvent(UpdateEventHandle handle)
    {
        updateEventHandle += handle;
    }

    public void UnRegisterUpdateEvent(UpdateEventHandle handle)
    {
        updateEventHandle -= handle;
    }

    void Update()
    {
        if(updateEventHandle != null)
        {
            updateEventHandle(Time.deltaTime);
        }
    }
}

