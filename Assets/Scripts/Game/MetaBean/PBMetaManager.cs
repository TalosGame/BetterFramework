using System;

public class PBMetaManager : SingletonBase<PBMetaManager>
{
    public  SkillMeta  skillMeta;

    public void LoadMeta<T>()
    {
        Type type = typeof(T);
        if (type == typeof(SkillMeta))
        {
                skillMeta = DataLoader<SkillMeta>.LoadPBData(type.FullName);
                return;
        }
    }

    public TM GetMetaBean<T, TM>(int id)
        where T : class
        where TM : class
    {
        Type type = typeof(T);
        if (type == typeof(SkillMeta))
        {
                if (skillMeta == null)
                {
                        LoadMeta<SkillMeta>();
                }
                return skillMeta.GetMetaValue<TM>(id) as TM;
        }
        return null;
    }
}

