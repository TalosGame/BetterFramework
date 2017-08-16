using System.Collections.Generic;
using Csv.Serialization;

/// <summary>
/// 游戏数据加载器
/// </summary>
public sealed class GameDataLoader<T> where T : class, new()
{
    public static List<T> DeserializeFromText(string txt)
	{
		if (txt == null)
			return null;
		//!- Basic enemy( all enemies except boss ) info
		var cs = new Csv.Serialization.CsvSerializer<T>()
		{
			UseTextQualifier = true,
			UseLineNumbers = false,
			IgnoreReferenceTypesExceptString = false,
		};

		return (List<T>)cs.DeserializeStream(txt);
	}

	public static T LoadCSV(string name)
	{
        return MLResourceManager.Instance.LoadResource(name, ResourceType.RES_DATAS) as T;
	}
}
