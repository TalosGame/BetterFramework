using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LuaInterface;

public sealed class LuaHelper
{
    private static StringBuilder stringBuilder = new StringBuilder();

    #region UI interface
    public static void RegisterWindow(int windowId)
    {
        UIManager.Instance.RegisterWindow(windowId);
    }

    public static void ShowWindow(int windowId)
    {
        UIManager.Instance.ShowWindow(windowId);
    }

	public static void ShowWindow(int windowId, LuaTable winData)
	{
		ShowWindowData data = ShowWindowData.Create ();
		data.forceResetWindow = winData.RawGet<bool>("forceResetWindow");
		data.forceClearBackSeqData = winData.RawGet<bool>("forceClearBackSeqData");
		data.executeNavLogic = winData.RawGet<bool>("executeNavLogic");
		data.checkNavigation = winData.RawGet<bool>("checkNavigation");
		data.hideAllOtherWindow = winData.RawGet<bool>("hideAllOtherWindow");
		data.param = winData["param"];

		UIManager.Instance.ShowWindow(windowId, data);
	}

    public static bool IsWindowShow(int windowId)
    {
        return UIManager.Instance.IsWindowShow(windowId);
    }

    public static void ReturnWindow()
    {
        UIManager.Instance.ReturnWindow();
    }

    public static void CloseWindow(int windowId)
    {
        UIManager.Instance.CloseWindow(windowId);
    }

    public static void CloseAllWindow()
    {
        UIManager.Instance.CloseAllWindow();
    }

    public static Component GetUIWidget(GameObject go, string childName, string type)
    {
        GameObject childObj = MonoExtendUtil.FindDeepChild(go, childName).gameObject;
        if (childObj == null)
            return null;

        Type t = Type.GetType(type);
        return childObj.GetComponent(t);
    }

    public static void AddButtonClick(GameObject go, LuaFunction luafuc, params object[] datas)
    {
        UIEventListener.Get(go).onClick += delegate (GameObject o)
        {
            if (luafuc != null)
            {
                luafuc.BeginPCall();
                luafuc.PushArgs(datas);
                luafuc.PCall();
                luafuc.EndPCall();
            }
        };
    }

    public static void AddButtonClick(GameObject go, LuaFunction luafuc)
    {
        UIEventListener.Get(go).onClick += delegate (GameObject o) {
            if (luafuc != null)
            {
                luafuc.Call();
            }
        };
    }

    public static String GetUILabelText(GameObject go)
    {
        if (go == null)
            return null;
        UILabel uiLable = go.GetComponent<UILabel>();
        if (uiLable == null)
            return null;
        return uiLable.text;
    }

    public static void SetUILabelText(GameObject go, string text)
    {
        if (go == null)
            return;
        UILabel uiLable = MonoExtendUtil.GetOrAddComponent<UILabel>(go);
        uiLable.text = text;
    }

    public static void SetUILabelTextLength(GameObject go, string text, int value)
    {
        if (go == null)
            return;
        UILabel uiLable = MonoExtendUtil.GetOrAddComponent<UILabel>(go);
        uiLable.text = text.PadLeft(value, '0');
    }

    public static void UIToggleValueChange(GameObject go, LuaFunction luafuc)
    {
        if (go == null)
            return;
        UIToggle toggle = go.GetComponent<UIToggle>();
        if (toggle == null)
            return;
        EventDelegate.Add(toggle.onChange, delegate () {
            if (luafuc != null)
            {
                luafuc.Call(toggle.value);
            }
        });
    }

    public static bool UIToggleValueChange(GameObject go)
    {
        if (go == null)
            return false;
        UIToggle uiToggle = go.GetComponent<UIToggle>();
        if (uiToggle == null)
            return false;
        return uiToggle.value;
    }

    public static void SetUIToggleValue(GameObject go, bool value)
    {
        if (go == null)
            return;
        UIToggle toggle = go.GetComponent<UIToggle>();
        if (toggle == null)
            return;
        toggle.value = value;
    }

    public static void DestroyGo(GameObject go, float time)
    {
        if (go == null)
            return;
        GameObject.Destroy(go, time);
    }

