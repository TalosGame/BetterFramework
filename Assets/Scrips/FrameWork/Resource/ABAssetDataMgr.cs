

public abstract class ABAssetDataBase
{
    public abstract ResData FindResData(string name, int resType = -1);
}

public class ABAssetDataMgr : SingletonBase<ABAssetDataMgr>
{
    private ABAssetDataBase abAssetData;
    public ABAssetDataBase AbAssetData
    {
        get
        {
            return abAssetData;
        }
    }

    public void SetUpABAssetData(ABAssetDataBase assetData)
    {
        this.abAssetData = assetData;
    }

    public ResData FindResData(string name, int resType = -1)
    {
        if(abAssetData == null)
        {
            return null;
        }

        return abAssetData.FindResData(name, resType);
    }
}
