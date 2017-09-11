using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class AudioMeta
{
    [ProtoMember(1)]
    public int id;
    [ProtoMember(2)]
    public string name;
    [ProtoMember(3)]
    public int type;
    [ProtoMember(4)]
    public bool loop;
    [ProtoMember(5)]
    public int cache;
    [ProtoMember(6)]
    public float delayTime;

    public AudioMeta()
    {}
}


[ProtoContract]
public class AudioMetaTable
{
    [ProtoMember(1)]
    public Dictionary<int, AudioMeta> AudioMetaDic = new Dictionary<int, AudioMeta>();

    public T GetMetaValue<T>(int id) where T : class
    {
        return AudioMetaDic[id] as T;
    }


    public AudioMetaTable()
    {}
}
