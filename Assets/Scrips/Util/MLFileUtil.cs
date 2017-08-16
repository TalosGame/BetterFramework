using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using LuaInterface;

public enum ENCODE_TYPE
{
	UTF8,
	DEFAULT,
	UTF8BOM
}

public class MLFileUtil 
{
    public static List<FileInfo> SearchFiles(string path, string searchPattern)
    {
        DirectoryInfo direcInfo = new DirectoryInfo(path);
        return DirSearch(direcInfo, searchPattern);
    }

    public static List<FileInfo> SearchFiles(string path, string[] exceptPaths, string searchPattern)
    {
        DirectoryInfo direcInfo = new DirectoryInfo(path);
        return DirSearch(direcInfo, exceptPaths, searchPattern);
    }

    public static List<FileInfo> SearchFiles(string path, string[] exceptPaths, string searchPattern, string[] exceptFiles)
    {
        DirectoryInfo direcInfo = new DirectoryInfo(path);
        return DirSearch(direcInfo, exceptPaths, searchPattern, exceptFiles);
    }

    private static List<FileInfo> DirSearch(DirectoryInfo d, string[] exceptPaths, string searchFor, string[] exceptFiles = null)
    {
        bool isContain = false;
        for (int i = 0; i < exceptPaths.Length; i++)
        {
            if (d.FullName.Contains(exceptPaths[i]))
            {
                isContain = true;
                break;
            }
        }

        if (isContain)
        {
            return null;
        }

        List<FileInfo> founditems = d.GetFiles(searchFor).ToList();

        // Add (by recursing) subdirectory items.
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            List<FileInfo> ret = DirSearch(di, exceptPaths, searchFor);
            if (ret == null)
                continue;

            founditems.AddRange(ret);
        }

        if (exceptFiles != null)
        {
            for (int j = founditems.Count - 1; j >= 0; j--)
            {
                FileInfo file = founditems[j];
                for (int k = 0; k < exceptFiles.Length; k++)
                {
                    if (file.FullName.EndsWith(exceptFiles[k]))
                    {
                        founditems.Remove(file);
                        break;
                    }
                }
            }
        }

        return (founditems);
    }


    private static List<FileInfo> DirSearch(DirectoryInfo d, string searchFor)
    {
        List<FileInfo> founditems = d.GetFiles(searchFor).ToList();

        // Add (by recursing) subdirectory items.
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis)
            founditems.AddRange(DirSearch(di, searchFor));

        return (founditems);
    }

    public static List<FileInfo> SearchFiles(string path, string[] exceptFiles)
    {
        List<FileInfo> files = SearchFiles(path, "*.*");

        List<FileInfo> ret = new List<FileInfo>();
        foreach (FileInfo file in files)
        {
            bool isFind = true;
            for (int i = 0; i < exceptFiles.Length; i++)
            {
                if (file.FullName.EndsWith(exceptFiles[i]))
                {
                    isFind = false;
                    break;
                }
            }

            if (!isFind)
            {
                continue;
            }

            ret.Add(file);
        }

        return ret;
    }

    public static List<string> SearchSimilarityFiles(string path, string similarityName)
    {
        DirectoryInfo direcInfo = new DirectoryInfo(path);

        List<string> ret = new List<string>();
        FileInfo []files = direcInfo.GetFiles();
        foreach(FileInfo file in files)
        {
            if (!file.Name.Contains(similarityName))
            {
                continue;
            }

            ret.Add(similarityName);
        }

        return ret;
    }

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static bool CheckFileExits(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="datas">文件数据</param>
    public static void SaveFile(string path, string fileName, byte[] datas)
    {
        MakeDirs(path);

        string outPutFilePath = Path.Combine(path, fileName).Replace("\\", "/");
        if (File.Exists(outPutFilePath))
        {
            File.Delete(outPutFilePath);
        }

        FileStream fs = new FileStream(outPutFilePath, FileMode.Create, FileAccess.Write);
        fs.Write(datas, 0, datas.Length);

        fs.Flush();
        fs.Close();
    }

    public static void SaveFile(string filePath, byte[] datas)
    {
        MakeDirs(filePath, true);
        File.WriteAllBytes(filePath, datas);
    }

    /// <summary>
    /// 文件拷贝
    /// </summary>
    /// <param name="srcPath">源目标路径</param>
    /// <param name="destPath">目标路径</param>
    /// <param name="fileName">文件名称</param>
    public static void CopyFile(string srcPath, string destPath, string fileName)
    {
        MakeDirs(destPath);

        string outPutFilePath = Path.Combine(destPath, fileName).Replace("\\", "/");
        Debugger.Log("outPutFilePath===" + outPutFilePath);
        if (File.Exists(outPutFilePath))
        {
            File.Delete(outPutFilePath);
        }

        string inPutFilePath = Path.Combine(srcPath, fileName).Replace("\\", "/");
        Debugger.Log("inPutFilePath===" + inPutFilePath);

        File.Copy(inPutFilePath, outPutFilePath);
    }

    public static void DirectoryCopy(string sourceDirName, string destDirName, string[] exceptFiles)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            bool isFind = false;
            for (int i = 0; i < exceptFiles.Length; i++)
            {
                if (temppath.EndsWith(exceptFiles[i]))
                {
                    isFind = true;
                    break;
                }
            }

            if (isFind)
            {
                continue;
            }

            file.CopyTo(temppath, false);
        }


        DirectoryInfo[] dirs = dir.GetDirectories();
        foreach (DirectoryInfo subdir in dirs)
        {
            string temppath = Path.Combine(destDirName, subdir.Name);
            DirectoryCopy(subdir.FullName, temppath, exceptFiles);
        }
    }


	public static string ReadString(string filePath, ENCODE_TYPE encodeType = ENCODE_TYPE.UTF8)
	{
		if (!File.Exists(filePath))
			return null;

		Encoding encode = null;
		if (encodeType == ENCODE_TYPE.DEFAULT)
		{
			encode = Encoding.Default;
		}
		else
		{
			encode = Encoding.UTF8;
		}

		return File.ReadAllText(filePath, encode);
	}

	public static void WriteString(string filePath, string content, ENCODE_TYPE encodeType = ENCODE_TYPE.UTF8)
	{
		MakeDirs(filePath, true);
		Encoding encode = null;
		if (encodeType == ENCODE_TYPE.DEFAULT)
		{
			encode = Encoding.Default;
		}
		else if (encodeType == ENCODE_TYPE.UTF8BOM)
		{
			encode = new System.Text.UTF8Encoding(true);
		}
		else
		{
			encode = new System.Text.UTF8Encoding(false);
		}

		File.WriteAllText(filePath, content, encode);
	}

	public static void MakeDirs(string path, bool pathIsFile = false)
	{
		if (pathIsFile)
		{
			path = Path.GetDirectoryName(path);
		}

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
}
