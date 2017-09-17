using UnityEngine;
using UnityEditor;

public class MLEditorMenu
{
    private const string EDITOR_WINDOW_NAME = "MetaEditor";
    private static Vector2 WINDOW_SIZE = new Vector2(320, 120);


	[MenuItem("MLTools/MetaUtil/MetaEditorWindow", false, 1)]
	public static void OpenMetaEditorWindow()
	{
		GUIHelper.OpenEditorWindow<MetaEditorWindow>(EDITOR_WINDOW_NAME, WINDOW_SIZE);
	}

	[MenuItem("MLTools/Compress/ETC1OrPvrtcTex")]
	public static void CompressETCOrPVRTCTex()
	{
		Debug.Log("CompressETCOrPVRTCTex!!!");
		Material[] selectMats = Selection.GetFiltered<Material>(SelectionMode.DeepAssets);
        TextureCompression.Instance.ExportAtlasEtcOrPvrtc(selectMats);
	}
}
