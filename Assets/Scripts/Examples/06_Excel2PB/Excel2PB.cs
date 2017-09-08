using UnityEngine;

public class Excel2PB : MonoBehaviour 
{
    private void Awake()
    {
		MLResourceManager resMgr = MLResourceManager.Instance;
		resMgr.InitResourceDefine(new GameResDefine());
		resMgr.CreateResourceMgr(new ResourcesManager());
		resMgr.ChangeResourceMgr(ResManagerType.resourceMgr);
    }

    void Start () 
    {
        Skill skillMeta = PBMetaManager.Instance.GetMetaBean<SkillMeta, Skill>(100);

        Debug.Log("act id:" + skillMeta.actId);
        Debug.Log("name:" + skillMeta.name);
        Debug.Log("state hit:" + skillMeta.stateHit);
        Debug.Log("be Attack GainAnger:" + skillMeta.beAttackGainAnger);
    }
}
