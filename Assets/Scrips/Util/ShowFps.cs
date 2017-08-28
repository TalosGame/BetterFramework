using UnityEngine;
using System.Collections;
using System;

/**
 * http://www.tuicool.com/articles/eEFfUb
 */
public class ShowFps : MonoBehaviour
{
#if SHOW_FPS

    private float f_UpdateInterval = 0.5f;
    private float f_LastInterval = 0.0f;
    private int i_Frames = 0;
    private float f_Fps = 0.0f;
    private const int i_FontSize = 60;
    private GUIStyle style;
    // Use this for initialization
    void Start()
    {
        f_LastInterval = Time.realtimeSinceStartup;
        i_Frames = 0;
        style = new GUIStyle();
        style.fontSize = i_FontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(46f / 256f, 163f / 256f, 256f / 256f, 256f / 256f);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width/2, 40, 200, 100), "FPS:" + f_Fps.ToString("f0"), style);       
        
    }

    // Update is called once per frame
    void Update()
    {
        ++i_Frames;

        if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval)
        {
            f_Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);

            i_Frames = 0;

            f_LastInterval = Time.realtimeSinceStartup;
        }
    }
#endif

}
