using System;
using System.IO;
using LuaInterface;

public class ByteBuffer
{
    private MemoryStream stream = null;
    private BinaryWriter byteWrite = null;
    private BinaryReader byteReader = null;

    private int offsetIndex = 0;
    private byte[] bData;

	#region operate bytes direct
	public void SetBytes(byte[] b)
    {
        offsetIndex = 0;
        bData = b;
    }

    public int GetInt()
    {
        int i = this.GetInt(bData, offsetIndex);
        offsetIndex += 4;
        return i;
    }
    #endregion

    private int GetInt(byte[] b, int offset)
	{
		return (int)b[offset + 0] & 0xff
			   | ((int)b[offset + 1] & 0xff) << 8
			   | ((int)b[offset + 2] & 0xff) << 16
			   | ((int)b[offset + 3] & 0xff) << 24;
	}

    #region stream function
    public ByteBuffer()
    {
        stream = new MemoryStream();
        byteWrite = new BinaryWriter(stream);
    }

    public ByteBuffer(byte[] datas)
    {
        if (datas == null)
        {
            stream = new MemoryStream();
            byteWrite = new BinaryWriter(stream);
            return;
        }

        stream = new MemoryStream(datas);
        byteReader = new BinaryReader(stream);
    }

    public void Close()
    {
        try
        {
            if (byteWrite != null)
            {
                byteWrite.Close();
                byteWrite = null;
            }

            if (byteReader != null)
            {
                byteReader.Close();
                byteReader = null;
            }

            stream.Close();
            stream = null;
            byteWrite = null;
            byteReader = null;
        }
        catch (Exception e)
        {
            Debugger.LogError(e.ToString());
        }
    }

    public void WriteInt(int v)
    {
        byteWrite.Write(v);
    }

    public void WriteBytes(byte[] datas)
    {
        byteWrite.Write(datas);
    }

    public byte[] ToBytes()
    {
        byteWrite.Flush();
        return stream.ToArray();
    }

    public int ReadInt()
    {
        return byteReader.ReadInt32();
    }

    public byte[] ReadBytes(int len)
    {
        return byteReader.ReadBytes(len);
    }
    #endregion
}
