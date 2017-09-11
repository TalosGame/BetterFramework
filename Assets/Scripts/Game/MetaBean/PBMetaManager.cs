using System;

public class PBMetaManager : SingletonBase<PBMetaManager>
{
    public  FormulaMetaTable  formulaMetaTable;

    public  AudioMetaTable  audioMetaTable;

    public  SkillMetaTable  skillMetaTable;

    public void LoadMeta<T>()
    {
        Type type = typeof(T);
        if (type == typeof(FormulaMetaTable))
        {
            formulaMetaTable = MetaLoader<FormulaMetaTable>.LoadPBData(type.FullName);
            return;
        }
        if (type == typeof(AudioMetaTable))
        {
            audioMetaTable = MetaLoader<AudioMetaTable>.LoadPBData(type.FullName);
            return;
        }
        if (type == typeof(SkillMetaTable))
        {
            skillMetaTable = MetaLoader<SkillMetaTable>.LoadPBData(type.FullName);
            return;
        }
    }

    public TM GetMeta<T, TM>(int id)
        where T : class
        where TM : class
    {
        Type type = typeof(T);
        if (type == typeof(FormulaMetaTable))
        {
             if (formulaMetaTable == null)
             {
                 LoadMeta<FormulaMetaTable>();
             }
             return formulaMetaTable.GetMetaValue<TM>(id) as TM;
        }
        if (type == typeof(AudioMetaTable))
        {
             if (audioMetaTable == null)
             {
                 LoadMeta<AudioMetaTable>();
             }
             return audioMetaTable.GetMetaValue<TM>(id) as TM;
        }
        if (type == typeof(SkillMetaTable))
        {
             if (skillMetaTable == null)
             {
                 LoadMeta<SkillMetaTable>();
             }
             return skillMetaTable.GetMetaValue<TM>(id) as TM;
        }
        return null;
    }
}