    public static Vector3 GetUILabelSize(GameObject go)
    {
        if (go == null)
            return new Vector3(0, 0, 0);
        UILabel uiLabel = go.GetComponent<UILabel>();
        if (uiLabel == null)
            return new Vector3(0, 0, 0);
        return uiLabel.CalculateBounds().size;
    }

    public static Vector2 GetSpritelSize(GameObject go)
    {

        if (go == null)
            return new Vector2(0, 0);
        UISprite uiSpriet = go.GetComponent<UISprite>();
        if (uiSpriet == null)
            return new Vector2(0, 0);
        return uiSpriet.CalculateBounds().size;
    }

    public static void UISlideValueChange(GameObject go, LuaFunction luafuc)
    {
        if (go == null)
            return;
        UISlider slide = go.GetComponent<UISlider>();
        if (slide == null)
            return;
        EventDelegate.Add(slide.onChange, delegate () {
            if (luafuc != null)
            {
                luafuc.Call(slide.value);
            }
        });
    }

    public static float GetUISlideOfValue(GameObject go)
    {
        if (go == null)
            return -1;
        UISlider slide = go.GetComponent<UISlider>();
        if (slide == null)
            return -1;
        return slide.value;
    }

    public static void SetUISlideValue(GameObject go, float value) {
        if (go == null)
            return;
        UISlider slide = go.GetComponent<UISlider>();
        if (slide == null)
            return;
        slide.value = value;
    }

    public static void SetActiveOrFalse(GameObject go, bool isTrue)
    {
        if (go == null)
            return;

        go.SetActive(isTrue);
    }

    public static void SetComponentActive(GameObject go, bool isTrue)
    {
        Collider collider = go.transform.GetComponent<Collider>();
        collider.enabled = isTrue;
    }

    public static void SettingSpringEnable(GameObject go, bool isTrue) {//设置滑动弹性开关
        if (go == null)
            return;
        SpringPanel sp = go.GetComponent<SpringPanel>();
        if (sp == null)
            return;
        sp.enabled = isTrue;
    }

    public static void SettingSprite(GameObject go, string name)//设置sprite 
    {
        if (go == null)
            return;
        UISprite sp = go.GetComponent<UISprite>();
        if (sp == null)
            return;
        sp.spriteName = name;
    }

    public static void SetVector3(GameObject go, float x, float y, float z)//设置ui位置
    {
        if (go == null) return;
        go.transform.localPosition = new Vector3(x, y, z);
    }

    public static Vector3 GetGameObjectLocalPosition(GameObject go)
    {
        Transform tf = go.transform;
        float x = tf.localPosition.x;
        float y = tf.localPosition.y;
        float z = tf.localPosition.z;
        Vector3 v3 = new Vector3(x, y, z);
        return  v3;
    }

    public static void SetLocalScale(GameObject go, float x, float y, float z)
    {
        if (go == null)
            return;

        go.transform.localScale = new Vector3(x, y, z);
    }

    /// <summary>
    /// 设置物体世界坐标
    /// </summary>
    /// <param name="moveTarget">Move target.</param>
    /// <param name="pos">Position.</param>
    public static void SetPosition(GameObject moveTarget, GameObject pos)
    {
        if (moveTarget != null && pos != null)
        {
            moveTarget.transform.position = new Vector3(pos.transform.position.x, pos.transform.position.y, 0);
        }
    }

    /// <summary>
    /// 设置物体相对坐标
    /// </summary>
    /// <param name="moveTarget">Move target.</param>
    public static void SetLocalPosition(GameObject moveTarget, GameObject pos)
    {
        if (moveTarget != null && pos != null)
        {
            moveTarget.transform.localPosition = pos.transform.localPosition;
        }
    }

    public static void SetSpriteSize(GameObject go, int x)
    {
        if (go == null)
            return;
        UISprite sprite = go.GetComponent<UISprite>();
        if (sprite == null)
            return;
        sprite.width = x;
    }

    public static void SetPanelOffset(GameObject go, float x, float y)//设置panel 遮挡的位置
    {
        if (go == null) return;
        UIPanel panel = go.GetComponent<UIPanel>();
        if (panel == null) return;
        panel.clipOffset = new Vector2(x, y);
    }


