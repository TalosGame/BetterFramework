using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoadResources : MonoBehaviour 
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
        GameObject carObj = MLResourceManager.Instance.LoadInstance("Fruitcar", GameResourceType.RES_LEVEL_OBJECTS) as GameObject;
        carObj.transform.localScale = new Vector3(2f, 2f, 1f);
	}
}
