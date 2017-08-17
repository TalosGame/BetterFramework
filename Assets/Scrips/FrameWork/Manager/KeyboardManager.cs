using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 创建者：dyjia
/// Date： 2017-8-10 22:48:03
/// 描述： 用于监听键盘事件
/// </summary>
public class KeyboardManager : DDOLSingleton<KeyboardManager>
{
    public Action back, home, menu;

	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (back != null)
            {
                back();
            }
        }
        if (Input.GetKeyUp(KeyCode.Home))
        {
            if (home != null)
            {
                home();
            }
        }
        if (Input.GetKeyUp(KeyCode.Menu))
        {
            if (menu != null)
            {
                menu();
            }
        }
	}
}