    /// <summary>
    /// 打开一个链接.
    /// </summary>
    /// <param name="url">URL.</param>
    public static void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    /// <summary>
    /// 设置图片进度条显示进度.
    /// </summary>
    /// <param name="f">F.</param>
    public static void SetSpriteFillAmount(GameObject go, float f)
    {
        UISprite sp = MonoExtendUtil.GetOrAddComponent<UISprite>(go);
        sp.fillAmount = f;
    }

    /// <summary>
    /// 设置物体角度
    /// </summary>
    public static void ChangeLocalRotation(GameObject go, Vector3 v3)
    {
        go.transform.localRotation = Quaternion.Euler(v3);
    }

    /// <summary>
    /// 添加子物体并返回创建结果
    /// </summary>
    public static GameObject AddChild(GameObject parent, GameObject prefab)
    {
        GameObject go = NGUITools.AddChild(parent, prefab);
        return go;
    }

    /// <summary>
    /// 改变父节点，不改变旋转值
    /// </summary>
    /// <param name="parent">Parent.</param>
    /// <param name="child">Child.</param>
    public static void SetParent(Transform parent, Transform child)
    {
        child.parent = parent;
        child.localPosition = Vector3.zero;
        child.localScale = Vector3.one;
    }

    /// <summary>
    /// 刷新UIgrid内节点的位置
    /// </summary>
    /// <returns><c>true</c>, if grid was updated, <c>false</c> otherwise.</returns>
    /// <param name="go">Go.</param>
    public static void UpdateGrid(GameObject go)
    {
        UIGrid ug = MonoExtendUtil.GetOrAddComponent<UIGrid>(go);
        ug.Reposition();
    }

    /// <summary>
    /// 刷新UItable内节点的位置
    /// </summary>
    /// <returns><c>true</c>, if table was updated, <c>false</c> otherwise.</returns>
    /// <param name="go">Go.</param>
    public static void UpdateTable(GameObject go)
    {
        UITable ug = MonoExtendUtil.GetOrAddComponent<UITable>(go);
        ug.Reposition();
    }

    /// <summary>
    /// 牌桌倒计时
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="start">开始Date.</param>
    /// <param name="stop">结束Date.</param>
    /// <param name="speed">间隔速度.</param>
    public static void SetCountLabelTime(GameObject go, int start, int stop, int speed)
    {
        LabelCount lc = MonoExtendUtil.GetOrAddComponent<LabelCount>(go);
        lc.enabled = false;
        lc.startTime = start;
        lc.stopTime = stop;
        lc.speed = speed;
        lc.enabled = true;
    }

    /// <summary>
    /// 停止牌桌倒计时
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="str">String.</param>
    public static void StopLabelCount(GameObject go, string str)
    {
        LabelCount lc = MonoExtendUtil.GetOrAddComponent<LabelCount>(go);
        lc.label.text = str;
        lc.enabled = false;
    }

    /// <summary>
    /// 立即删除当前物体
    /// </summary>
    /// <param name="go">Go.</param>
    public static void DestroyGameObject(GameObject go)
    {
        if (go != null)
        {
            go.transform.parent = null;
            GameObject.Destroy(go);
        }
        else
        {
            Debugger.LogError("can not destroy null object");
        }
    }

    /// <summary>
    /// 解除子物体的父子关系并删除所有子节点。
    /// </summary>
    public static void DestroyChildren(GameObject go)
    {
        go.transform.DestroyChildren();
        GC.Collect();
    }

    /// <summary>
    /// 更改UISprite图片名称
    /// </summary>
    /// <param name="go">Go.</param>
    public static void ChangeSpriteName(GameObject go, string name)
    {
        UISprite us = MonoExtendUtil.GetOrAddComponent<UISprite>(go);
        us.spriteName = name;
        UIButton bt = go.GetComponent<UIButton>();
        if (bt != null)
        {
            bt.normalSprite = name;
        }
    }
    public static void SnapSprite(GameObject go)
    {
        UISprite us = MonoExtendUtil.GetOrAddComponent<UISprite>(go);
        us.MakePixelPerfect();
    }

    public static void ChangeTextureName(GameObject go, string name)
    {
        UITexture ut = MonoExtendUtil.GetOrAddComponent<UITexture>(go);
        Debugger.Log(name);
        Debugger.Log(ut.mainTexture);
       // ut.name= name;
    }

    /// <summary>
    /// 分割字符串
    /// </summary>
    /// <param name="go">Go.</param>
    public static string[] StringSplit(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;
        string[] sArray = str.Split(':');
        return sArray;
    }

