using UnityEngine;
using System.Collections;

public class UILoading : UIWindowBase 
{
    protected UIProgressBar progressBar;
    protected UILabel progressLab;

	protected override void InitWindowData ()
	{
		
	}

    /// <summary>
    /// 设置进度条的值
    /// </summary>
    /// <param name="value"></param>
    public void SetProgressValue(float value)
    {
        progressBar.value = value;
    }

    /// <summary>
    /// 设置加载内容
    /// </summary>
    public void SetProgressInfo(string info)
    {
        progressLab.text = info;
    }
}
