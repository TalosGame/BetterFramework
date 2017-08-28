/// <summary>
/// 创建者：dyjia
/// Date： 2017-7-20 19:26:05
/// 描述： 用于拖拽手牌打牌
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIDragCrad : MonoBehaviour {
    
    public Vector4 rect;
    public Action callback;
    public Action dragStart;
    Vector3 myPos;
    public UISprite bg, dragBg;
    public UISprite fg, dragFg;
    float dis = 0;
    public Transform dragPanel;
    Transform oldRoot;
    UISprite[] childSprites;
    float updown = 0;
    float xy = 0;

	// Use this for initialization
	void Start () {
        myPos = transform.localPosition;
        childSprites = gameObject.GetComponentsInChildren<UISprite>();
	}

    void OnDragStart ()
    {
        if (dragStart != null)
        {
            dragStart();
        }
        oldRoot = bg.transform.parent;
        bg.transform.parent = dragPanel;
        bg.enabled = false;
        bg.enabled = true;
        foreach (UISprite us in childSprites)
        {
            us.enabled = false;
            us.enabled = true;
        }
    }

    void OnDrag (Vector2 delta)
    {
        transform.localPosition += new Vector3(delta.x,delta.y,0);
        dragBg.transform.localPosition = transform.localPosition;
//        dis = Vector3.Distance(transform.localPosition, myPos);
//        dis = Mathf.Abs(dis);
        updown = transform.localPosition.y - myPos.y;
        bool needChangeSprite = false;
        if (updown >= 0)    //up
        {
            if (updown >= rect.x)
            {
                needChangeSprite = true;
            }
        }
        else
        {
            if (Mathf.Abs(updown) >= rect.y)
            {
                needChangeSprite = true;
            }
        }
        xy = transform.localPosition.x - myPos.x; 
        if (xy >= 0)        //right
        {
            if (xy >= rect.w)
            {
                needChangeSprite = true;
            }
        }
        else
        {
            if (Mathf.Abs(xy) >= rect.z)
            {
                needChangeSprite = true;
            }
        }
        DragChangeSprite(needChangeSprite);
    }

    void DragChangeSprite (bool isChange)
    {
        if (isChange)
        {
            bg.enabled = false;
            dragBg.enabled = true;
            dragFg.spriteName = fg.spriteName;
            fg.enabled = false;
            dragFg.enabled = true;
        }
        else
        {
            bg.enabled = true;
            dragBg.enabled = false;
            fg.enabled = true;
            dragFg.enabled = false;
        }
    }

    void OnDragEnd ()
    {
        bg.transform.parent = oldRoot;
        foreach (UISprite us in childSprites)
        {
            us.enabled = false;
            us.enabled = true;
        }
        bg.enabled = true;
        dragBg.enabled = false;
        fg.enabled = true;
        dragFg.enabled = false;
        transform.localPosition = myPos;
        if (callback != null)
        {
            bool needCallBack = false;
            if (updown >= 0)    //up
            {
                if (updown >= rect.x)
                {
                    needCallBack = true;
                }
            }
            else
            {
                if (Mathf.Abs(updown) >= rect.y)
                {
                    needCallBack = true;
                }
            }
            if (xy >= 0)        //right
            {
                if (xy >= rect.w)
                {
                    needCallBack = true;
                }
            }
            else
            {
                if (Mathf.Abs(xy) >= rect.z)
                {
                    needCallBack = true;
                }
            }
            if (needCallBack)
            {
                callback();
            }
        }
    }

    void OnDisable ()
    {
        transform.localPosition = myPos;
        //gameObject.SetActive(true);
    }
}