	/// <summary>
	/// 转换时间戳
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static long GetTimeLong(string str)
    {
        DateTime dt2 = Convert.ToDateTime(str);
        string time1 = string.Format("{0:MMddHHmmss}", dt2);
        long time2 = long.Parse(time1);
        Debugger.Log(time2);
        return time2;
    }

    public static string GetTimeString(string str)
    {
        string s1 = str.Replace("-","");
        string s2 = s1.Replace(":", "");
        string s3 = s2.Replace(" ", "");
        return s3;
    }

    /// <summary>
    /// 固定位置插入字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string StringConect(string str,int value)
    {
        if (str.Length > value)
        {
            string newstr=str.Insert(value, "##");
            newstr = newstr.Replace("##", "\n");
            return newstr;
        }
        return str;
    }
	/// <summary>
	/// 处理开房记录中时间显示的字符
	/// </summary>
	public static string StringTime(string str)
    {
        if (str[str.Length - 1] == ':')
        {
            string s = str.Substring(0, 10);
            s = s.Replace("-", "/");
            return s;
        }
        return str = str.Replace("-", "/");
    }

    /// <summary>
    /// 获取当前物体层次
    /// </summary>
    /// <returns>The depth.</returns>
    /// <param name="go">Go.</param>
    public static int GetDepth(GameObject go)
    {
        UISprite us = MonoExtendUtil.GetOrAddComponent<UISprite>(go);
        return us.depth;
    }

    /// <summary>
    /// 设置图片层次
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="depth">Depth.</param>
    public static void SetDepth(GameObject go, int depth)
    {
        UISprite us = MonoExtendUtil.GetOrAddComponent<UISprite>(go);
        us.depth = depth;
    }

    /// <summary>
    /// 获取物体当前显示状态
    /// </summary>
    /// <returns><c>true</c>, if active was gotten, <c>false</c> otherwise.</returns>
    /// <param name="go">Go.</param>
    public static bool GetActive(GameObject go)
    {
        return go.activeInHierarchy;
    }

    /// <summary>
    /// 设置子物体显示状态
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="state">If set to <c>true</c> state.</param>
    public static void SetActiveChildren(GameObject go, bool state)
    {
        NGUITools.SetActiveChildren(go, state);
    }

    public static void AddScrollViewContentUpdate(GameObject go, LuaFunction luaFunc)
    {
        if (go == null || luaFunc == null)
            return;

        UILoopScrollView loopScrollView = go.GetComponent<UILoopScrollView>();
        if (loopScrollView == null)
            return;

        loopScrollView.onContentUpdate = delegate (GameObject scrollView, GameObject item, object data)
        {
            luaFunc.Call<GameObject, GameObject, object>(scrollView, item, data);
        };
    }

    public static void AddScrollViewItemClick(GameObject go, LuaFunction luaFunc)
    {
        if (go == null || luaFunc == null)
            return;

        UILoopScrollView loopScrollView = go.GetComponent<UILoopScrollView>();
        if (loopScrollView == null)
            return;

		loopScrollView.onContentClick = delegate (GameObject scrollView, GameObject item, object data)
        {
			luaFunc.Call<GameObject, GameObject, object>(scrollView, item, data);
        };
    }

    public static void SetScrollViewData(GameObject go, LuaTable data)
    {
        if (go == null)
            return;

        UILoopScrollView loopScrollView = go.GetComponent<UILoopScrollView>();
        if (loopScrollView == null)
            return;

        loopScrollView.SetData(data);
    }

    public static void AddScrollViewData(GameObject go, LuaTable data)
    {
        if (go == null)
            return;

        UILoopScrollView loopScrollView = go.GetComponent<UILoopScrollView>();
        if (loopScrollView == null)
            return;

        loopScrollView.AddData(data);
    }

    public static void AddScrollViewData(GameObject go, object data)
    {
        if (go == null)
            return;

        UILoopScrollView loopScrollView = go.GetComponent<UILoopScrollView>();
        if (loopScrollView == null)
            return;

        loopScrollView.AddData(data);
    }

