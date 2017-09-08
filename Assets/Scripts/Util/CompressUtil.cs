using System.IO;
using ICSharpCode.SharpZipLib.GZip;

public sealed class CompressUtil
{
	#region GZip
	/// <summary>
	/// 通过GZip压缩字节流
	/// </summary>
	/// <returns>The encode.</returns>
	/// <param name="datas">Datas.</param>
	public static byte[] GZipEncode(byte[] datas)
	{
		MemoryStream ms = new MemoryStream();
		GZipOutputStream gzipos = new GZipOutputStream(ms);

		gzipos.Write(datas, 0, datas.Length);
		gzipos.Close();

		return ms.ToArray();
	}

	/// <summary>
	/// 通过GZip解压缩字节流
	/// ps: 解压缩字节流只能支持1m数据
	/// </summary>
	/// <returns>The decode.</returns>
	/// <param name="datas">压缩过的字节流</param>
	public static byte[] GZipDecode(byte[] datas)
	{
		GZipInputStream gzipis = new GZipInputStream(new MemoryStream(datas));
		MemoryStream re = new MemoryStream();

		int count = 0;
		byte[] ret = new byte[4096];
		while ((count = gzipis.Read(ret, 0, ret.Length)) != 0)
		{
			re.Write(ret, 0, count);
		}

		return re.ToArray();
	}
    #endregion
}
