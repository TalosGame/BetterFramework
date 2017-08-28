using UnityEngine;

public class LoadAssetBundle : MonoBehaviour 
{
    void Awake()
    {
		MLResourceManager resMgr = MLResourceManager.Instance;
		resMgr.InitResourceDefine(new GameResDefine());
        resMgr.CreateResourceMgr(new GameABAssetMgr());
        resMgr.ChangeResourceMgr(ResManagerType.assetBundleMgr);
    }

	void Start ()
    {
		GameObject roleTexObj = MLResourceManager.Instance.LoadInstance("RoleTex", GameResourceType.RES_ROLE_TEXTURES) as GameObject;
		roleTexObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
	}
}