    public static void CleanScrollViewDatas(GameObject go)
    {
        if (go == null)
            return;

        UILoopScrollView loopScrollView = go.GetComponent<UILoopScrollView>();
        if (loopScrollView == null)
            return;

        loopScrollView.ClearDatas();
    }

    public static bool IsDragScrollViewBottom(GameObject go)
    {
        UILoopScrollView loopScrollView = go.GetComponent<UILoopScrollView>();

        if (loopScrollView == null)
            return false;

        return loopScrollView.IsDragScrollViewBottom();
    }

    public static void SetScrollViewPanelOffset(GameObject go,float x,float y)
    {
        UIScrollView scrollview = go.GetComponent<UIScrollView>();
        if (scrollview == null)
            return;
        scrollview.panel.clipOffset = new Vector2(x,y);
    }

    /// <summary>
    /// 拖拽手牌添加事件处理
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="luafuc">Luafuc.</param>
    /// <param name="distance">Distance.</param>
    public static void DragCard(GameObject go, LuaFunction start, LuaFunction luafuc, params object[] datas)
    {
        UIDragCrad dc = go.GetComponent<UIDragCrad>();
        if (dc != null)
        {
            dc.dragStart += delegate
            {
                if (start != null)
                {
                    start.BeginPCall();
                    start.PushArgs(datas);
                    start.PCall();
                    start.EndPCall();
                }
            };
            dc.callback += delegate {
                if (luafuc != null)
                {
                    luafuc.BeginPCall();
                    luafuc.PushArgs(datas);
                    luafuc.PCall();
                    luafuc.EndPCall();
                }
            };
        }
        else
        {
            Debugger.LogError("can not find uidragcard");
        }
    }

    /// <summary>
    /// UISprite脚本开关
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="enable">If set to <c>true</c> enable.</param>
    public static void SetSpriteEnable(GameObject go, bool enable)
    {
        if (go != null)
        {
            UISprite us = go.GetComponent<UISprite>();
            if (us != null)
            {
                us.enabled = enable;
            }
        }
    }

    /// <summary>
    /// 设置ui组件的宽高
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public static void SetUIWidgetSize (GameObject go, int width, int height)
    {
        UIWidget uw = go.GetComponent<UIWidget>();
        if (uw != null)
        {
            uw.width = width;
            uw.height = height;
        }
    }

    /// <summary>
    /// 改变UI颜色
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
    public static void ChangeWidgetColor (GameObject go, int r, int g, int b, int a)
    {
        UIWidget uw = go.GetComponent<UIWidget>();
        if (uw != null)
        {
            float fr = r / 255;
            float fg = g / 255;
            float fb = b / 255;
            float fa = a / 255;

            uw.color = new Color(fr, fg, fb, fa);
        }
    }

    /// <summary>
    /// 给按钮添加onpress事件
    /// </summary>
    public static void AddPressEvent (GameObject go, LuaFunction luafuc, object self)
    {
        if (go != null)
        {
            UIEventListener.Get(go).onPress = delegate(GameObject o, bool state) {
                luafuc.Call(self ,o.name, state);
            };
        }
    }
    #endregion

    #region PlayerPrefs interface
    public static void PlayerPrefsSetting(string pp, float value) {
        PlayerPrefs.SetFloat(pp, value);
        PlayerPrefs.Save();
    }
    public static float PlayerPrefsGetting(string pp)
    {
        if (PlayerPrefs.HasKey(pp))
            return PlayerPrefs.GetFloat(pp);
        else
            return -1;
    }
    public static bool PlayerPrefsHas(string pp) {
        return PlayerPrefs.HasKey(pp);
    }
    public static void PlayerPrefsDeleteKey(string pp) {
        if (!PlayerPrefs.HasKey(pp))
            return;
        else
            PlayerPrefs.DeleteKey(pp);
    }
    public static void PlayerPrefsSettingString(string pp, string value)
    {
        PlayerPrefs.SetString(pp, value);
        PlayerPrefs.Save();
    }
    public static string PlayerPrefsGettingString(string pp)
    {
        if (PlayerPrefs.HasKey(pp))
            return PlayerPrefs.GetString(pp);
        else
            return "";
    }
    #endregion

