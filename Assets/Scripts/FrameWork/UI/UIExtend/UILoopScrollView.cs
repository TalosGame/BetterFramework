using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

public class UILoopScrollView : UIScrollView 
{
    ///------------------delegate 相关----------------------------///
	public delegate void OnContentClick(GameObject scrollView, GameObject item, object data);

    public OnContentClick onContentClick;

    public delegate void OnContentUpdate(GameObject scrollView, GameObject go, object data);

    public OnContentUpdate onContentUpdate;
    ///-----------------------------------------------------------///
    
    // 缓存item根节点名称
    private const string CACHE_ITEM_ROOT = "CacheItemRoot";

    /// <summary>
    /// 单元格预制名称
    /// </summary>
    public string itemPrefabPath;

    /// <summary>
    /// 最多创建单元格预制数
    /// </summary>
    public int maxGridNum = 10;

    /// <summary>
    /// 是否自动刷新单元格范围
    /// </summary>
    public bool autoCountGrid = true;

    /// <summary>
    /// 缓存item根节点
    /// </summary>
    private Transform cacheItemRoot;

    /// <summary>
    /// 缓存多item预制
    /// </summary>
    private Dictionary<string, Queue<GameObject>> cacheItemDic = new Dictionary<string, Queue<GameObject>>();

    /// <summary>
    /// 是否加载数据完成
    /// </summary>
    public bool loadComplete = false;

    /// <summary>
    /// 记录滚动层开始坐标
    /// </summary>
    private Vector3 startPos = Vector3.zero;

    /// <summary>
    /// 视图数据
    /// </summary>
    private List<object> datas = new List<object>();

    /// <summary>
    /// 是否缓存过
    /// </summary>
    private bool preloadItemComplete = false;

    /// <summary>
    /// 跳转id
    /// </summary>
    private int jumpIndex = 0;

    private UIBetterGrid grid;
    public UIBetterGrid Grid
    {
        get { return grid; }
    }

    private List<UIGridItem> items = new List<UIGridItem>(); 

    public bool resetPos = true;
    public bool DragLastIndex = false;

    protected override void Awake()
    {
        base.Awake();

        grid = transform.GetComponentInChildren<UIBetterGrid>();
        if(grid == null)
        {
            Debugger.LogError("Need UIBetterGrid Component!!");
            return;
        }

        grid.onUpdateGrid = OnUpdateGrid;

        // 预创建缓存item根节点
        cacheItemRoot = MonoExtendUtil.FindDeepChild(gameObject, CACHE_ITEM_ROOT);
        if (cacheItemRoot == null)
        {
            cacheItemRoot = new GameObject("CacheItemRoot").transform;
            MonoExtendUtil.AddChildToTarget(transform, cacheItemRoot);
        }
    }

    #region 加载相关
    public void SetData(List<object> itemDatas)
    {
        ClearDatas();

        for (int i = 0; i < itemDatas.Count; i++)
        {
            datas.Add(itemDatas[i]);
        }
        PreLoadCacheItems();

        loadComplete = true;
        resetPos = true;
    }

    public void SetData(LuaTable v)
    {
        ClearDatas();

        for (int i = 0; i < v.Length; i++)
        {
            object o = v.GetObjectByIndex(i);
            AddData(o);
        }

        preloadItemComplete = false;
        PreLoadCacheItems();

        loadComplete = true;
        resetPos = true;
    }

    public void AddData(LuaTable v)
    {
        for (int i = 0; i < v.Length; i++)
        {
            object o = v.GetObjectByIndex(i);
            AddData(o);
        }

        PreLoadCacheItems();
        loadComplete = true;
        resetPos = false;
    }

    public void ChangeData(object data)
    {
        for (int j = 0; j < items.Count; j++)
        { 
            UIGridItem item = items[j];
            if (data == item.Data)
            { 
                if(onContentUpdate != null)
                {
                    onContentUpdate(gameObject, item.gameObject, data);
                }

                break;
            }
        }        
    }

    /// <summary>
    /// 预加载缓存item对象
    /// </summary>
    private void PreLoadCacheItems()
    {
        if (preloadItemComplete)
            return;
        Queue<GameObject> cacheItems = null;
        if(!cacheItemDic.TryGetValue(itemPrefabPath, out cacheItems))
        {
            cacheItems = new Queue<GameObject>();
            cacheItemDic.Add(itemPrefabPath, cacheItems);
        }

        if(cacheItems.Count >= maxGridNum)
        {
            preloadItemComplete = true;
            return;
        }
        for (int i = 0; i < maxGridNum; i++)
        {
			GameObject cell = MLResourceManager.Instance.LoadInstance(itemPrefabPath, ResourceType.RES_UI) as GameObject;
            cell.SetActive(false);
            cell.name = cell.name + i;

            cacheItems.Enqueue(cell);
        }

        preloadItemComplete = true;
    }

