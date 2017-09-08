using UnityEngine;
using UnityEditor;

public class MetaEditorWindow : EditorWindow
{
    private static Vector2 WINDOW_SIZE = new Vector2(320, 120);

    private const string EDITOR_WINDOW_NAME = "MetaEditor";

    private const string LAB_EXCEL_TO_META_TYPE = "Excel2Meta type:";
    private const string LAB_EXCEL_PATH = "Excel files path:";
    private const string LAB_META_BEAN_PATH = "Meta class path:";
    private const string LAB_META_BIN_PATH = "Meta bin path:";
    private const string LAB_META_NEED_GZIP_COMPRESS = "Need GZIP compress:";
    private const string LAB_EXPORT_EXCEL_META = "Export excels to meta";

    private MetaEditorData editorData = null;

    [MenuItem("MLTools/MetaUtil/MetaEditorWindow", false, 1)]
    public static void OpenMetaEditorWindow()
    {
        GUIHelper.OpenEditorWindow<MetaEditorWindow>(EDITOR_WINDOW_NAME, WINDOW_SIZE);
    }

    void OnEnable()
    {
        editorData = MetaEditorData.Instance;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        using (new FixedWidthLabel(LAB_EXCEL_TO_META_TYPE))
        {
            editorData.type = (Excel2MetaType)EditorGUILayout.EnumPopup(editorData.type);
        }

        using (new FixedWidthLabel(LAB_EXCEL_PATH))
        {
            switch(editorData.type)
            {
                case Excel2MetaType.Excel2CSV:
                    editorData.excel2CSVMetaPath = EditorGUILayout.TextField(editorData.excel2CSVMetaPath);
                    break;
                case Excel2MetaType.Excel2PB:
                    editorData.excel2PBMetaPath = EditorGUILayout.TextField(editorData.excel2PBMetaPath);
                    break;
            }
        }

		using (new FixedWidthLabel(LAB_META_BEAN_PATH))
		{
            editorData.metaBeanPath = EditorGUILayout.TextField(editorData.metaBeanPath);
		}

		using (new FixedWidthLabel(LAB_META_BIN_PATH))
		{
			editorData.metaBinPath = EditorGUILayout.TextField(editorData.metaBinPath);
		}

        if(editorData.type == Excel2MetaType.Excel2PB)
        {
            using (new FixedWidthLabel(LAB_META_NEED_GZIP_COMPRESS))
            {
                editorData.isGZIPCompress = EditorGUILayout.Toggle(editorData.isGZIPCompress);
            }
        }

        if(GUILayout.Button(LAB_EXPORT_EXCEL_META))
        {
            ExportExcel2Meta();
        }

        EditorGUILayout.EndVertical();
    }

    private void OnDestroy()
    {
        editorData.SaveData();
    }

    private void ExportExcel2Meta()
    {
        switch (editorData.type)
        {
            case Excel2MetaType.Excel2CSV:
                Debug.Log("Excel2CSV!!");
                break;
            case Excel2MetaType.Excel2PB:
                Debug.Log("Excel2PB!!");
                Excel2PBTools.GenerateXlsx2Cls();
                Excel2PBTools.GenerateXlsx2Bin();
                break;
        }
    }
}