    #region UITween interface
    public static void PlayTweenPosition(GameObject go, Vector3 vFrom, Vector3 vTo, float time, LuaFunction luafun)
    {
        TweenPosition tp = MonoExtendUtil.GetOrAddComponent<TweenPosition>(go);
        tp.from = vFrom;
        tp.to = vTo;
        tp.duration = time;
        tp.ResetToBeginning();
        tp.style = UITweener.Style.Once;
        tp.enabled = true;

        EventDelegate callBack = new EventDelegate(delegate
        {
            if (luafun != null)
                luafun.Call();
        });
        callBack.oneShot = true;
        tp.onFinished.Add(callBack);

        tp.PlayForward();
    }

    public static void PlayTweenScale(GameObject go, Vector3 vFrom, Vector3 vTo, float time1,float time2)
    {
        TweenScale ts = MonoExtendUtil.GetOrAddComponent<TweenScale>(go);
        ts.from = vFrom;
        ts.to = vTo;
        ts.duration = time1;
        ts.delay = time2;
        ts.style = UITweener.Style.Once;
        ts.enabled = true;
        ts.PlayForward();
    }

    public static void PlayTweenScaleReverse(GameObject go)
    {
        TweenScale ts = MonoExtendUtil.GetOrAddComponent<TweenScale>(go);
        ts.enabled = true;
        ts.PlayReverse();
    }

    /// <summary>
    /// 播放UI上自带的动画
    /// </summary>
    /// <param name="go">Go.</param>
    public static void PlayTweenPosition(GameObject go)
    {
        TweenPosition tp = MonoExtendUtil.GetOrAddComponent<TweenPosition>(go);
        tp.ResetToBeginning();
        tp.enabled = true;
        tp.Play();
    }

    public static void PlayTweenScale(GameObject go)
    {
        TweenScale tp = MonoExtendUtil.GetOrAddComponent<TweenScale>(go);
        tp.ResetToBeginning();
        tp.enabled = true;
        tp.Play();
    }

    /// <summary>
    /// 设置UIButton安装状态
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="isEnable">If set to <c>true</c> is enable.</param>
    public static void SetUIButtonEnable(GameObject go, bool isEnable)
    {
        UIButton bt = go.GetComponent<UIButton>();
        if (bt != null)
        {
            BoxCollider bc = go.GetComponent<BoxCollider>();
            UIWidget[] uws = go.GetComponentsInChildren<UIWidget>();
            if (isEnable)
            {
                bt.SetState(UIButtonColor.State.Normal, true);
                bc.enabled = true;
                foreach (UIWidget uw in uws)
                {
                    uw.color = bt.defaultColor;
                }
            }
            else
            {
                bt.SetState(UIButtonColor.State.Disabled, true);
                bc.enabled = false;
                foreach (UIWidget uw in uws)
                {
                    uw.color = bt.disabledColor;
                }
            }
        }
    }

    /// <summary>
    /// 设置碰撞体状态。
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="isEnable">If set to <c>true</c> is enable.</param>
    public static void SetBoxColliderEnable(GameObject go, bool isEnable)
    {
        BoxCollider bc = go.GetComponent<BoxCollider>();
        if (bc != null)
        {
            UIButton bt = go.GetComponent<UIButton>();
            if (bt != null)
            {
                if (isEnable)
                {
                    bc.enabled = isEnable;
                    bt.enabled = isEnable;
                }
                else
                {
                    bt.enabled = isEnable;
                    bc.enabled = isEnable;
                }
            }
            bc.enabled = isEnable;
        }
    }
    #endregion

    #region scene interface
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    #endregion

    /*
    #region sound interface
    public static void PreloadSound(string name, string path)
    {
        AudioManager.Instance.PreloadAudio(name, path);
    }

    public static void PlaySound(string name)
    {
        AudioManager.Instance.PlayAudio(name);
    }

    public static void StopAudio(string name)
    {
        AudioManager.Instance.StopAudio(name);
    }

    public static void StopAudios(int type)
    {
        AudioType audioType = (AudioType)type;
        AudioManager.Instance.StopAudios(audioType);
    }

    public static void StopAudios()
    {
        AudioManager.Instance.StopAudios();
    }

    public static void MusicSetting(bool isPlaying)
    {
        AudioManager.Instance.MusicSetting(isPlaying);
    }

    public static void SoundSetting(bool isPlaying)
    {
        AudioManager.Instance.SoundSetting(isPlaying);
    }
    public static void ChangeMusicVolum(int type, float volum) {
        AudioType audioType = (AudioType)type;
        AudioManager.Instance.AddOrSubAudiosVolume(audioType, volum);
    }
    #endregion
    */

