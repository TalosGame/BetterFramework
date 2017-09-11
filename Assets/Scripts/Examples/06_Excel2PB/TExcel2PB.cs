using UnityEngine;

public class TExcel2PB : MonoBehaviour 
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
        SkillMeta skillMeta = PBMetaManager.Instance.GetMeta<SkillMetaTable, SkillMeta>(100);
        Debug.Log("act id:" + skillMeta.actId);
        Debug.Log("name:" + skillMeta.name);
        Debug.Log("state hit:" + skillMeta.stateHit);
        Debug.Log("be Attack GainAnger:" + skillMeta.beAttackGainAnger);

        WeightFormulaMeta weightFormulaMeta = PBMetaManager.Instance.GetMeta<FormulaMetaTable, WeightFormulaMeta>(1);
        Debug.Log("weight:" + weightFormulaMeta.weight);
        Debug.Log("a:" + weightFormulaMeta.a);
        Debug.Log("unit:" + weightFormulaMeta.unit);

        FunctionFormulaMeta functionFormulaMeta = PBMetaManager.Instance.GetMeta<FormulaMetaTable, FunctionFormulaMeta>(0);
        Debug.Log("split weight:" + functionFormulaMeta.splitWeight);
        Debug.Log("split count:" + functionFormulaMeta.splitCount);
        Debug.Log("spore weight:" + functionFormulaMeta.sporeWeight);
	}
}
