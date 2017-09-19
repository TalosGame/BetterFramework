//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This script makes it possible for a scroll view to wrap its content, creating endless scroll views.
/// Usage: simply attach this script underneath your scroll view where you would normally place a UIGrid:
/// 
/// + Scroll View
/// |- UIWrappedContent
/// |-- Item 1
/// |-- Item 2
/// |-- Item 3
/// </summary>

[AddComponentMenu("NGUI/Interaction/Better Grid")]
public class UIBetterGrid : MonoBehaviour
{
    /// <summary>
    /// 对齐方式
    /// </summary>
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }

    public enum Sorting
    {
        None,
        Alphabetic,
        Horizontal,
        Vertical,
        Custom,
    }

    /// <summary>
    /// Type of arrangement -- vertical, horizontal or cell snap.
    /// </summary>

    public Arrangement arrangement = Arrangement.Horizontal;

    /// <summary>
    /// How to sort the grid's elements.
    /// </summary>

    public Sorting sorting = Sorting.None;

    /// <summary>
    /// Maximum children per line.
    /// If the arrangement is horizontal, this denotes the number of columns.
    /// If the arrangement is vertical, this stands for the number of rows.
    /// </summary>

    public int maxPerLine = 0;

    /// <summary>
    /// The width of each of the cells.
    /// </summary>

    public float cellWidth = 200f;

    /// <summary>
    /// The height of each of the cells.
    /// </summary>

    public float cellHeight = 200f;

	/// <summary>
	/// Whether the content will be automatically culled. Enabling this will improve performance in scroll views that contain a lot of items.
	/// </summary>

	public bool cullContent = true;

	/// <summary>
	/// Minimum allowed index for items. If "min" is equal to "max" then there is no limit.
	/// For vertical scroll views indices increment with the Y position (towards top of the screen).
	/// </summary>

	public int minNum = 0;

	/// <summary>
	/// Maximum allowed index for items. If "min" is equal to "max" then there is no limit.
	/// For vertical scroll views indices increment with the Y position (towards top of the screen).
	/// </summary>

	public int maxNum = 0;

	/// <summary>
	/// Whether hidden game objects will be ignored for the purpose of calculating bounds.
	/// </summary>

	public bool hideInactive = false;

	/// <summary>
	/// Callback that will be called every time an item needs to have its content updated.
	/// The 'wrapIndex' is the index within the child list, and 'realIndex' is the index using position logic.
	/// </summary>
	public delegate void OnUpdateGrid(UIGridItem go, int realIndex);

	public OnUpdateGrid onUpdateGrid;

	protected Transform mTrans;
	protected UIPanel mPanel;
	protected UIScrollView mScroll;
	protected bool mHorizontal = false;
	protected bool mFirstTime = true;
	protected List<Transform> mChildren = new List<Transform>();

    [HideInInspector]
    public Vector2 firstPoint; //初始化位置

    private int rowNum = 0;
    private int colNum = 0;

	/// <summary>
	/// Initialize everything and register a callback with the UIPanel to be notified when the clipping region moves.
	/// </summary>

	protected virtual void Start ()
	{
		SortBasedOnScrollMovement();
		WrapContent();
		if (mScroll != null) mScroll.GetComponent<UIPanel>().onClipMove = OnMove;
		mFirstTime = false;
	}

	/// <summary>
	/// Callback triggered by the UIPanel when its clipping region moves (for example when it's being scrolled).
	/// </summary>

	protected virtual void OnMove (UIPanel panel) { WrapContent(); }

    /// <summary>
    /// Custom sort delegate, used when the sorting method is set to 'custom'.
    /// </summary>

    public System.Comparison<Transform> onCustomSort;

    protected virtual void Sort(List<Transform> list) { }

	/// <summary>
	/// Immediately reposition all children.
	/// </summary>

	[ContextMenu("Sort Based on Scroll Movement")]
	public virtual void SortBasedOnScrollMovement ()
	{
		if (!CacheScrollView()) return;

		// Cache all children and place them in order
		mChildren.Clear();
		for (int i = 0; i < mTrans.childCount; ++i)
		{
			Transform t = mTrans.GetChild(i);
			if (hideInactive && !t.gameObject.activeInHierarchy) continue;
			mChildren.Add(t);
		}

        SortItems();

		ResetChildPositions();
	}

    public void JumpToItem(int itemIdx, int cacheItemNum) 
    {
        if (!CacheScrollView()) return;

        SetChildPositions(itemIdx, cacheItemNum);
    }

    /// <summary>
    /// Clear Child
    /// </summary>
    public void ClearChildren()
    {
        mChildren.Clear();
    }


	/// <summary>
	/// Cache the scroll view and return 'false' if the scroll view is not found.
	/// </summary>

	protected bool CacheScrollView ()
	{
		mTrans = transform;
		mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
		mScroll = mPanel.GetComponent<UIScrollView>();
		if (mScroll == null) return false;
		if (mScroll.movement == UIScrollView.Movement.Horizontal) mHorizontal = true;
		else if (mScroll.movement == UIScrollView.Movement.Vertical) mHorizontal = false;
		else return false;
		return true;
	}

    /// <summary>
    /// Sort child items
    /// </summary>
    private void SortItems()
    {
        // Sort the list using the desired sorting logic
        if (sorting != Sorting.None)
        {
            if (sorting == Sorting.Alphabetic) mChildren.Sort(UIGrid.SortByName);
            else if (sorting == Sorting.Horizontal) mChildren.Sort(UIGrid.SortHorizontal);
            else if (sorting == Sorting.Vertical) mChildren.Sort(UIGrid.SortVertical);
            else if (onCustomSort != null) mChildren.Sort(onCustomSort);
            else Sort(mChildren);
        }
    }

	/// <summary>
	/// Helper function that resets the position of all the children.
	/// </summary>

	protected virtual void ResetChildPositions ()
	{
        int maxGridCount = mChildren.Count;

        if (arrangement == Arrangement.Horizontal)
        {
            colNum = maxPerLine;
            rowNum = Mathf.RoundToInt((float)maxGridCount / colNum);
        }
        else
        {
            rowNum = maxPerLine;
            colNum = Mathf.RoundToInt((float)maxGridCount / rowNum);
        }

        //MSLog.Log("row num===" + rowNum + " col num==" + colNum);

        int x = 0;
        int y = 0;

        // Re-add the children in the same order we have them in and position them accordingly
        for (int i = 0, imax = maxGridCount; i < imax; ++i)
        {
            Transform t = mChildren[i];

            Vector3 pos = t.localPosition;
            float depth = pos.z;

            pos = (arrangement == Arrangement.Horizontal) ? new Vector3(cellWidth * x, -cellHeight * y, depth)
                : new Vector3(cellWidth * y, -cellHeight * x, depth);

            t.localPosition = pos;

            UpdateItem(t, i);

            if (++x >= maxPerLine && maxPerLine > 0)
            {
                x = 0;
                ++y;
            }
        }        
	}

    private void SetChildPositions(int itemIdx, int cacheItemNum)
    {
        // TODO 水平滚动暂不支持
        if(arrangement == Arrangement.Vertical)
            return;

        int maxGridCount = mChildren.Count;
        if (maxGridCount == 0)
            return;

        int lastRowItemIdx = 0;
        if (arrangement == Arrangement.Horizontal)
        {
            colNum = maxPerLine;
            rowNum = Mathf.RoundToInt((float)maxGridCount / colNum);

            int itemRow = (itemIdx / colNum) + 1;
            lastRowItemIdx = itemRow * colNum - 1;
        }
//         else
//         {
//             rowNum = maxPerLine;
//             colNum = Mathf.RoundToInt((float)maxGridCount / rowNum);
//         }

        int startIdx = lastRowItemIdx - rowNum * colNum + 1;
        int x = 0;
        int y = 0;

        // Re-add the children in the same order we have them in and position them accordingly
        for (int i = 0, imax = lastRowItemIdx + 1; i < imax; ++i)
        {
            int childIdx = i - startIdx;
            if (i >= startIdx && childIdx < maxGridCount) 
            {
                Transform t = mChildren[childIdx];

                Vector3 pos = t.localPosition;
                float depth = pos.z;

                pos = (arrangement == Arrangement.Horizontal) ? new Vector3(cellWidth * x, -cellHeight * y, depth)
                    : new Vector3(cellWidth * y, -cellHeight * x, depth);

                t.localPosition = pos;

                UpdateItem(t, i);
            }

            if (++x >= maxPerLine && maxPerLine > 0)
            {
                x = 0;
                ++y;
            }
        }

        Vector3 headItemPos = Vector3.zero;
        Vector4 clipRegion = mPanel.baseClipRegion;
        int viewItemNum = (int)(clipRegion.w / cellHeight) * colNum;
        int cacheItemLeft = cacheItemNum - viewItemNum;

        if (arrangement == Arrangement.Horizontal)
        {
            Transform t = mChildren[0];
            if (cacheItemLeft < mChildren.Count)
                t = mChildren[cacheItemLeft];
            headItemPos = t.localPosition;
        }

        mScroll.transform.localPosition = new Vector3(-headItemPos.x, -headItemPos.y, headItemPos.z);
        mPanel.clipOffset = new Vector2(headItemPos.x, headItemPos.y);
    }

	/// <summary>
	/// Wrap all content, repositioning all children as needed.
	/// </summary>

	public virtual void WrapContent ()
	{
        float extents = 0;
        if(!mHorizontal)
        {
            extents = rowNum * cellHeight * 0.5f;
        }else
        {
            extents = colNum * cellWidth * 0.5f;
        }

        //Debug.Log("WrapContent extents===" + extents);
        
        Vector3[] corners = mPanel.worldCorners;
		for (int i = 0; i < 4; ++i)
		{
			Vector3 v = corners[i];
			v = mTrans.InverseTransformPoint(v);
			corners[i] = v;
		}
		
		Vector3 center = Vector3.Lerp(corners[0], corners[2], 0.5f);
		bool allWithinRange = true;
		float ext2 = extents * 2f;

        int minIdx = minNum;
        int maxIdx = -1 * (maxNum - 1);

        // 判断是否需要循环复用
        bool canWarp = true;
        UILoopScrollView loopSW = mScroll as UILoopScrollView;
        if (maxNum <= loopSW.maxGridNum)
        {
            canWarp = false;
        }

        // 水平滚动
		if (mHorizontal)
		{
			float min = corners[0].x - cellWidth;
			float max = corners[2].x + cellWidth;

			for (int i = 0, imax = mChildren.Count; i < imax; ++i)
			{
				Transform t = mChildren[i];
				float distance = t.localPosition.x - center.x;

                // 水平向左滑动，左边补在后边
				if (distance < -extents)
				{
					Vector3 pos = t.localPosition;
					pos.x += ext2;
					distance = pos.x - center.x;

                    int row = Mathf.Abs(Mathf.RoundToInt(pos.y / cellHeight));
                    int col = Mathf.RoundToInt(pos.x / cellWidth);
                    int realIndex = row + col * rowNum;

                    if (minNum == maxNum || ((realIndex >= minIdx && realIndex <= Math.Abs(maxIdx)) && canWarp))
					{
                        //Debug.Log("Move Horizontal left row====" + row + " col====" + col + " realIndex===" + realIndex);

						t.localPosition = pos;
                        UpdateItem(t, realIndex);
					}
					else allWithinRange = false;
				}
				else if (distance > extents) // 水平向后滑动， 右边补在左边
				{
					Vector3 pos = t.localPosition;
					pos.x -= ext2;
					distance = pos.x - center.x;

                    int row = Mathf.Abs(Mathf.RoundToInt(pos.y / cellHeight));
                    int col = Mathf.RoundToInt(pos.x / cellWidth);
                    int realIndex = row + col * rowNum;

                    if (minNum == maxNum || ((realIndex >= minIdx && realIndex <= Math.Abs(maxIdx)) && canWarp))
					{
                        //Debug.Log("Move Horizontal right row====" + row + " col====" + col + " realIndex===" + realIndex);

						t.localPosition = pos;
                        UpdateItem(t, realIndex);
					}
					else allWithinRange = false;
				}
				else if (mFirstTime) UpdateItem(t, i);

				if (cullContent)
				{
					distance += mPanel.clipOffset.x - mTrans.localPosition.x;
					if (!UICamera.IsPressed(t.gameObject))
						NGUITools.SetActive(t.gameObject, (distance > min && distance < max), false);
				}
			}
		}
		else // 垂直滚动
		{
            float min = corners[0].y - cellHeight;
            float max = corners[2].y + cellHeight;

			for (int i = 0, imax = mChildren.Count; i < imax; ++i)
			{
				Transform t = mChildren[i];
				float distance = t.localPosition.y - center.y;

				if (distance < -extents) // 垂直向下滑动， 下面的挂在上面
				{
					Vector3 pos = t.localPosition;
					pos.y += ext2;
					distance = pos.y - center.y;
                    int realIndex = Mathf.RoundToInt(pos.y / cellHeight);

                    if (minNum == maxNum || ((maxIdx <= realIndex && realIndex <= minIdx) && canWarp))
					{
                        int row = Mathf.Abs(Mathf.RoundToInt(pos.y / cellHeight));
                        int col = Mathf.RoundToInt(pos.x / cellWidth);
                        realIndex = row * colNum + col;

                        //Debug.Log("Move vertical down row====" + row + " col====" + col + " realIndex===" + realIndex);

						t.localPosition = pos;
                        UpdateItem(t, realIndex);
					}
					else allWithinRange = false;
				}
				else if (distance > extents) // 垂直向上滑动， 上面的排在下面
				{
					Vector3 pos = t.localPosition;
					pos.y -= ext2;
					distance = pos.y - center.y;

                    int row = Mathf.Abs(Mathf.RoundToInt(pos.y / cellHeight));
                    int col = Mathf.RoundToInt(pos.x / cellWidth);
                    int realIndex = row * colNum + col;

                    if (minNum == maxNum || ((realIndex >= minIdx && realIndex <= Math.Abs(maxIdx)) && canWarp))
					{
                        //Debug.Log("Move vertical up row====" + row + " col====" + col + " realIndex===" + realIndex);

						t.localPosition = pos;
						UpdateItem(t, realIndex);
					}
					else allWithinRange = false;
				}
				else if (mFirstTime) UpdateItem(t, i);

				if (cullContent)
				{
					distance += mPanel.clipOffset.y - mTrans.localPosition.y;
					if (!UICamera.IsPressed(t.gameObject))
                    { 
                        bool isShow = distance > min && distance < max;
                        NGUITools.SetActive(t.gameObject, isShow, false);
                    }
				}
			}
		}
        mScroll.restrictWithinPanel = !allWithinRange;
        //MSLog.LogError("____ mScroll.restrictWithinPanel = " + mScroll.restrictWithinPanel.ToString());
		mScroll.InvalidateBounds();
	}

	/// <summary>
	/// Sanity checks.
	/// </summary>
	void OnValidate ()
	{
        if (!Application.isPlaying && NGUITools.GetActive(this))
        {
            SortBasedOnScrollMovement();
        }

		if (maxNum < minNum)
			maxNum = minNum;
		if (minNum > maxNum)
			maxNum = minNum;
	}

	/// <summary>
	/// Want to update the content of items as they are scrolled? Override this function.
	/// </summary>

	protected virtual void UpdateItem (Transform item, int index)
	{
        UIGridItem gridItem = item.GetComponent<UIGridItem>();
        if(gridItem != null)
        {
            gridItem.realIndex = index;
        }

		if (onUpdateGrid != null)
		{
			onUpdateGrid(gridItem, index);
		}
	}
}
