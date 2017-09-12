using UnityEngine;

public class GameEntry : MonoBehaviour 
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
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_MAIN_MENU);
	}
}
