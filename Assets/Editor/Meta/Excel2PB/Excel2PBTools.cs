//
// Excel2PBTools.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.制作Excel to PB 原型c#文件和数据工具类
//
//      2. 需要生成数值类文件，该类文件是以protobuf为基础。
//      后续根据读取excel数据， 通过反射创建该类文件，可配置
//      进行GZip压缩导出protobuf数据。 反之根据protobuf数
//      据序列化为实例对象，原理一样.
//
// Copyright (c) 2017 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Excel;
using ProtoBuf;

public class Excel2PBTools
{
    private static string EXCEL_FILE_PATH = Application.dataPath + @"/Art/Excels/Excels2PB";
    private static string EXCEL_BEAN_FILE_PATH = Application.dataPath + @"/Scripts/Game/MetaBean";

    private static string EXCEL_BIN_FILE_PATH = Application.dataPath + @"/Art/Resources/Metas";

    // 特殊字段
    private const string FIELD_INTS_ARRAY = "ints";
    private const string FIELD_STRING_ARRAY = "strings";
    // TODO 自己可以在这里扩展更多特殊类型

    #region 根据excel创建c#类文件
    public static void GenerateXlsx2Cls()
    {
        string code = "";
        string propertyManagerCode = "";

        Debug.Log("===开始创建excel Class文件===");

        List<FileInfo> files = getExcelFiles();
        List<string> classDicNames = new List<string>();
        List<string> classNames = new List<string>();
        foreach (FileInfo file in files)
        {
            FileStream fs = file.Open(FileMode.Open, FileAccess.Read);

            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            string[] names = fileName.Split('#');
            string className = GetClassName(names[1]);

            if(className == null)
            {
                Debug.LogError("Get excel class name error! file name===" + fileName);
                continue;
            }

            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
            DataSet result = excelReader.AsDataSet();
            DataTableCollection tables = result.Tables;
            if(tables.Count > 1)
            {
                classNames.Add(className);

                //propertyManagerCode += "    [ProtoMember(" + (priority1++) + ")]\n";
                //propertyManagerCode += "    public " + className + " " + className + ";\n\n";

                CreateMultiTableClassFile(tables, className);

            }else
            {
                //classDicNames.Add(className);
                classNames.Add(className);

                propertyManagerCode += "    public  " + GetClassTableName(className) + "  " + GetClassTableInstName(className) + ";\n\n";

                CreateOneTableClassFile(tables, className);
            }

            excelReader.Close();
            fs.Close();
        }

        Debug.Log("===创建 PBMetaManager Class文件===");
        code = "";
        code += WritePBMetaManagerClass(classNames, propertyManagerCode);
        WriteClassFile(EXCEL_BEAN_FILE_PATH, "PBMetaManager", code);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();

        Debug.Log("===完成创建excel Class文件===");
    }

    /// <summary>
    /// 创建单sheet excel 类文件
    /// </summary>
    /// <param name="tables"></param>
    /// <param name="className"></param>
    private static void CreateOneTableClassFile(DataTableCollection tables, string className)
    {
        string code = "";
        code += "using System.Collections;\n";
        code += "using System.Collections.Generic;\n";
        code += "using ProtoBuf;\n\n";

        DataTable dt = tables[0];
        code += WriteClass(dt, className);

        code += WriteClassTable(className);

        Debug.Log("===创建excel " + className + " 文件===");
        WriteClassFile(EXCEL_BEAN_FILE_PATH, className, code);
    }

    /// <summary>
    /// 创建多sheet excel 类文件
    /// </summary>
    /// <param name="tables"></param>
    /// <param name="className"></param>
    private static void CreateMultiTableClassFile(DataTableCollection tables, string className)
    {
        string code = "";
        // 每个sheet对应一个class
        List<string> classNames = new List<string>();

        code += "using System;\n";
        code += "using System.Collections;\n";
        code += "using System.Collections.Generic;\n";
        code += "using ProtoBuf;\n\n";

        foreach(DataTable dt in tables)
        {
            string clsName = GetClassName(dt.TableName);

            code += WriteClass(dt, clsName);

            classNames.Add(clsName);
        }

        /*
        code += "[ProtoContract]\n";
        code += "public class " + className + "\n";
        code += "{\n";

        int priority1 = 1;
        for (int i = 0; i < classNames.Count; i++)
        {
            string clsName = classNames[i];
            code += "    [ProtoMember(" + (priority1++) + ")]\n";
            code += "    public Dictionary<int, " + clsName + "> " + clsName + "Dic = new Dictionary<int, " + clsName + ">();\n\n";
        }

        code += WriteClassFucntion(null, classNames);

        code += "\n";
        code += "    public " + className + "()\n";
        code += "    {}\n";
        code += "}\n";
        */

        code += WriteClassTable(className, classNames);

        Debug.Log("===创建excel " + className + " 文件===");
        WriteClassFile(EXCEL_BEAN_FILE_PATH, className, code);
    }

