using UnityEngine;
using System.Collections;

public class UIGridItem : MonoBehaviour 
{
    public GameObject selectedBG;
    public GameObject disabledBG;
    public GameObject normalBG;

    public enum State { None, Normal, Selected, Disabled, Greyed }

    /// <summary>
    /// 真实索引
    /// </summary>
    public int realIndex;

    private State mState = State.None;

    private UILoopScrollView scrollView;

    public State state
    {
        get { return mState; }
        set
        {
            if (mState == value) return;

            mState = value;
            bool isGreyed = mState == State.Greyed;
            if (selectedBG != null) NGUITools.SetActive(selectedBG, mState == State.Selected);
            if (disabledBG != null) NGUITools.SetActive(disabledBG, mState == State.Disabled);
            if (normalBG != null) NGUITools.SetActive(normalBG, isGreyed || mState == State.Normal);
        }
    }

    void Awake()
    {
        if (scrollView == null)
        {
            scrollView = NGUITools.FindInParents<UILoopScrollView>(gameObject);
        }
    }

    public void createBoxCollider()
    {
        if(scrollView == null)
        {
            return;
        }

        Bounds bound = NGUIMath.CalculateRelativeWidgetBounds(this.transform, true);
        bound.size = new Vector3(scrollView.Grid.cellWidth, scrollView.Grid.cellHeight, 0);
        
        BoxCollider boxCollider = this.GetComponent<BoxCollider>();

        //有就用原先的。没有就创建一个新的，中心点和大小跟整个gameobject保持一致
        if (boxCollider == null)
        {
            boxCollider = this.gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.center = new Vector3(bound.center.x, bound.center.y, 0f);
            boxCollider.size = new Vector3(Mathf.Ceil(bound.size.x), Mathf.Ceil(bound.size.y), 0f);
        }
    }

    void OnClick()
    {
        if (scrollView != null && scrollView.onContentClick != null)
            scrollView.onContentClick(scrollView.gameObject, this.gameObject, this.gameObject);
    }
}