    #region Resource interface
    public static void AddWindowResPath(int windowID, string path)
    {
        MLResourceManager.Instance.AddExUIWindow(windowID, path);
    }

    public static void AddExResourcePath(int typeID, string path)
    {
        MLResourceManager.Instance.AddExResourcePath(typeID, path);
    }

    public static GameObject LoadUIItem(string name)
    {
        stringBuilder.Length = 0;
        stringBuilder.AppendFormat("UIItem/{0}", name);

        string path = stringBuilder.ToString();
        return (GameObject)MLResourceManager.Instance.LoadInstance(path);
    }
    public static void LoadUITexture(GameObject go, string name)//加载图片
    {
        if (go == null)
            return;
        stringBuilder.Length = 0;
        stringBuilder.AppendFormat("UITextrues/{0}", name);
        string path = stringBuilder.ToString();
        Texture t = (Texture)MLResourceManager.Instance.LoadResource(path);
        UITexture tt = go.GetComponent<UITexture>();
        if (tt == null)
            return;
        tt.mainTexture = t;
    }
    #endregion

    #region NetWork interface
    public static void StartSendHeartMsg(int second)
    {
        GameSocket.Instance.HeartTime = second;
        GameSocket.Instance.StartSendHeartMsg();
    }

    // 连接服务器
    public static void ConnectServer(int socketState, string ip, int port)
    {
        GameSocket.Instance.ConnectToServer(socketState, ip, port);
    }

    // 重新连接
    public static void ReConnection()
    {
        GameSocket.Instance.ReConnection();
    }

    // 改变连接状态
    public static void ChangeSocketState(int socketState)
    {
        GameSocket.Instance.ChangeSocketState(socketState);
    }

    // 关闭连接
    public static void CloseSocket(int socketState)
    {
        GameSocket.Instance.CloseSocket(socketState);
    }

    //判断网络是否可用
    public static bool IsNetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    // 发送消息
    public static void SendMessage(int msgId, LuaByteBuffer buffer)
    {
        GameSocket.Instance.SendMessage(msgId, buffer.buffer);
    }

    // 发送验证消息
    public static void SendVerifyMessage(int type)
    {
        ByteBuffer buff = new ByteBuffer();
        buff.WriteInt(GameConst.MSG_VERIFY_ID);
        buff.WriteInt(4);
        buff.WriteInt(type);

        byte[] bytes = buff.ToBytes();
        buff.Close();

        GameSocket.Instance.SendMessageNoCallBack(bytes);
    }
    #endregion

    #region hot update & loading interface
    // 初始cdn服务器url地址
    public static void InitCdnServerUrl(string url)
    {
        HotUpdateMgr hpMgr = HotUpdateMgr.Instance;
        hpMgr.CdnServerURL = url;
    }

    /// <summary>
    /// 检查游戏是否需要更新
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public static bool CheckGameNeedHotUpdate(int productId)
    {
        return HotUpdateMgr.Instance.CheckGameHotUpdate(productId);
    }

    /// <summary>
    /// 开始游戏热更新
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="luaFunc"></param>
    public static void DOGameHotUpdateInLua(int productId, LuaFunction luaFunc)
    {
        HotUpdateMgr hpMgr = HotUpdateMgr.Instance;
        hpMgr.DoHotUpdateComplete = delegate
        {
            if (luaFunc != null)
            {
                luaFunc.Call();
            }
        };

        hpMgr.DOGameHotUpdate(productId, true);
    }

    /// <summary>
    /// 设置当前玩的产品id
    /// </summary>
    /// <param name="productId"></param>
    public static void PlayingProduct(int productId)
    {
        LocalResConfigMgr.Instance.ProductId = productId;
    }

    public static void DOGameHotUpdateInCS(int productId, LuaFunction luaFunc)
    {
        HotUpdateMgr hpMgr = HotUpdateMgr.Instance;
        hpMgr.DoHotUpdateComplete = delegate
        {
            if (luaFunc != null)
                luaFunc.Call();
        };

        // 显示热更新界面
        UIManager.Instance.ShowWindow(UIWindowID.WINDOWID_HOT_RES_LOADING);

        hpMgr.DOGameHotUpdate(productId, false);
    }

