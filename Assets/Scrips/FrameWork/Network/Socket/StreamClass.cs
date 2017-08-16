using System.Text;
 
public class StreamClass 
{	
	private int offsetIndex = 0;
	private byte[] bData;

	public void SetBytes(byte[] b)
	{
		offsetIndex = 0;
		bData = b;
	}

	public byte readByte()
	{
		byte b = this.getByte(bData, offsetIndex++);
		return b;
	}
	
	public short readShort()
	{
		short s = this.getShort(bData, offsetIndex);
		offsetIndex += 2;
		return s;
	}
	
	public int readInt()
	{
		int i = this.getInt(bData, offsetIndex);
		offsetIndex += 4;
		return i;
	}
	
	public long readLong()
	{
		long l = this.getLong(bData,offsetIndex);
		offsetIndex += 8;
		return l;
	}
	
	public byte[] readBytes(int length)
	{
		byte[] bMessage = new byte[length];
		for(int i = 0;i<length;i++){
			byte tb = this.readByte();
			bMessage[i] = tb;
		}
		
		return bMessage;
	}
	
	public string readUTF8(int length)
	{
		byte[] bMessage = new byte[length];
		for(int i = 0;i<length;i++){
			byte tb = this.readByte();//TypeConvert.getByte(mReceiveData,33+i);
			bMessage[i] = tb;
		}

//		if(StreamClass.isUnZip){
//			byte[] b = CompressionHelper.DeCompress(bMessage);
//			
//			return Encoding.UTF8.GetString(b);
//		}

		return Encoding.UTF8.GetString(bMessage);
	}

	//***********
	private byte getByte(byte[] b,int offset)
	{
		return b[offset];
	}

	private int getInt(byte[] b,int offset)
	{
		return    (int)b[offset+3] & 0xff 
               | ((int)b[offset+2] & 0xff) << 8 
               | ((int)b[offset+1] & 0xff) << 16
               | ((int)b[offset] & 0xff) << 24;

	}

	private long getLong( byte[] array, int offset )
	{
        return ((((long) array[offset + 0] & 0xff) << 56)
              | (((long) array[offset + 1] & 0xff) << 48)
              | (((long) array[offset + 2] & 0xff) << 40)
              | (((long) array[offset + 3] & 0xff) << 32)
              | (((long) array[offset + 4] & 0xff) << 24)
              | (((long) array[offset + 5] & 0xff) << 16)
              | (((long) array[offset + 6] & 0xff) << 8) 
              | (((long) array[offset + 7] & 0xff) << 0));            
    }

	private short getShort(byte[] b, int offset)
	{
        return (short)( b[1 + offset] & 0xff |(b[0 + offset] & 0xff) << 8 );
    }

    /*
	public static string getString(byte[] buf){
        StringBuilder strbuf = new StringBuilder();	
        strbuf.Append("{");
        foreach (byte b in buf) {
            if (b == 0) {
                strbuf.Append("00");
            } 
            else if (b == -1) 
            {
                strbuf.Append("FF");
            } 
            else 
            {
                //string str = Integer.toHexString(b).toUpperCase();
				string str = Convert.ToString(b, 16);//Convert.ToString(b); 
                // sb.append(a);
                if (str.Length == 8) 
                {
                    str = str.Substring(6, 8);
                } 
                else if (str.Length < 2) 
                {
                    str = "0" + str;
                }
                strbuf.Append(str);
            }
            strbuf.Append(" ");
        }
        strbuf.Append("}");
        return strbuf.ToString();
    }*/
	//***********

	#region stream tool function
	public static byte[] getBytes(byte b){
		byte[] buf = new byte[1];  
		buf[0] = b;
		return buf;
	}

	public static byte[] getBytes(int n){
		byte[] b = new byte[4];
		b[3] = (byte) (n & 0xff);
		b[2] = (byte) (n >> 8 & 0xff);
		b[1] = (byte) (n >> 16 & 0xff);
		b[0] = (byte) (n >> 24 & 0xff);
		return b;
	}

	public static byte[] getBytes(long n) {
		byte[] b = new byte[8];
		b[7] = (byte) (n & 0xff);
		b[6] = (byte) (n >> 8  & 0xff);
		b[5] = (byte) (n >> 16 & 0xff);
		b[4] = (byte) (n >> 24 & 0xff);
		b[3] = (byte) (n >> 32 & 0xff);
		b[2] = (byte) (n >> 40 & 0xff);
		b[1] = (byte) (n >> 48 & 0xff);
		b[0] = (byte) (n >> 56 & 0xff);
		return b;
	}

	public static byte[] getBytes(short n) {
		byte[] b = new byte[2];
		b[1] = (byte) ( n       & 0xff);
		b[0] = (byte) ((n >> 8) & 0xff);
		return b;
	}
	#endregion
}
