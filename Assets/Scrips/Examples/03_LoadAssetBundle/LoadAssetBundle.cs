using System;
using System.Collections;
using UnityEngine;

public class LoadAssetBundle : MonoBehaviour 
{
	private const string LOAD_SYNC_TEXT = "同步加载";
	private const string LOAD_ASYNC_TEXT = "异步加载";

	private GUIStyle btnStyle;
	private GameObject roleObj;

    void Awake()
    {
		MLResourceManager resMgr = MLResourceManager.Instance;
		resMgr.InitResourceDefine(new GameResDefine());
        resMgr.CreateResourceMgr(new GameABAssetMgr());
        resMgr.ChangeResourceMgr(ResManagerType.assetBundleMgr);

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
			StartCoroutine(CleanRoleTexCor(() => {
				LoadAsyncRoleTex();
			}));

			return;
		}
	}

	private void LoadSyncRoleTex()
	{
		roleObj = MLResourceManager.Instance.LoadInstance("TerryRole", GameResourceType.RES_ROLE) as GameObject;
		roleObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
	}

	private void LoadAsyncRoleTex()
	{
		MLResourceManager.Instance.LoadAsyncInstance("TerryRole", (obj) =>
		{
			roleObj = obj as GameObject;
			roleObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
		}, GameResourceType.RES_ROLE_TEXTURES);
	}

	private IEnumerator CleanRoleTexCor(Action action)
	{
		if (action == null)
		{
			yield break;
		}

		if (roleObj == null)
		{
			action();
			yield break;
		}

		Destroy(roleObj);
		roleObj = null;
		yield return new WaitForSeconds(1f);

		action();
	}
}
