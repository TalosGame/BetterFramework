
public class GameABAssetMgr : AssetBundleManager
{
	protected override ResourceInfo CreateResourceInfo(string name, int type)
	{
		ResourceInfo ret = base.CreateResourceInfo(name, type);

		// TODO 查询依赖资源

		return ret;
	}

    /// <summary>
    /// 获取依赖
    /// </summary>
    /// <returns>The depend resource info.</returns>
    /// <param name="name">Name.</param>
    protected override ResourceInfo GetDependResourceInfo(string name)
    {
        return null;
    }
}