    private static string WriteClass(DataTable dt, string className)
    {
        string code = "";

        code += "[ProtoContract]\n";
        code += "public class " + className + "\n";
        code += "{\n";

        int col = dt.Columns.Count;
        int priority2 = 1;
        for (int c = 0; c < col; c++)
        {
            code += "    [ProtoMember(" + (priority2++) + ")]\n";

            string type = dt.Rows[2][c].ToString();
            string field = dt.Rows[1][c].ToString();
            if (type.Equals(FIELD_INTS_ARRAY))
            {

                code += "    public List<int> _" + field + " = new List<int>();\n";
                code += "    public List<int> " + field + "\n";
                code += "    {\n";
                code += "        get { return _" + field + ";}\n";
                code += "    }\n";
            }else if(type.Equals(FIELD_STRING_ARRAY))
            {
                code += "    public List<string> _" + field + " = new List<string>();\n";
                code += "    public List<string> " + field + "\n";
                code += "    {\n";
                code += "        get { return _" + field + ";}\n";
                code += "    }\n";
            }
            // TODO 其他特殊类型
            else
            {
                code += "    public " + type + " " + field + ";\n";
            }
        }

        code += "\n";
        code += "    public " + className + "()\n";
        code += "    {}\n";
        code += "}\n\n\n";

        return code;
    }

    private static string WriteClassTable(string className, List<string> sheetNames)
    {
        string code = "";

		code += "[ProtoContract]\n";
		code += "public class " + className + "Meta\n";
		code += "{\n";

		int priority1 = 1;
		for (int i = 0; i < sheetNames.Count; i++)
		{
			string clsName = sheetNames[i];
			code += "    [ProtoMember(" + (priority1++) + ")]\n";
			code += "    public Dictionary<int, " + clsName + "> " + clsName + "Dic = new Dictionary<int, " + clsName + ">();\n\n";
		}

		code += WriteClassFucntion(null, sheetNames);

		code += "\n";
		code += "    public " + className + "Meta()\n";
		code += "    {}\n";
		code += "}\n";

        return code;
    }

    private static string WriteClassTable(string className)
    {
        string code = "";

		code += "[ProtoContract]\n";
		code += "public class " + className + "Meta\n";
		code += "{\n";

		int priority1 = 1;
		code += "    [ProtoMember(" + (priority1) + ")]\n";
		code += "    public Dictionary<int, " + className + "> " + className + "Dic = new Dictionary<int, " + className + ">();\n\n";

		code += "    public T GetMetaValue<T>(int id) where T : class\n";
		code += "    {\n";
		code += "        return " + className + "Dic[id] as T;\n";
		code += "    }\n\n";

		code += "\n";
		code += "    public " + className + "Meta()\n";
		code += "    {}\n";
		code += "}\n";

        return code;
    }

    /// <summary>
    /// 根据原型数据字典类生成相应的接口方法
    /// </summary>
    /// <param name="classNames">多sheet标签类</param>
    /// <param name="classDicNames">单sheet标签类</param>
    /// <returns></returns>
    private static string WriteClassFucntion(List<string> classNames, List<string> classDicNames)
    {
        if(classDicNames.Count <= 0)
        {
            return "\n";
        }

        string code = "";
        code += "    public IDictionary GetPrototypeDic<T>()\n";
        code += "    {\n";
        code += "        Type type = typeof(T);\n";

        if(classDicNames != null)
        {
            for (int i = 0; i < classDicNames.Count; i++)
            {
                string clsName = classDicNames[i];
                code += "        if (type == typeof(" + clsName + "))\n";
                code += "        {\n";
                code += "            return " + clsName + "Dic;\n";
                code += "        }\n";
            }
        }
        
        if(classNames != null)
        {
            code += "        IDictionary ret = null;\n";

            for (int i = 0; i < classNames.Count; i++)
            {
                string clsName = classNames[i];
                code += "        ret = " + clsName + ".GetPrototypeDic<T>();\n";
                code += "        if (ret != null)\n";
                code += "        {\n";
                code += "            return ret;\n";
                code += "        }\n";
            }
        }

        code += "        return null;\n";
        code += "    }\n\n";

        code += "    public T GetPrototypeValue<T>(int id) where T : class\n";
        code += "    {\n";
        code += "        IDictionary dic = GetPrototypeDic<T>();\n";
        code += "        return dic[id] as T;\n";
        code += "    }\n\n";

        return code;
    }