    private Queue<GameObject> GetCacheItems()
    { 
        Queue<GameObject> cacheItems = null;
        if (!cacheItemDic.TryGetValue(itemPrefabPath, out cacheItems))
        {
            return null;
        }

        return cacheItems;
    }

    void LateUpdate()
    {
        if (loadComplete)
        {
            CreateGridCells();

            loadComplete = false;

            if (jumpIndex != 0)
            {
                grid.JumpToItem(jumpIndex, maxGridNum);
                RestrictWithinBounds(true, canMoveHorizontally, canMoveVertically);
                jumpIndex = 0;
            }
        }

        //if (grid.transform.childCount > 0)
           // Debug.Log("===last=====" + grid.transform.GetChild(grid.transform.childCount -1).transform.localPosition.y);
        base.LateUpdate();
    }

    public bool IsDragScrollViewBottom()
    {
        

//            Bounds bound = this.bounds;
//         float boundY = bound.size.y;
//         float viewY = this.panel.GetViewSize().y;
//         float offsetY = this.panel.clipOffset.y;
// 
// 
//         if (-offsetY - viewY + Grid.transform.position.y >= boundY)
//         {
//            // Debug.Log("boundY===" + boundY + " viewY===" + viewY + " offsetY===" + offsetY);
//             return false;
//         } 

        return DragLastIndex;
    }
    

    public void JumpToIndex(int index) 
    {
        if (grid == null)
        {
            return;
        }
        if (index < 0 )
        {
            return;
        }
        if (index > datas.Count)
            index = datas.Count - 1;
        if (loadComplete)
        {
            jumpIndex = index;
        }
        else
        {
            grid.JumpToItem(index, maxGridNum);
            RestrictWithinBounds(true, canMoveHorizontally, canMoveVertically);
        }
    }

    /// <summary>
    /// 创建表单元格
    /// </summary>
    private void CreateGridCells()
    {
        if(grid == null)
        {
            return;
        }

        if (datas.Count == grid.transform.childCount || grid.transform.childCount >= maxGridNum)
        {
            RefreshScrollView( true );
            return;
        }

        Queue<GameObject> cacheItems = null;
        if(!cacheItemDic.TryGetValue(itemPrefabPath, out cacheItems))
        {
            return;
        }

        int hasCount = grid.transform.childCount;
        for (int i = 0; i < datas.Count - hasCount; i++)
        {
            if(i >= maxGridNum || cacheItems.Count <= 0)
            {
                break;
            }

            GameObject cell = cacheItems.Dequeue();
            if (cell == null)
                continue;

            MonoExtendUtil.AddChildToTarget(grid.transform, cell.transform);
            NGUITools.SetActive(cell, true);

            UIGridItem gridItem = cell.GetOrAddComponent<UIGridItem>();
            gridItem.Data = datas[i];
            gridItem.createBoxCollider();

            items.Add(gridItem);
        }

        RefreshScrollView( false );
    }

    private void RefreshScrollView( bool bFull )
    {
        if (autoCountGrid)
        {
            grid.minNum = 0;
            grid.maxNum = datas.Count;
        }

        if (resetPos) {
            grid.SortBasedOnScrollMovement();
            ResetPosition();
        }
        else if (!bFull)
        {
            grid.SortBasedOnScrollMovement();
        }
    }

    /// <summary>
    /// 获取某个指定的数据
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public object GetData(int index)
    {
        if (datas != null && datas.Count > 0 && this.datas.Count > index && index >= 0)
        {
            return this.datas[index];
        }

        return null;
    }

    public object GetData(GameObject go)
    {
        int index = GetDataIndex(go);
        if (index >= 0 && index < datas.Count)
        {
            return GetData(index);
        }
        return null;
    }

    public int GetDataIndex(GameObject go)
    {
        UIGridItem gridItem = go.GetComponent<UIGridItem>();
        if(gridItem == null)
        {
            return -1;
        }

        return gridItem.realIndex;
    }

    public void AddData(object obj)
    {
        if(datas == null)
        {
            return;
        }

        datas.Add(obj);
    }

