/// <summary>
/// 创建者：dyjia.
/// 时间： 2017-7-27 17:22:10
/// 描述： 用于控制scroll view的自动到下一个item
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScrollWiewControl : MonoBehaviour {
    
    public float delayTime = 5;
    public List<GameObject> items;
    bool isShow = false;
    public UICenterOnChild center;
    public UIPanel panel;
    GameObject cur,next;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnEnable ()
    {
        isShow = true;
        center.onCenter += OnCenter;
        StartCoroutine(ShowNext ());
    }

    void OnCenter (GameObject go)
    {
        cur = go;
        StopCoroutine("ShowNext");
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == cur)
            {
                if (i != (items.Count - 1))
                {
                    next = items[i + 1];
                }
                else
                {
                    next = items[0];
                }
                break;
            }
        }
        StartCoroutine("ShowNext");
    }

    void OnDisable ()
    {
        isShow = false;
    }

    IEnumerator ShowNext ()
    {
        yield return new WaitForSeconds(delayTime);
        SetCenterItem(next);
    }

    void SetCenterItem (GameObject go)
    {
        if (center != null)
        {
            if (center.enabled)
                center.CenterOn(go.transform);
        }
        else if (panel != null && panel.clipping != UIDrawCall.Clipping.None)
        {
            UIScrollView sv = panel.GetComponent<UIScrollView>();
            Vector3 offset = -panel.cachedTransform.InverseTransformPoint(go.transform.position);
            if (!sv.canMoveHorizontally) offset.x = panel.cachedTransform.localPosition.x;
            if (!sv.canMoveVertically) offset.y = panel.cachedTransform.localPosition.y;
            SpringPanel.Begin(panel.cachedGameObject, offset, 6f);
        }
    }
}
