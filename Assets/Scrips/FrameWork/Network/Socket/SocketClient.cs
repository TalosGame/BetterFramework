using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LuaInterface;

/****************************************************************
*   Author：Miller
*   创建时间：4/28/2016 8:58:40 AM
*   
*	QQ: 408310416 
*	Email: wangquan84@126.com
*
*   描述说明：
*   客服端异步socket收发消息类
*   注： 消息头可根据具体的游戏来修改.
*
*   修改历史：
*
*
*****************************************************************/
/// <summary>
/// Socket 相关回调
/// </summary>
public delegate void SocketDelegate(int identiy);
public delegate void HandleDatasProcess(int msgId, byte[] datas);

public class SocketClient
{
    /// <summary>
    /// 最大收包缓存大小
    /// </summary>
    private const int BUFFER_SIZE = 1024 * 10;

    /// <summary>
    /// 协议头字节数
    /// </summary>
    private const int HEAD_SIZE = 8;

    //单次读取数据的缓冲器
    private byte[] mReceiveData;
    // 缓冲区10K
    private byte[] buffer = new byte[BUFFER_SIZE];
    //缓冲区当前大小
    private int bufferSize = 0;
    //当前消息的长度包括头
    private int messageLength = 0;
    //是否是第一次读取数据
    private int firshReceive = 0;

    //ip 地址
    public string ip;
    // 端口号
    public int port;

    // 连接标识
    private int identiy;
    public int Identiy
    {
        get { return identiy; }
        set { identiy = value; }
    }

    // 等待重连
    private bool waitReconnect = false;
    public bool WaitReconnect
    {
        get { return waitReconnect; }
        set { waitReconnect = value; }
    }

    // 连接次数
    private int connectTime;
    public int ConnectTime
    {
        get { return connectTime; }
        set { connectTime = value; }
    }

    // socket
    private Socket socket;
    // 协议头缓存
    private ByteBuffer parseHeadBuffer = new ByteBuffer();

	// 连接成功回调
    public SocketDelegate ConnectSucess;
    // 连接失败回调
    public SocketDelegate ConnectFailure;
    // 重连成功回调
    public SocketDelegate ReConnectSucess;
    // 重连失败回调
    public SocketDelegate ReConnectFailure;
    // 连接异常回调
    public SocketDelegate ConnectException;
	// 关闭连接回调
    public SocketDelegate CloseConnect;

    // 处理socket流数据
    public HandleDatasProcess HandleDatasProcess;

	//日志开关
	private static bool _enableLog = true;

    public SocketClient(string ip, int port, int identiy)
    {
        this.ip = ip;
        this.port = port;
        this.identiy = identiy;

        mReceiveData = new byte[1024];
    }

    private Socket CreateSocket()
    {
        // 检查ip地址是否是ipv6
        IPAddress[] address = Dns.GetHostAddresses(ip);
        if (address[0].AddressFamily == AddressFamily.InterNetworkV6)
        {
            this.socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        }
        else
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 5000);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        socket.NoDelay = true;

