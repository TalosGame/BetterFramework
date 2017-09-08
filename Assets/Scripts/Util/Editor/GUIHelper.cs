using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class GUIHelper 
{
    public static readonly GUIStyle splitter;

    private static readonly Color splitterColor = EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f);

    /// <summary>
    /// 编辑资源路径
    /// </summary>
    private const string EDITOR_RES_PATH = "Assets/Art/Editor/";

    static GUIHelper()
    {
        //GUISkin skin = GUI.skin;

        splitter = new GUIStyle();
        splitter.normal.background = EditorGUIUtility.whiteTexture;
        splitter.stretchWidth = true;
        splitter.margin = new RectOffset(0, 0, 7, 7);
    }

    // GUILayout Style
    public static void Splitter(Color rgb, float space = 1, float thickness = 1)
    {
        GUILayout.Space(space);

        Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitter, GUILayout.Height(thickness));

        if (Event.current.type == EventType.Repaint)
        {
            Color restoreColor = GUI.color;
            GUI.color = rgb;
            splitter.Draw(position, false, false, false, false);
            GUI.color = restoreColor;
        }

        GUILayout.Space(space);
    }

    public static void Splitter(float thickness, GUIStyle splitterStyle, float space = 1)
    {
        GUILayout.Space(space);

        Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitterStyle, GUILayout.Height(thickness));

        if (Event.current.type == EventType.Repaint)
        {
            Color restoreColor = GUI.color;
            GUI.color = splitterColor;
            splitterStyle.Draw(position, false, false, false, false);
            GUI.color = restoreColor;
        }

        GUILayout.Space(space);
    }

    public static void Splitter(float thickness = 1)
    {
        Splitter(thickness, splitter);
    }

    // GUI Style
    public static void Splitter(Rect position, float space = 1)
    {
        GUILayout.Space(space);

        if (Event.current.type == EventType.Repaint)
        {
            Color restoreColor = GUI.color;
            GUI.color = splitterColor;
            splitter.Draw(position, false, false, false, false);
            GUI.color = restoreColor;
        }

        GUILayout.Space(space);
    }

    /**
     * 绘制分割线
     * @param space         分割线空间
     * @param lineHeight    分割线高度
     **/
    //     public static void splitterLine(float space, float lineHeight)
    //     {
    //         GUILayout.Space(space);
    //         GUILayout.Box(GUIContent.none, MLGUIUtil.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(lineHeight));
    //         GUILayout.Space(space);
    //     }

	public static bool ShowEditorDialog(string message){
		return ShowEditorDialog("Editor Error!", message);
    }

	public static bool ShowEditorDialog(string title, string message){
		return EditorUtility.DisplayDialog(title, message, "Ok");
	}

//    public static void DrawVerticalBezier(Vector3 start, Vector3 end, bool drawArrow, Color color)
//    {
//        float tangentOff = (end.y - start.y) / 2;
//        if (end.y < start.y)
//        {
//            if (drawArrow)
//                end.y += 8;
//        }
//        else
//        {
//            if (drawArrow)
//                end.y -= 8;
//        }
//
//        Handles.DrawBezier(start,
//                           end,
//                           start + Vector3.up * tangentOff,
//                           end - Vector3.up * tangentOff,
//                           color,
//                           null,
//                           3);
//
//        if (drawArrow)
//        {
//            if (end.y > start.y)
//                DrawTextureAt(LoadEditorRes(EditorString.EDITOR_TEX_ARROW_DOWN),
//                                            new Rect(end.x - 10, end.y - 8, 20, 20), color);
//            else
//                DrawTextureAt(LoadEditorRes(EditorString.EDITOR_TEX_ARROW_UP),
//                                            new Rect(end.x - 10, end.y - 12, 20, 20), color);
//        }
//    }

    public static void DrawTextureAt(Texture tex, Rect dst, Color c)
    {
        Color gc = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(dst, tex, ScaleMode.ScaleToFit, true);
        GUI.color = gc;
    }

    public static Texture2D LoadEditorRes(string name)
    {
        string path = EDITOR_RES_PATH + name;
        Texture2D res = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;

        return res;
    }

	public static void DrawLabel(string str, Color color, float width = 0){
		GUI.color = color;
		GUILayout.Label (str, GUILayout.Width(width));
		GUI.color = Color.white;
	}

	public static void DrawOutline(Rect rect, Color color){
		Handles.BeginGUI();
		Handles.color = color;
		Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin));
		Handles.DrawLine(new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax));
		Handles.DrawLine(new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMin, rect.yMax));
		Handles.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMin, rect.yMin));
		Handles.EndGUI();
	}

	public static void FillCircle(Vector2 center, float radius, Color color){
		Handles.BeginGUI();
		Handles.color = color;
		Handles.DrawSolidDisc (center, Vector3.back, radius);
		Handles.EndGUI();
	}

	public static void DrawCircle(Vector2 center, float radius, Color color){
		Handles.BeginGUI();
		Handles.color = color;
		Handles.DrawWireDisc(center, Vector3.back, radius);
		Handles.EndGUI();
	}

	public static void DrawLine(Vector2 stPos, Vector2 endPos, Color color){
		Handles.color = color;
		Handles.DrawLine(stPos, endPos);
		Handles.color = Color.white;
	}

	public static Texture2D CreateTexWithColor( int width, int height, Color col){
		Color[] pix = new Color[width * height];
		for( int i = 0; i < pix.Length; ++i ){
			pix[ i ] = col;
		}

		Texture2D result = new Texture2D( width, height );
		result.SetPixels( pix );
		result.Apply();
		return result;
	}

	public static T OpenEditorWindow<T>(string name, Vector2 size) where T : EditorWindow{
		T editorWin = EditorWindow.GetWindow<T>(name);
		editorWin.maxSize = size;
		editorWin.minSize = size;
		editorWin.Focus();
		editorWin.Show();

		return editorWin;
	}

	public static string GetSelectFolderPath(){
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (string.IsNullOrEmpty(path)){
			return "Assets";
		}

		if (Path.GetExtension(path) != ""){
			return path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
		}

		return path;
	}
}