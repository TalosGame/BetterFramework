using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 声音基本数据
/// </summary>
[Serializable]
public class SoundData
{
    public SoundData()
    {
        
    }

	[SerializeField]
	private string _name;
	public string NAME { get { return _name; } set { _name = value; } }

	[SerializeField]
	private int _type;
	public int Type { get { return _type; } set { _type = value; } }

	[SerializeField]
	private bool _loop;
	public bool Loop { get { return _loop; } set { _loop = value; } }

	[SerializeField]
	private int _cache;
	public int Cache { get { return _cache; } set { _cache = value; } }

	[SerializeField]
	private float _delayTime;
	public float DelayTime { get { return _delayTime; } set { _delayTime = value; } }

	[SerializeField]
	private bool _firstdelay;
	public bool FirstDelay { get { return _firstdelay; } set { _firstdelay = value; } }
}

public class SoundDataTable
{
	public static Dictionary<string, SoundData> datas = new Dictionary<string, SoundData>();

	public static string CSVText
	{
		get
		{
            var csvFile = GameDataLoader<TextAsset>.LoadCSV("SoundDatas");
			if (csvFile != null)
			{
				return csvFile.text;
			}
			return null;
		}
	}

	public static void Load(string text)
	{
		List<SoundData> temps = new List<SoundData>();
		temps = GameDataLoader<SoundData>.DeserializeFromText(text);
		Init(temps);
	}

	public static void Init(List<SoundData> temps)
	{
		SoundData temp = null;
		for (int i = 0; i < temps.Count; i++)
		{
			temp = temps[i];
			if (datas.ContainsKey(temp.NAME))
				continue;

			datas.Add(temp.NAME, temp);
		}
	}

	static public SoundData Get(string name)
	{
		if (datas.Count <= 0)
		{
			Load(CSVText);
		}

		if (datas.ContainsKey(name))
		{
			return datas[name];
		}

		return null;
	}
}