    private static string WritePBMetaManagerClass(List<string> classNames, string classInsCode)
    {
        string code = "";
		code += "using System;\n\n";
		code += "public class PBMetaManager : SingletonBase<PBMetaManager>\n";
        code += "{\n";

        code += classInsCode;

        code += "    public void LoadMeta<T>()\n";
        code += "    {\n";
        code += "        Type type = typeof(T);\n";
		foreach(string className in classNames)
        {
            string clsTabName = GetClassTableName(className);
            string clsTabInsName = GetClassTableInstName(className);

            code += "        if (type == typeof(" + clsTabName + "))\n";
            code += "        {\n";
            code += "                " + clsTabInsName + " = DataLoader<"+ clsTabName + ">.LoadPBData(type.FullName);\n";
            code += "                return;\n";
            code += "        }\n";
		}
        code += "    }\n\n";

        code += "    public TM GetMetaBean<T, TM>(int id)\n";
        code += "        where T : class\n";
	    code += "        where TM : class\n";
	    code += "    {\n";
		code += "        Type type = typeof(T);\n";
        foreach (string className in classNames)
        {
			string clsTabName = GetClassTableName(className);
			string clsTabInsName = GetClassTableInstName(className);
            code += "        if (type == typeof(" + clsTabName + "))\n";
            code += "        {\n";
            code += "                if (" + clsTabInsName + " == null)\n";
            code += "                {\n";
            code += "                        LoadMeta<" + clsTabName + ">();\n";
            code += "                }\n";
            code += "                return " + clsTabInsName + ".GetMetaValue<TM>(id) as TM;\n";
            code += "        }\n";
        }
        code += "        return null;\n";
        code += "    }\n";
        code += "}\n\n";

        return code;
    }
    #endregion