    public void RemoveData(object obj)
    {
        if (datas == null)
        {
            return;
        }

        datas.Remove(obj);
    }
    #endregion

    #region 清理相关
    public void ClearDatas()
    {
        if (datas.Count <= 0)
            return;

        datas.Clear();

        if (grid.transform.childCount <= 0)
            return;

        RecycleCacheItems();
        items.Clear();

        DisableSpring();
        ResetPosition();
    }

    /// <summary>
    /// 回收item到缓存中
    /// </summary>
    private void RecycleCacheItems()
    {
        Transform gridTrans = grid.transform;
        if(gridTrans.childCount <= 0)
        {
            return;
        }

        Queue<GameObject> cacheItems = null;
        if (!cacheItemDic.TryGetValue(itemPrefabPath, out cacheItems))
        {
            return;
        }

        for (int i = gridTrans.childCount - 1; i >= 0; i--)
        {
            Transform itemTrans = gridTrans.GetChild(i);
            if (cacheItems.Contains(itemTrans.gameObject))
            {
                continue;
            }

            MonoExtendUtil.AddChildToTarget(cacheItemRoot.transform, itemTrans);
            NGUITools.SetActive(itemTrans.gameObject, false);

            cacheItems.Enqueue(itemTrans.gameObject);
        }
        grid.ClearChildren();
    }

    #endregion

    #region 处理弹性回滚
    public override bool RestrictWithinBounds(bool instant, bool horizontal, bool vertical)
    {
        Bounds b = bounds;
        Vector3 constraint = mPanel.CalculateConstrainOffset(b.min, b.max);

        int gridCount = grid.transform.childCount;

        if (!horizontal) constraint.x = 0f;
        if (!vertical) constraint.y = 0f;

        if (constraint.sqrMagnitude > 0.1f)
        {
            if (!instant && dragEffect == DragEffect.MomentumAndSpring)
            {
                if ((movement == Movement.Vertical && shouldMoveVertically)
                    || (movement == Movement.Horizontal && shouldMoveHorizontally))
                {
                    // Spring back into place
                    Vector3 pos = mTrans.localPosition + constraint;
                    pos.x = Mathf.Round(pos.x);
                    pos.y = Mathf.Round(pos.y);
                    SpringPanel.Begin(mPanel.gameObject, pos, 13f).strength = 8f;
                }
            }
            else
            {
                // Jump back into place
                MoveRelative(constraint);

                // Clear the momentum in the constrained direction
                if (Mathf.Abs(constraint.x) > 0.01f) mMomentum.x = 0;
                if (Mathf.Abs(constraint.y) > 0.01f) mMomentum.y = 0;
                if (Mathf.Abs(constraint.z) > 0.01f) mMomentum.z = 0;
                mScroll = 0f;
            }
            return true;
        }
        return false;
    }

    public override void DragStart()
    {
        startPos = transform.localPosition;
    }

    public override void DragFinished()
    {
        if (movement == Movement.Vertical && !shouldMoveVertically)
        {
            if (dragEffect == UIScrollView.DragEffect.MomentumAndSpring)
            {
                // Spring back into place
                SpringPanel.Begin(mPanel.gameObject, startPos, 13f).strength = 8f;
            }
            return;
        }

        if (movement == Movement.Horizontal && !shouldMoveHorizontally)
        {
            if (dragEffect == UIScrollView.DragEffect.MomentumAndSpring)
            {
                // Spring back into place
                SpringPanel.Begin(mPanel.gameObject, startPos, 13f).strength = 8f;
            }

            return;
        }
    }

    public override bool shouldMoveVertically
    {
        get
        {
            bool canMove = true;
            if(grid.transform.childCount <= maxGridNum - grid.maxPerLine)
            {
                canMove = false;
            }

            float size = bounds.size.y;
            if (mPanel.clipping == UIDrawCall.Clipping.SoftClip) size += mPanel.clipSoftness.y * 2f;
            return Mathf.RoundToInt(size - mPanel.height) > 0 || canMove;
        }
    }

    #endregion

    #region 回调相关
	void OnUpdateGrid(UIGridItem item, int index)
    {
        if (index == datas.Count - 1)
        {
            DragLastIndex = true;
        }
        else
        {
            DragLastIndex = false;
        }

        if(onContentUpdate != null)
        {
            object data = GetData(index);
            if(data == null)
            {
                return;
            }

			item.Data = data;

			onContentUpdate(gameObject, item.gameObject, data);
        }
    }
    #endregion
}