        return socket;
    }

    private void DoConnectSucess(object src, SocketDelegate callBack)
    {
        Socket socket = (Socket)src;
        socket.Blocking = true;

        if (callBack != null)
            callBack(identiy);

        socket.BeginReceive(mReceiveData, 0, mReceiveData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
    }

    public bool IsConnect
    {
        get
        {
            if (this.socket == null || !this.socket.Connected)
                return false;

            return true;
        }
    }

    #region socket connect
    /// <summary>
    /// Socket连接
    /// </summary>
    /// <param name="serverAddress">server地址</param>
    /// <param name="port">端口号</param>
    public void Connection(string ip, int port)
    {
        if (this.socket != null && this.socket.Connected)
        {
            return;
        }

        try
        {
            Socket socket = CreateSocket();
            
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();

            eventArgs.RemoteEndPoint = ConvertIpEndPoint();
            eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectCallback);
            eventArgs.UserToken = socket;

            socket.ConnectAsync(eventArgs);
        }
        catch (Exception e)
        {
            DoSocketException();
            DebugLogError(identiy, e.ToString());
        }
    }

    private IPEndPoint ConvertIpEndPoint()
    {
        IPAddress ipAdress = null;
        IPEndPoint ipEndPoint = null;
        bool isValidIP = IPAddress.TryParse(this.ip, out ipAdress);
        if (isValidIP)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.port);
            return ipEndPoint;
        }

        IPHostEntry hostEntry = Dns.GetHostEntry(ip);
        ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], this.port);
        return ipEndPoint;
    }

    private void ConnectCallback(object src, SocketAsyncEventArgs result)
    {
        DebugLog(identiy, "connectCallback, isCompleted:" + result + " " + result.SocketError);
        if (result.SocketError != SocketError.Success)
        {
            this.socket.Close();
            this.socket = null;

            if (ConnectFailure != null)
                ConnectFailure(identiy);

			return;
        }

        DoConnectSucess(src, ConnectSucess);
    }
    #endregion

    #region socket reconnect
    public void ResetConnectTime(bool waitReconnect)
    {
        this.connectTime = 0;
        this.waitReconnect = waitReconnect;
    }

    /// <summary>
    /// 判断是否没有走重连机制
    /// </summary>
    public bool IsNotReConnect
    {
        get {
            DebugLog(identiy, "Connect time==" + this.connectTime);
            return this.connectTime <= 0 && !this.waitReconnect;
        }
    }

    /// <summary>
    /// 重新链接
    /// </summary>
    public void ReConnection()
    {
        this.ConnectTime++;

        try
        {
            Socket socket = CreateSocket();

            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReConnectCallBack);
            eventArgs.UserToken = socket;

            socket.ConnectAsync(eventArgs);
        }
        catch (Exception e)
        {
            DoSocketException();
            DebugLogError(identiy, e.ToString());
        }
    }

    private void ReConnectCallBack(object src, SocketAsyncEventArgs result)
    {
        DebugLog(identiy, "ReConnectCallback, isCompleted:" + result + " " + result.SocketError);
        if (result.SocketError != SocketError.Success)
        {
            this.socket.Close();
            this.socket = null;

            if (ReConnectFailure != null)
                ReConnectFailure(identiy);

            return;
        }

        DoConnectSucess(src, ReConnectSucess);
    }
    #endregion

    #region receive message
    /// <summary>
    /// 保持socket连接, 一直监听server发过来的消息
    /// </summary>
    /// <param name="iar"></param>
    private void ReceiveCallback(IAsyncResult iar)
    {
        try
        {
            if (this.socket == null)
            {
                return;
            }

            Socket s = iar.AsyncState as Socket;
            int read = s.EndReceive(iar);
            if (read < 1)
            {
                DoSocketException();
                return;
            }

            //DebugLog(identiy,"receive msg !!!!!!!!!!!!!!!!!!!!!!!");

            AddBuffer(mReceiveData, read);

            VerifyData();

            //重新开始读取数据
            s.BeginReceive(mReceiveData, 0, mReceiveData.Length, 0, new AsyncCallback(ReceiveCallback), s);
        }
        catch (Exception e)
        {
            DoSocketException();

            DebugLogError(identiy, e.ToString());
        }
    }

    /// <summary>
    /// 添加数据到缓冲区
    /// </summary>
    /// <param name="b"></param>
    /// <param name="size"></param>
    private void AddBuffer(byte[] b, int size)
    {
        Array.Copy(b, 0, buffer, bufferSize, size);
        bufferSize += size;
    }

	/// <summary>
	/// 验证当前协议数据是否完成
	/// </summary>
	private void VerifyData()
	{
		//Debugger.Log("VerifyData function buff size===" + _bufferSize + " message lenghth===" + _messageLength);
		if (bufferSize >= HEAD_SIZE)
		{
			ParseHead(buffer);
			//如果缓冲区的大小大于当前协议的长度，那说明当前这条协议已经接受完成
			if (bufferSize >= messageLength)
			{
				byte[] b = RemoveMessgaeBuffer(messageLength);

				ParseData(b);

				//再次重新验证
				VerifyData();
			}
		}
	}

	private void ParseHead(byte[] buffer)
	{
		if (bufferSize >= HEAD_SIZE && firshReceive == 0)
		{
            parseHeadBuffer.SetBytes(buffer);
            int msgId = parseHeadBuffer.GetInt();
            int protobufLen = parseHeadBuffer.GetInt();
            messageLength = protobufLen + HEAD_SIZE;
			firshReceive = 1;
		}
	}

	/// <summary>
	/// 移除当前协议的数据并返回
	/// </summary>
	/// <param name="length"></param>
	/// <returns>当前协议的数据</returns>
	private byte[] RemoveMessgaeBuffer(int length)
	{
		byte[] b = new byte[length];
		byte[] s = new byte[bufferSize - length];

		for (int i = 0; i < bufferSize; i++)
		{
			if (i < length)
				b[i] = buffer[i];
			else
				s[i - length] = buffer[i];
		}

		ClearBuffer();
		AddBuffer(s, s.Length);
		return b;
	}

	/// <summary>
	/// 清空缓冲区
	/// </summary>
	private void ClearBuffer()
	{
		firshReceive = 0;
		messageLength = 0;
		bufferSize = 0;
	}

    /// <summary>
    /// 解析数据
    /// </summary>
    /// <param name="b"></param>
    private void ParseData(byte[] b)
    {
        if (HandleDatasProcess == null)
            return;

        ByteBuffer readBuffer = new ByteBuffer(b);

        int msgId = readBuffer.ReadInt();
		int len = readBuffer.ReadInt();
        byte[] datas = readBuffer.ReadBytes(len);
        DebugLog(identiy, "received msg id : " + msgId + "  len : " + len);
        
        HandleDatasProcess(msgId, datas);

        readBuffer.Close();
    }
	#endregion

	#region send message
    /// <summary>
    /// 发送消息给server
    /// </summary>
    /// <param name="datas"></param>
    public void SendMessage(int msgId, byte[] datas)
    {
        ByteBuffer writeBuffer = null;
        try
        {
            if (this.socket == null)
            {
                return;
            }

            writeBuffer = new ByteBuffer();

            writeBuffer.WriteInt(msgId);
            writeBuffer.WriteInt(datas.Length);
            writeBuffer.WriteBytes(datas);
            var sendBytes = writeBuffer.ToBytes();
            DebugLog(identiy, "send msg id : " + msgId + "  len: " + datas.Length);
            writeBuffer.Close();

            socket.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, new AsyncCallback(this.SendMessageCallBack), socket);
        }
        catch (Exception e)
        {
            if (writeBuffer != null)
                writeBuffer.Close();

            DoSocketException();

            DebugLogError(identiy, e.ToString());
        }
    }

    public void SendMessageNoCallBack(byte[] datas)
    {
        try
        {
            if (this.socket == null)
            {
                return;
            }

            socket.BeginSend(datas, 0, datas.Length, SocketFlags.None, null, socket);
        }
        catch (Exception e)
        {
            DebugLogError(identiy, e.ToString());
        }
    }

    /// <summary>
    /// 信息发送到server完成回调
    /// </summary>
    /// <param name="iar"></param>
    private void SendMessageCallBack(IAsyncResult iar)
    {
		SocketError socketError = SocketError.Success;
        socket.EndSend(iar, out socketError);

		// lua 层处理发送超时
		if (socketError == SocketError.TimedOut)
		{
            // TODO 发上一条消息
            DebugLogError(identiy, "Send message time out!");
			return;
		}

        if (socketError != SocketError.Success)
        {
            DebugLogError(identiy, "Send message error!");

            DoSocketException();
            return;
        }

        DebugLog(identiy, "Send message successful!!!");
    }
    #endregion

    #region 处理socket关闭和异常
    /// <summary>
    /// 处理socket异常
    /// </summary>
    private void DoSocketException()
    {
        DebugLog(identiy, "DoSocketException!!!!!!!!!!!");

        ReleaseSocket(ConnectException);
    }

    /// <summary>
    /// 关闭socket连接
    /// <para>注意：在不需要使用时关闭socket连接</para>
    /// </summary>
    public void CloseSocket()
    {
        ReleaseSocket(CloseConnect);
    }

    private void ReleaseSocket(SocketDelegate callBack)
    {
        if (this.socket != null)
        {
            try
            {
                //this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();
                this.socket = null;
            }
            catch (Exception e)
            {
                this.socket = null;
                DebugLogError(identiy, e.ToString());
            }
            finally
            {
                if (callBack != null)
                    callBack(this.identiy);
            }
        }
    }
    #endregion

    #region debug
    //日志开关
    public static void EnableLogging(bool enableLog)
    {
        _enableLog = enableLog;
    }

    public static void DebugLog(int identiy, string msg)
    {
        if (!_enableLog)
            return;

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            Debug.Log("socket identiy:" + identiy + " msg:" + msg);
        });
    }

    public static void DebugLogError(int identiy, string msg)
    {
        if (!_enableLog)
            return;

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            Debug.LogError("socket identiy:" + identiy + " msg:" + msg);
        });
    }

    public static void DebugLogWarn(int identiy, string msg)
    {
        if (!_enableLog)
            return;

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            Debug.LogWarning("socket identiy:" + identiy + " msg:" + msg);
        });
    }
    #endregion
}