    #region 读取excel生成protobuf数据
    public static void GenerateXlsx2Bin()
    {
        Debug.Log("===开始读取excel数据===");
        
        //PropertyData proertyData = new PropertyData();
        List<FileInfo> files = getExcelFiles();
        foreach (FileInfo file in files)
        {
            Debug.Log("===读取excel文件===" + file.Name);

            FileStream fs = file.Open(FileMode.Open, FileAccess.Read);

            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            string[] names = fileName.Split('#');
            string className = GetClassName(names[1]);
            string typeName = className + "Meta";

            Type t = GetType(typeName);
            object metaObj = Activator.CreateInstance(t);
            if (metaObj == null)
                continue;

            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
            DataSet result = excelReader.AsDataSet();
            DataTableCollection tables = result.Tables;

            // 判断是否有多个sheet
            if(tables.Count > 1)
            {
                ReadMultiTableClass(tables, metaObj, className);
            }else
            {
                ReadOneTableClass(tables[0], metaObj, className);
            }

            excelReader.Close();
            fs.Close();

			Debug.Log("===开始序列化excel数据===");
			WriteExcelDatas(metaObj);
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        Debug.Log("===序列化excel数据完成导出bin===");
    }

	public static Type GetType(string typeName)
	{
		var type = Type.GetType(typeName);
		if (type != null) return type;
		foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
		{
			type = a.GetType(typeName);
			if (type != null)
				return type;
		}

		return null;
	}

    private static void ReadMultiTableClass(DataTableCollection tables, object classObj, string className)
    {
        // 获取字段类型
        FieldInfo fieldInfo = classObj.GetType().GetField(className);
        Type clsType = fieldInfo.FieldType;

        // 创建
        object fieldClsObj = Activator.CreateInstance(clsType);
        fieldInfo.SetValue(classObj, fieldClsObj);

        foreach (DataTable dt in tables)
        {
            string clsName = GetClassName(dt.TableName);
            ReadOneTableClass(dt, fieldClsObj, clsName);
        }
    }

    private static void ReadOneTableClass(DataTable dt, object classObj, string className)
    {
        int row = dt.Rows.Count;
        int col = dt.Columns.Count;

        // 获取field字段
        FieldInfo field = classObj.GetType().GetField(className + "Dic");
        object fieldVal = field.GetValue(classObj);

        IDictionary clsDic = fieldVal as IDictionary;
        Type[] arguments = clsDic.GetType().GetGenericArguments();
        Type clsType = arguments[1];

        for (int r = 3; r < row; r++)
        {
            object obj = Activator.CreateInstance(clsType);
            object keyObj = null;

            for (int c = 0; c < col; c++)
            {
                string name = dt.Rows[1][c].ToString();
                string value = dt.Rows[r][c].ToString();
                string type = dt.Rows[2][c].ToString();

                try
                {
                    if (FIELD_INTS_ARRAY.Equals(type))
                    {
                        if(value == null || "".Equals(value))
                        {
                            continue;
                        }

                        string[] vals = value.Split('=');

                        PropertyInfo pi = obj.GetType().GetProperty(name);
                        List<int> intArray = (List<int>)pi.GetValue(obj, null);
                        for(int i = 0; i < vals.Length; i++)
                        {
                            intArray.Add(int.Parse(vals[i]));
                        }
                    }else if(FIELD_STRING_ARRAY.Equals(type))
                    {
                        if (value == null || "".Equals(value))
                        {
                            continue;
                        }

                        string[] vals = value.Split('=');

                        PropertyInfo pi = obj.GetType().GetProperty(name);
                        List<string> stringArray = (List<string>)pi.GetValue(obj, null);
                        for (int i = 0; i < vals.Length; i++)
                        {
                            stringArray.Add(vals[i]);
                        }
                    }else
                    {
                        FieldInfo fInf = clsType.GetField(name);
                        object cvalue = System.ComponentModel.TypeDescriptor.GetConverter(fInf.FieldType).ConvertFrom(value);
                        fInf.SetValue(obj, cvalue);

                        if (name.Equals("id"))
                        {
                            keyObj = cvalue;
                        }
                    }

                }catch(Exception e)
                {
                    Debug.Log("name===" + name + " value===" + value + " type===" + type);

                    Debug.LogError(e.ToString());
                    continue;
                }
                
            }

            if (clsDic != null)
            {
                clsDic.Add(keyObj, obj);
            }
        }
    }
    #endregion

    #region 工具方法块
    private static void WriteExcelDatas(object obj)
    {
        byte[] datas = SerializerPBClass(obj);
        string metaName = string.Format("{0}.bytes", obj.GetType().FullName);

        MLFileUtil.SaveFile(EXCEL_BIN_FILE_PATH, metaName, datas);
    }

    private static List<FileInfo> getExcelFiles()
    {
        List<FileInfo> files = MLFileUtil.SearchFiles(EXCEL_FILE_PATH, "*.xlsx");
        return files;
    }

    private static void WriteClassFile(string path, string className, string code)
    {
        string filePath = path + "/" + className + ".cs";
        File.WriteAllText(filePath, code, UnicodeEncoding.UTF8);
    }

    private static byte[] SerializerPBClassUseWithGZip(object cls)
	{
		byte[] datas = SerializerPBClass(cls);
		return CompressUtil.GZipEncode(datas);
	}

    private static byte[] SerializerPBClass(object cls)
	{
		using (MemoryStream ms = new MemoryStream())
		{
			// 通过protobuf序列化
			Serializer.Serialize(ms, cls);
			byte[] datas = ms.ToArray();

			return datas;
		}
	}

	/// <summary>
	/// 通过页签获取类名
	/// </summary>
	/// <param name="sheetName"></param>
	/// <returns></returns>
	private static string GetClassName(string sheetName)
	{
		if (!string.IsNullOrEmpty(sheetName))
		{
			if (sheetName.Length > 1)
			{
				return char.ToUpper(sheetName[0]) + sheetName.Substring(1);
			}

			return char.ToUpper(sheetName[0]).ToString();
		}

		return null;
	}

    private static string GetClassTableName(string name)
    {
        return string.Format("{0}Meta", name);
    }

    private static string GetClassTableInstName(string name)
    {
        string classTabName = GetClassTableName(name);
        return char.ToLower(classTabName[0]).ToString() + classTabName.Substring(1);
    }

    #endregion
}
