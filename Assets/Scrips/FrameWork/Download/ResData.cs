using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

[Serializable]
public class ResData
{
	// AB名称
	[XmlAttribute("BundleName")]
	public string bundleName;
	// 资源名称
	[XmlAttribute("Name")]
	public string name;
	// 资源路径
	[XmlAttribute("Path")]
	public string path;
	// MD5码
	[XmlAttribute("MD5")]
	public string md5;
	// 依赖资源名称
	[XmlArrayAttribute("Dependencies")]
	public List<string> dependencies;

	public ResData()
	{
		dependencies = new List<string>();
	}
}