    public static void LoadingGame(LuaFunction luaFunc)
    {
#if STREAM_ASSET
        LoadingManager loadingMgr = LoadingManager.Instance;
        loadingMgr.LoadingComplete = delegate
        {
            if (luaFunc != null)
                luaFunc.Call();
        };

        loadingMgr.StartLoadRes(ResLoadingStatus.GameRes);
#else
        // 清空所有声音
        //AudioManager.Instance.RelaseAllAudios();

        // 清除所有资源
        MLResourceManager.Instance.UnloadAllResource(unloadObject: true);

        UIManager.Instance.CloseAllWindow();

        SceneManager.LoadScene(GameConst.GAME_SCENE);

        if (luaFunc != null)
            luaFunc.Call();
#endif
    }

    public static void ExitGame(LuaFunction luaFunc)
    {
#if STREAM_ASSET
        LoadingManager loadingMgr = LoadingManager.Instance;
        loadingMgr.LoadingComplete = delegate
        {
            if (luaFunc != null)
                luaFunc.Call();
        };

        loadingMgr.StartLoadRes(ResLoadingStatus.ExitGame);
#else
        // 清空所有声音
        //AudioManager.Instance.RelaseAllAudios();

        // 清除所有资源
        MLResourceManager.Instance.UnloadAllResource(unloadObject: true);

        SceneManager.LoadScene(GameConst.LOBBY_SCENE);

        if (luaFunc != null)
            luaFunc.Call();
#endif
    }
    #endregion

    #region event handle interface
    public static void PostNotification(string name)
    {
        NotificationCenter.Instance.PostNotification(name);
    }

    public static void PostNotification(string name, object obj)
    {
        NotificationCenter.Instance.PostNotification(name, obj);
    }
    #endregion

    #region string interface
    public static string ConvertUTF8(string str)
    {
        Debugger.Log("ConvertUTF8 str=========" + str);

        Encoding gbkEncoding = Encoding.GetEncoding("gb18030");
        byte[] bytes = gbkEncoding.GetBytes(str);
        string ret = Encoding.UTF8.GetString(bytes);
        return ret;
    }

    public static string ConvertGbk2UTF8(string str)
    {
        Debugger.Log("ConvertGbk2UTF8 str=========" + str);
        Encoding gbkEncoding = Encoding.GetEncoding("gbk");
        byte[] bytes = gbkEncoding.GetBytes(str);
        string ret = Encoding.UTF8.GetString(bytes);
        return str;
    }
    #endregion

    /// <summary>
    /// 判断是否是安卓平台
    /// </summary>
    /// <returns></returns>
    public static bool IsAndroidOrPC() {
#if UNITY_STANDALONE || UNITY_EDITOR|| SHAREWECHAT
            return false;
#else
            return true;
#endif
    }

	public static void ExitApp()
	{
		Application.Quit();
	}

	#region call lua interface
	private static string GetLuaFuncName(string module, string func)
	{
		stringBuilder.Length = 0;
		stringBuilder.AppendFormat("{0}.{1}", module, func);
		return stringBuilder.ToString();
	}

	public static void CallLuaFunction(string module, string func)
	{
		LuaManager luaMgr = LuaManager.Instance;
		if (luaMgr == null)
			return;

		LuaFunction luaFunc = luaMgr.GetLuaFunction(GetLuaFuncName(module, func));
		if (luaFunc == null)
			return;

		luaFunc.Call();
		luaFunc.Dispose();
		luaFunc = null;
	}

	public static void CallLuaDirectFunction(string name, params object[] datas)
	{
		LuaManager luaMgr = LuaManager.Instance;
		if (luaMgr == null)
			return;

		LuaFunction luaFunc = luaMgr.GetLuaFunction(name);
		if (luaFunc == null)
			return;

		luaFunc.BeginPCall();
		luaFunc.PushArgs(datas);
		luaFunc.PCall();
		luaFunc.EndPCall();
		luaFunc.Dispose();
		luaFunc = null;
	}

	public static void CallLuaFunction(string module, string func, params object[] datas)
	{
		CallLuaDirectFunction(GetLuaFuncName(module, func), datas);
	}
	#endregion
}
