using System;
using System.Collections;
using UnityEngine;

public class LoadResources : MonoBehaviour 
{
	private const string LOAD_SYNC_TEXT = "同步加载";
	private const string LOAD_ASYNC_TEXT = "异步加载";

    private GUIStyle btnStyle;
    private GameObject roleTexObj;

    void Awake()
    {
        MLResourceManager resMgr = MLResourceManager.Instance;
        resMgr.InitResourceDefine(new GameResDefine());
        resMgr.CreateResourceMgr(new ResourcesManager());
        resMgr.ChangeResourceMgr(ResManagerType.resourceMgr);

		btnStyle = new GUIStyle("button");
		btnStyle.fontSize = 32;
    }

    void OnGUI()
    {
		if (GUI.Button(new Rect(10, 10, 150, 60), LOAD_SYNC_TEXT, btnStyle))
        {
			StartCoroutine(CleanRoleTexCor(() => {
				LoadSyncRoleTex();
			}));

            return;
        }

        if (GUI.Button(new Rect(10, 80, 150, 60), LOAD_ASYNC_TEXT, btnStyle))
        {
            StartCoroutine(CleanRoleTexCor(()=>{
                LoadAsyncRoleTex();    
            }));

            return;
        }
    }

    private void LoadSyncRoleTex()
    {
		roleTexObj = MLResourceManager.Instance.LoadInstance("RoleTex", GameResourceType.RES_ROLE_TEXTURES) as GameObject;
		roleTexObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
    }

    private void LoadAsyncRoleTex()
    {
        MLResourceManager.Instance.LoadAsyncInstance("RoleTex", (obj) => 
        {
            roleTexObj = obj as GameObject;
            roleTexObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        }, GameResourceType.RES_ROLE_TEXTURES);
    }

    private IEnumerator CleanRoleTexCor(Action action)
    {
		if (action == null)
		{
			yield break;
		}

        if (roleTexObj == null)
        {
            action();
            yield break;
        }

        Destroy(roleTexObj);
        roleTexObj = null;
        yield return new WaitForSeconds(1f);

        action();
    }
}
