

using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TextureCompression : SingletonBase<TextureCompression>
{
    private const string COMPRESS_ERROR_TITLE = "压缩图集错误";
    private const string COMPRESS_ERROR_CHOICE_MATERIAL = "请选择材质球!!!";

    private const string SHADER_BLEND_SHADER = "BlendAlpha";

    private const string STR_COMPRESS_ATLAS_TITLE = "压缩图集";
    private const string STR_COMPRESS_ATLAS_CONTENT = "压缩图集处理中...";

    private int progress = 0;
    private int totalNum = 0;

    #region compress texture ETC|PVRTC split texture rgb and alpha
    public void ExportAtlasEtcOrPvrtc(Material[] mats)
    {
        if (mats == null || mats.Length <= 0)
        {
            GUIHelper.ShowEditorDialog(COMPRESS_ERROR_TITLE, COMPRESS_ERROR_CHOICE_MATERIAL);
            return;
        }

        ShowProgressBar(mats.Length);

        foreach (Material mat in mats)
        {
            string shaderName = mat.shader.name;
            if (shaderName == SHADER_BLEND_SHADER)
            {
                UpdateProgressBar();
                continue;
            }

            string matPath = AssetDatabase.GetAssetOrScenePath(mat);
            string exportPath = Path.GetDirectoryName(matPath);
            string atlasName = Path.GetFileNameWithoutExtension(matPath);

            Texture2D mainTex = (Texture2D)mat.mainTexture;
            //Debug.Log("Texture name==" + mainTex.name);

            Texture2D rgbTex, alphaTex;
            SplitAlpha(mainTex, true, out rgbTex, out alphaTex);

            mat.shader = Shader.Find("Unlit/Transparent Masked");
            mat.SetTexture("_MainTex", rgbTex);
            mat.SetTexture("_Mask", alphaTex);

            EditorUtility.SetDirty(mat);

            UpdateProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

	private void SplitAlpha(Texture2D src, bool alphaHalfSize, out Texture2D rgb, out Texture2D alpha)
	{
		if (src == null)
			throw new ArgumentNullException("src");

		string path = AssetDatabase.GetAssetPath(src);
		var importer = (TextureImporter)AssetImporter.GetAtPath(path);
		importer.isReadable = true;
		AssetDatabase.ImportAsset(path);

		alpha = CreateAlphaTexture(src, alphaHalfSize, path);
		rgb = CreateRGBTexture(src, path);
	}

	private Texture2D CreateAlphaTexture(Texture2D src, bool alphaHalfSize, string path = null)
	{
		if (src == null)
			throw new ArgumentNullException("src");

		// create texture
		var srcPixels = src.GetPixels();
		var tarPixels = new Color[srcPixels.Length];
		for (int i = 0; i < srcPixels.Length; i++)
		{
			float r = srcPixels[i].a;
			tarPixels[i] = new Color(r, r, r);
		}

        Texture2D alphaTex = new Texture2D(src.width, src.height, TextureFormat.RGB24, false);
		alphaTex.SetPixels(tarPixels);
		alphaTex.Apply();

		// save
		string srcPath = path;
		if (string.IsNullOrEmpty(srcPath))
			srcPath = AssetDatabase.GetAssetPath(src);

		string saveAssetPath = GetAlphaTexPath(srcPath);
		var bytes = alphaTex.EncodeToPNG();
		MLFileUtil.SaveFile(saveAssetPath, bytes);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		// setting
		int size = alphaHalfSize ? Mathf.Max(src.width / 2, src.height / 2, 32) : Mathf.Max(src.width, src.height, 32);
		ReImportAsset(saveAssetPath, size, androidFormat:TextureImporterFormat.ETC_RGB4, 
			iosFormat:TextureImporterFormat.PVRTC_RGB4);

		return (Texture2D)AssetDatabase.LoadAssetAtPath(saveAssetPath, typeof(Texture2D));
	}

	private Texture2D CreateRGBTexture(Texture2D src, string path = null)
	{
		if (src == null)
			throw new ArgumentNullException("src");

		string srcPath = path;
		if (string.IsNullOrEmpty(srcPath))
			srcPath = AssetDatabase.GetAssetPath(src);

		string saveAssetPath = GetRGBTexPath(srcPath);

		AssetDatabase.DeleteAsset(saveAssetPath);
		AssetDatabase.CopyAsset(srcPath, saveAssetPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		int size = Mathf.Max(src.width, src.height, 32);
		ReImportAsset(saveAssetPath, size, androidFormat:TextureImporterFormat.ETC_RGB4, 
			iosFormat:TextureImporterFormat.PVRTC_RGB4);

		return (Texture2D)AssetDatabase.LoadAssetAtPath(saveAssetPath, typeof(Texture2D));
	}
	#endregion

	#region texture importer
	public void ReImportAsset(string path, int maxSize, bool alphaIsTransparency = false,
								TextureImporterFormat androidFormat = TextureImporterFormat.Automatic,
								TextureImporterFormat iosFormat = TextureImporterFormat.Automatic, 
                              	TextureImporterNPOTScale npotScale = TextureImporterNPOTScale.ToNearest)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

#if UNITY_5_5_OR_NEWER
        importer.textureType = TextureImporterType.Default;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
#else
        importer.textureType = TextureImporterType.Image;
        importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
#endif

        importer.textureShape = TextureImporterShape.Texture2D;
        importer.anisoLevel = 0;
        importer.spriteImportMode = SpriteImportMode.None;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = maxSize;
        importer.isReadable = false;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.alphaIsTransparency = alphaIsTransparency;
        importer.compressionQuality = (int)TextureCompressionQuality.Normal;
        importer.npotScale = npotScale;

#if UNITY_5_5_OR_NEWER
		importer.SetPlatformTextureSettings(CreateTexPlatformSettings("iphone", maxSize, iosFormat));
		importer.SetPlatformTextureSettings(CreateTexPlatformSettings("Android", maxSize, androidFormat));
#else
		importer.SetPlatformTextureSettings("iPhone", maxSize, iosFormat);
		importer.SetPlatformTextureSettings("Android", maxSize, androidFormat);
#endif

        AssetDatabase.ImportAsset(path);
	}

	private TextureImporterPlatformSettings CreateTexPlatformSettings(string platform, int maxSize, TextureImporterFormat format)
	{
		TextureImporterPlatformSettings setting = new TextureImporterPlatformSettings();
		setting.name = platform;
		setting.maxTextureSize = maxSize;
		setting.format = format;

		return setting;
	}
	#endregion

	private string GetRGBTexPath(string path)
	{
		return GetTexPath(path, "_RGB.");
	}

	private string GetAlphaTexPath(string path)
	{
		return GetTexPath(path, "_Alpha.");
	}

	private string GetTexPath(string path, string extension)
	{
		string dir = Path.GetDirectoryName(path);
		string filename = Path.GetFileNameWithoutExtension(path);
		string result = dir + "/" + filename + extension + "png";
		return result;
	}

	private void ShowProgressBar(int totalNum)
	{
		this.progress = 0;
		this.totalNum = totalNum;

		EditorUtility.DisplayCancelableProgressBar(STR_COMPRESS_ATLAS_TITLE,
			STR_COMPRESS_ATLAS_CONTENT, (float)progress / totalNum);
	}

	private void UpdateProgressBar()
	{
		this.progress++;

		EditorUtility.DisplayCancelableProgressBar(STR_COMPRESS_ATLAS_TITLE,
			STR_COMPRESS_ATLAS_CONTENT, (float)progress / totalNum);

		if (progress >= totalNum)
		{
			progress = 0;
			EditorUtility.ClearProgressBar();
		}
	}

	private void HideProgressBar()
	{
		EditorUtility.ClearProgressBar();
	}
}
