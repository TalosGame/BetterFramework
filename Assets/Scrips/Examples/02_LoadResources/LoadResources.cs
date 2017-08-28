using UnityEngine;

public class LoadResources : MonoBehaviour 
{
    void Awake()
    {
        MLResourceManager resMgr = MLResourceManager.Instance;
        resMgr.InitResourceDefine(new GameResDefine());
        resMgr.CreateResourceMgr(new ResourcesManager());
        resMgr.ChangeResourceMgr(ResManagerType.resourceMgr);
    }

    void Start () 
    {
        GameObject carObj = MLResourceManager.Instance.LoadInstance("RoleTex", GameResourceType.RES_ROLE_TEXTURES) as GameObject;
        carObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
	}
}
