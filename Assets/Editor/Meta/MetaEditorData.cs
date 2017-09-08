using UnityEngine;
using UnityEditor;

public enum Excel2MetaType
{
    Excel2CSV,
    Excel2PB,
}

public class MetaEditorData : ScriptableObject
{
	// excel 2 meta type
	public Excel2MetaType type = Excel2MetaType.Excel2PB;
    // excel2PB meta path
    public string excel2PBMetaPath = "Art/Excels/Excels2PB";
    // excel2CSV meta path
    public string excel2CSVMetaPath = "Art/Excels/Excels2CSV";
	// meta bin path
	public string metaBinPath = "Art/Resources/Metas";
    // meta bean path
    public string metaBeanPath = "Scripts/Game/MetaBean";
    // need GZIP compress
    public bool isGZIPCompress = false;

	public const string META_EDITOR_ASSET_NAME = "-MetaEditor";
	public const string META_EDITOR_ASSET_FULL_PATH = "Assets/Editor/Meta/" + META_EDITOR_ASSET_NAME + ".asset";

	private static MetaEditorData _instance;
    public static MetaEditorData Instance
    {
		get
		{

			if (_instance == null)
			{
				string[] assetGUIDs = AssetDatabase.FindAssets(META_EDITOR_ASSET_NAME);
				if (assetGUIDs.Length <= 0)
				{
					_instance = ScriptableObject.CreateInstance<MetaEditorData>();

					AssetDatabase.CreateAsset(_instance, META_EDITOR_ASSET_FULL_PATH);
					AssetDatabase.SaveAssets();

					Selection.activeObject = _instance;
				}
				else
				{
					string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
					_instance = AssetDatabase.LoadAssetAtPath<MetaEditorData>(assetPath);
				}
			}

			if (_instance == null)
				Debug.LogError("create meta editor asset error!");

			return _instance;
		}
    }

    public void SaveData()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
