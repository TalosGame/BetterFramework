using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using LuaInterface;

/// <summary>
/// 包数据
/// </summary>
public struct PackageData
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public int msgId;

    /// <summary>
    /// 消息数据
    /// </summary>
    public LuaByteBuffer buffer;

    /// <summary>
    /// 包的下标
    /// </summary>
    public int packageIndex;

    public PackageData(int msgId, byte[] datas)
    {
        this.msgId = msgId;
        this.buffer = new LuaByteBuffer(datas);
        this.packageIndex = 0;
    }
}

/// <summary>
/// 请求消息
/// </summary>
public struct RequestMessage
{
    // 消息ID
    public int msgId;

    // socket标识
    public int identiy;

    // 消息数据
    public byte[] datas;

    // 消息时间毫秒数
    public long time;

    public RequestMessage(int msgId, int identiy, byte[] datas)
    {
        this.msgId = msgId;
        this.identiy = identiy;
        this.datas = datas;
        this.time = TimeUtil.CurrentTimeMillis();
    }
}

public class GameSocket : DDOLSingleton<GameSocket>
{
    private const string GNETWORK_MODULE = "GNetwork";
    private const string GNETWORK_FUNC_CONNECT_SUCESS = "ConnectSucess";
    private const string GNETWORK_FUNC_CONNECT_ERROR = "ConnectError";
    private const string GNETWORK_FUNC_RECONNECT_SUCESS = "ReconnectSucess";
    private const string GNETWORK_FUNC_RECONNECT_ERROR = "ReconnectError";
    private const string GNETWORK_FUNC_DISCONNECT = "CloseConnect";
    private const string GNETWORK_FUNC_RECEIVE_MESSAGE = "ReceiveMessage";
    private const string GNETWORK_FUNC_SEND_MESSAGE = "SendMessage";
    private const string GNETWORK_FUNC_WAIT_PROCESS = "WaitProcess";

    // 静默链接次数
    private const int CONNECT_SILENCE_NUM = 2;
    // 链接最大次数
    private const int CONNECT_MAX_NUM = 10;

    // 当前socket标识
    private int socketIdentiy = -1;

    // socket管理
    private Dictionary<int, SocketClient> sockets = new Dictionary<int, SocketClient>();

    // debug消息Index
    private int debugPackageIndex = 0;

    // 消息队列
    private Queue<RequestMessage> requestQueue = new Queue<RequestMessage>();
    private Queue<RequestMessage> retryQueue = new Queue<RequestMessage>();
    private Queue<PackageData> responseQueue = new Queue<PackageData>();

    // 心跳
    private const int MSG_HEART_ID = 3;
    private int heartTime = 60;
    public int HeartTime
    {
        get { return heartTime; }
        set { heartTime = value; }
    }

    private Thread heartMsgThread = null;
    private long heartStartTime = 0;

    // 改变Socket state
    public void ChangeSocketState(int state) 
    {
        this.socketIdentiy = state;
    }

    public void ConnectToServer(int socketState, string ip, int port)
    {
        SocketClient client = null;
        if (!sockets.TryGetValue(socketState, out client)) {
            client = CreateClient(ip, port, socketState);
            sockets.Add(socketState, client);
        }

        client.Connection(ip, port);

        Debugger.Log("==========ConnectToServer ip:" + ip + " port:" + port + "==================");
    }

    /// <summary>
    /// 重新连接
    /// </summary>
    public void ReConnection()
    {
        SocketClient client = GetSocketClient(this.socketIdentiy);
        if (client == null)
            return;

        client.WaitReconnect = false;
        client.ReConnection();
    }

    private SocketClient CreateClient(string ip, int port, int socketIdentiy)
    {
        SocketClient client = new SocketClient(ip, port, socketIdentiy);

        client.ConnectSucess += this.ConnectSucess;
        client.ConnectFailure += this.ConnectFailure;

        client.ReConnectSucess += this.ReConnectSucess;
        client.ReConnectFailure += this.ReConnectFailure;

        client.ConnectException += this.ConnectException;
        client.CloseConnect += this.CloseConnect;

        client.HandleDatasProcess += this.HandleDatasProcess;

        return client;
    }

