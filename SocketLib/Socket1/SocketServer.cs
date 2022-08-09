using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketLib.Socket1
{
    public class SocketServer
    {
        private string _ip;
        private int _port;
        private Socket _socket;
        private bool _isListen = true;
        private ReaderWriterLockSlim RWLock_ClientList;
        private List<SocketConnection> _clientList;
        public Action<SocketServer, SocketConnection> HandleNewClientConnected;
        public Action<Exception> HandleException;
        public Action<byte[], SocketConnection, SocketServer> HandleRecMsg;
        public Action<byte[], SocketConnection, SocketServer> HandleSendMsg;
        public Action<SocketConnection, SocketServer> HandleClientClose;
        public SocketServer(string ip, int port)
        {
            this._ip = ip;
            this._port = port;
            RWLock_ClientList = new ReaderWriterLockSlim();
            _clientList = new List<SocketConnection>();
        }
        public bool StartServer(int backlog = 10)
        {
                //实例化套接字（ip4寻址协议，流式传输，TCP协议）
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //创建ip对象
                IPAddress address = IPAddress.Parse(_ip);
                //创建网络节点对象包含ip和port
                IPEndPoint endpoint = new IPEndPoint(address, _port);
                //将 监听套接字绑定到 对应的IP和端口
                _socket.Bind(endpoint);
                //设置监听队列长度为Int32最大值(同时能够处理连接请求数量)
                _socket.Listen(backlog);
                //开始监听客户端
                StartListen();
                return true;
        }
        private void StartListen()
        {
            _socket.BeginAccept(AcceptCallBack, null);
        }
        private void AcceptCallBack(IAsyncResult asyncResult)
        {
            Socket newSocket = _socket.EndAccept(asyncResult);
            if (_isListen)
            {
                StartListen();
            }
            SocketConnection socketConnection = new SocketConnection(newSocket, this)
            {
                HandleRecMsg = HandleRecMsg == null ? null : new Action<byte[], SocketConnection, SocketServer>(HandleRecMsg),
                HandleClientClose = HandleClientClose == null ? null : new Action<SocketConnection, SocketServer>(HandleClientClose),
                HandleSendMsg = HandleSendMsg == null ? null : new Action<byte[], SocketConnection, SocketServer>(HandleSendMsg),
                HandleException = HandleException == null ? null : new Action<Exception>(HandleException)
            };
            socketConnection.StartRecMsg();
            AddConnection(socketConnection);
            HandleNewClientConnected?.Invoke(this, socketConnection);
        }
        public void AddConnection(SocketConnection theConnection)
        {
            RWLock_ClientList.EnterWriteLock();
            try
            {
                _clientList.Add(theConnection);
            }
            finally
            {
                RWLock_ClientList.ExitWriteLock();
            }
        }
        public void RemoveConnection(SocketConnection theConnection)
        {
            RWLock_ClientList.EnterWriteLock();
            try
            {
                _clientList.Remove(theConnection);
            }
            finally
            {
                RWLock_ClientList.ExitWriteLock();
            }
        }
        public int GetConnectionCount()
        {
            RWLock_ClientList.EnterReadLock();
            try
            {
                return _clientList.Count;
            }
            finally
            {
                RWLock_ClientList.ExitReadLock();
            }
        }
    }
}
