using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class RadiusFormula
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float weight;
    [ProtoMember(3)]
    public float a;
    [ProtoMember(4)]
    public float b;

    public RadiusFormula()
    {}
}


[ProtoContract]
public class CameraFormula
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

    public CameraFormula()
    {}
}


[ProtoContract]
public class WeightFormula
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float weight;
    [ProtoMember(3)]
    public float a;
    [ProtoMember(4)]
    public string unit;

    public WeightFormula()
    {}
}


[ProtoContract]
public class FunctionFormula
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float splitWeight;
    [ProtoMember(3)]
    public int splitCount;
    [ProtoMember(4)]
    public float sporeWeight;

    public FunctionFormula()
    {}
}


[ProtoContract]
public class EffectFormula
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public float effectWeight;

    public EffectFormula()
    {}
}


[ProtoContract]
public class FormulaMeta
{
    [ProtoMember(1)]
    public Dictionary<int, RadiusFormula> RadiusFormulaDic = new Dictionary<int, RadiusFormula>();

    [ProtoMember(2)]
    public Dictionary<int, CameraFormula> CameraFormulaDic = new Dictionary<int, CameraFormula>();

    [ProtoMember(3)]
    public Dictionary<int, WeightFormula> WeightFormulaDic = new Dictionary<int, WeightFormula>();

    [ProtoMember(4)]
    public Dictionary<int, FunctionFormula> FunctionFormulaDic = new Dictionary<int, FunctionFormula>();

    [ProtoMember(5)]
    public Dictionary<int, EffectFormula> EffectFormulaDic = new Dictionary<int, EffectFormula>();

    public IDictionary GetPrototypeDic<T>()
    {
        Type type = typeof(T);
        if (type == typeof(RadiusFormula))
        {
            return RadiusFormulaDic;
        }
        if (type == typeof(CameraFormula))
        {
            return CameraFormulaDic;
        }
        if (type == typeof(WeightFormula))
        {
            return WeightFormulaDic;
        }
        if (type == typeof(FunctionFormula))
        {
            return FunctionFormulaDic;
        }
        if (type == typeof(EffectFormula))
        {
            return EffectFormulaDic;
        }
        return null;
    }

    public T GetMetaValue<T>(int id) where T : class
    {
        IDictionary dic = GetPrototypeDic<T>();
        return dic[id] as T;
    }


    public FormulaMeta()
    {}
}