    public void CloseSocket(int socketIdentiy)
    {
        Debugger.Log("Close socket! socket state===" + socketIdentiy);

        SocketClient client = GetSocketClient(socketIdentiy);
        if (client == null)
            return;

        client.CloseSocket();

        RemoveSocketClient(socketIdentiy);
    }

    protected override void Destroy()
    {
        if (heartMsgThread != null)
        {
            heartMsgThread.Abort();
            heartMsgThread = null;
        }

        // 关闭所有链接
        if (sockets.Count > 0)
        {
            var enumerator = sockets.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SocketClient client = enumerator.Current.Value;
                client.ConnectSucess -= this.ConnectSucess;
                client.ConnectFailure -= this.ConnectFailure;

                client.ReConnectSucess -= this.ReConnectSucess;
                client.ReConnectFailure -= this.ReConnectFailure;

                client.ConnectException -= this.ConnectException;
                client.CloseConnect -= this.CloseConnect;

                client.HandleDatasProcess -= this.HandleDatasProcess;

                client.CloseSocket();
                client = null;
            }
        }
    }

    #region 心跳处理
    public void StartSendHeartMsg()
    {
        if (heartMsgThread != null)
            return;

        heartMsgThread = new Thread(new ThreadStart(OnUpdateSendHeartMsg));
        heartMsgThread.Start();
    }

    private void OnUpdateSendHeartMsg()
    {
        while (true)
        {
            Thread.Sleep(HeartTime);

            // send heart message
            SendHeartMessage();
        }
    }

    private void SendHeartMessage()
    {
        if (sockets.Count <= 0)
            return;

        ByteBuffer buff = new ByteBuffer();
        buff.WriteInt(MSG_HEART_ID);
        buff.WriteInt(0);

        byte[] bytes = buff.ToBytes();
        buff.Close();

        SendMessages(bytes);
    }

    /// <summary>
    /// 给所有服务器发送消息
    /// </summary>
    /// <param name="datas"></param>
    private void SendMessages(byte[] datas)
    {
        var enumerator = sockets.GetEnumerator();
        while (enumerator.MoveNext())
        {
            SocketClient client = enumerator.Current.Value;
            if (!client.IsConnect && client.IsNotReConnect)
            {
                client.ReConnection();
                continue;
            }

            client.SendMessageNoCallBack(datas);
        }
    }
    #endregion

    #region 处理接收消息
    void Update()
    {
        UpdateReciveMessage();
    }

    /// <summary>
    /// 消息处理
    /// </summary>
    public void UpdateReciveMessage()
    {
        lock (this.responseQueue)
        {
            while (this.responseQueue.Count > 0)
            {
                PackageData package = (PackageData)responseQueue.Dequeue();
                if (package.msgId == MSG_HEART_ID)
                {
                    if (heartStartTime == 0)
                        heartStartTime = TimeUtil.CurrentTimeMillis();

                    long endTime = TimeUtil.CurrentTimeMillis();
                    long useTime = endTime - heartStartTime;
                    heartStartTime = endTime;

                    Debugger.Log("Received heart! time==" + useTime);
                    continue;
                }

                //call lua socket
                LuaHelper.CallLuaFunction(GNETWORK_MODULE, GNETWORK_FUNC_RECEIVE_MESSAGE, package.msgId, package.buffer);
            }
        }
    }

    /// <summary>
    /// 处理Socket 接受Socket流数据
    /// </summary>
    /// <param name="msgId">Message identifier.</param>
    /// <param name="datas">Datas.</param>
    public void HandleDatasProcess(int msgId, byte[] datas)
    {
        lock (this.responseQueue)
        {
            PackageData package = new PackageData(msgId, datas);

            debugPackageIndex = debugPackageIndex + 1;
            package.packageIndex = debugPackageIndex;

            responseQueue.Enqueue(package);
        }
    }

    /// <summary>
    /// 检查发送消息队列发送是否有超时
    /// </summary>
    private void CheckSendMessageTimeOut()
    {
        
    }
    #endregion

    #region 处理发送消息
    public void SendMessage(int msgId, byte[] datas)
    {
//         // 重发队列有
//         if (retryQueue.Count > 0)
//             return;

        SocketClient client = GetSocketClient();
        if (client == null || datas == null)
            return;

//         RequestMessage reqMsg = new RequestMessage(msgId, client.Identiy, datas);
//         requestQueue.Enqueue(reqMsg);

        client.SendMessage(msgId, datas);
    }

    public void SendMessageNoCallBack(byte[] datas) 
    {
        SocketClient client = GetSocketClient();
        if (client == null || datas == null)
            return;

        client.SendMessageNoCallBack(datas);
    }

    public void ReSendMessage()
    {
        while (this.retryQueue.Count > 0)
        {
            
        }
    }

    #endregion

    #region common function
    private SocketClient GetSocketClient()
    {
        return GetSocketClient(this.socketIdentiy);
    }

    private SocketClient GetSocketClient(int socketState)
    {
        SocketClient client = null;
        if (!sockets.TryGetValue(socketState, out client))
        {
            return null;
        }

        return client;
    }

    private void RemoveSocketClient()
    {
        RemoveSocketClient(this.socketIdentiy);
    }

    private void RemoveSocketClient(int socketState)
    {
        sockets.Remove(socketState);
    }
    #endregion

    #region event handle
    private void ConnectSucess(int identiy)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SocketClient client = GetSocketClient(identiy);
            if (client == null)
                return;

            Debugger.Log("==========ConnectToServer sucess ip:" + client.ip + " port:" + client.port + "==================");

            // 链接成功标识socket状态
            this.socketIdentiy = identiy;

            LuaHelper.CallLuaFunction(GNETWORK_MODULE, GNETWORK_FUNC_CONNECT_SUCESS);
        });
    }

    private void ConnectFailure(int identiy)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            LuaHelper.CallLuaFunction(GNETWORK_MODULE, GNETWORK_FUNC_CONNECT_ERROR);
        });
    }

    private void ReConnectSucess(int identiy)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SocketClient client = GetSocketClient(identiy);
            if (client == null)
                return;

            client.ResetConnectTime(false);

            LuaHelper.CallLuaFunction(GNETWORK_MODULE, GNETWORK_FUNC_RECONNECT_SUCESS, identiy);
        });
    }

    private void ReConnectFailure(int identiy)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SocketClient client = GetSocketClient(identiy);
            if (client.ConnectTime >= CONNECT_MAX_NUM)
            {
                client.ResetConnectTime(true);

                LuaHelper.CallLuaFunction(GNETWORK_MODULE, GNETWORK_FUNC_RECONNECT_ERROR, identiy);
                return;
            }

            // 尝试重连判断是否改连接状态下
            if (client.ConnectTime == CONNECT_SILENCE_NUM)
            {
                if(this.socketIdentiy == identiy)
                    LuaHelper.CallLuaFunction(GNETWORK_MODULE, GNETWORK_FUNC_WAIT_PROCESS, true);
            }

            // 尝试重新链接
            client.ReConnection();
        });
    }

    private void ConnectException(int identiy)
    {
        // 连接异常交给lua处理
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            SocketClient client = GetSocketClient(identiy);
            if (client == null || client.WaitReconnect)
                return;

            // 执行静默重连逻辑
            client.ReConnection();
        });
    }

    private void CloseConnect(int identiy)
    {
        // 关闭连接回调
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            LuaHelper.CallLuaFunction(GNETWORK_MODULE, GNETWORK_FUNC_DISCONNECT, identiy);
        });
    }
    #endregion
}