using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class SkillMeta
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public int skillType;
    [ProtoMember(3)]
    public int consumeType;
    [ProtoMember(4)]
    public int consumeValue;
    [ProtoMember(5)]
    public int isChangeOtherTransform;
    [ProtoMember(6)]
    public int cameraActionID;
    [ProtoMember(7)]
    public int QTECameraEditorID;
    [ProtoMember(8)]
    public int skillWay;
    [ProtoMember(9)]
    public string actId;
    [ProtoMember(10)]
    public string name;
    [ProtoMember(11)]
    public int skillState;
    [ProtoMember(12)]
    public int priority;
    [ProtoMember(13)]
    public int duringNumbers;
    [ProtoMember(14)]
    public int stateUnit;
    [ProtoMember(15)]
    public int stateValue;
    [ProtoMember(16)]
    public int stateHit;
    [ProtoMember(17)]
    public float delayValue;
    [ProtoMember(18)]
    public int singleType;
    [ProtoMember(19)]
    public int nearType;
    [ProtoMember(20)]
    public int affectCamp;
    [ProtoMember(21)]
    public int beAttackGainAnger;
    [ProtoMember(22)]
    public int attackGainAnger;
    [ProtoMember(23)]
    public float skillTime;
    [ProtoMember(24)]
    public float skillFactor;
    [ProtoMember(25)]
    public string skillIcon;
    [ProtoMember(26)]
    public string skillExplain;

    public SkillMeta()
    {}
}


[ProtoContract]
public class SkillMetaTable
{
    [ProtoMember(1)]
    public Dictionary<int, SkillMeta> SkillMetaDic = new Dictionary<int, SkillMeta>();

    public T GetMetaValue<T>(int id) where T : class
    {
        return SkillMetaDic[id] as T;
    }


    public SkillMetaTable()
    {}
}
