using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class RadiusFormulaMeta
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float weight;
    [ProtoMember(3)]
    public float a;
    [ProtoMember(4)]
    public float b;

    public RadiusFormulaMeta()
    {}
}


[ProtoContract]
public class CameraFormulaMeta
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float a;
    [ProtoMember(3)]
    public float b;
    [ProtoMember(4)]
    public float size;
    [ProtoMember(5)]
    public float xOffset;
    [ProtoMember(6)]
    public float yOffset;

    public CameraFormulaMeta()
    {}
}


[ProtoContract]
public class WeightFormulaMeta
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float weight;
    [ProtoMember(3)]
    public float a;
    [ProtoMember(4)]
    public string unit;

    public WeightFormulaMeta()
    {}
}


[ProtoContract]
public class FunctionFormulaMeta
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float splitWeight;
    [ProtoMember(3)]
    public int splitCount;
    [ProtoMember(4)]
    public float sporeWeight;

    public FunctionFormulaMeta()
    {}
}


[ProtoContract]
public class EffectFormulaMeta
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float effectWeight;

    public EffectFormulaMeta()
    {}
}


[ProtoContract]
public class FormulaMetaTable
{
    [ProtoMember(1)]
    public Dictionary<int, RadiusFormulaMeta> RadiusFormulaMetaDic = new Dictionary<int, RadiusFormulaMeta>();

    [ProtoMember(2)]
    public Dictionary<int, CameraFormulaMeta> CameraFormulaMetaDic = new Dictionary<int, CameraFormulaMeta>();

    [ProtoMember(3)]
    public Dictionary<int, WeightFormulaMeta> WeightFormulaMetaDic = new Dictionary<int, WeightFormulaMeta>();

    [ProtoMember(4)]
    public Dictionary<int, FunctionFormulaMeta> FunctionFormulaMetaDic = new Dictionary<int, FunctionFormulaMeta>();

    [ProtoMember(5)]
    public Dictionary<int, EffectFormulaMeta> EffectFormulaMetaDic = new Dictionary<int, EffectFormulaMeta>();

    public IDictionary GetPrototypeDic<T>()
    {
        Type type = typeof(T);
        if (type == typeof(RadiusFormulaMeta))
        {
            return RadiusFormulaMetaDic;
        }
        if (type == typeof(CameraFormulaMeta))
        {
            return CameraFormulaMetaDic;
        }
        if (type == typeof(WeightFormulaMeta))
        {
            return WeightFormulaMetaDic;
        }
        if (type == typeof(FunctionFormulaMeta))
        {
            return FunctionFormulaMetaDic;
        }
        if (type == typeof(EffectFormulaMeta))
        {
            return EffectFormulaMetaDic;
        }
        return null;
    }

    public T GetMetaValue<T>(int id) where T : class
    {
        IDictionary dic = GetPrototypeDic<T>();
        return dic[id] as T;
    }


    public FormulaMetaTable()
    {}
}
