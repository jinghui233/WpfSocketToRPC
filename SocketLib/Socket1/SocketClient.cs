using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketLib.Socket1
{
    public class SocketClient
    {
        private Socket _socket = null;
        private string _ip = "";
        private int _port = 0;
        private bool _isRec = true;
        private byte[] buffer = new byte[1024 * 1024 * 4];
        public Action<byte[], SocketClient> HandleRecMsg;
        public Action<byte[], SocketClient> HandleSendMsg;
        public Action<SocketClient> HandleClientClose;
        public Action<Exception> HandleException;
        public SocketClient(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }
        private bool IsSocketConnected()
        {
            bool part1 = _socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (_socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public bool StartClient()
        {
            //实例化 套接字 （ip4寻址协议，流式传输，TCP协议）
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //创建 ip对象
            IPAddress address = IPAddress.Parse(_ip);
            //创建网络节点对象 包含 ip和port
            IPEndPoint endpoint = new IPEndPoint(address, _port);
            //将 监听套接字  绑定到 对应的IP和端口
            _socket.Connect(endpoint);
            //开始接受服务器消息
            StartRecMsg();
            return true;
        }
        public bool StartRecMsg()
        {
            _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecCallBack, null);
            return true;
        }
        private void RecCallBack(IAsyncResult asyncResult)
        {
            int length = _socket.EndReceive(asyncResult);
            byte[] recBytes = new byte[length];
            Array.Copy(buffer, 0, recBytes, 0, length);
            if (length > 0 && _isRec && IsSocketConnected())
            {
                StartRecMsg();
                HandleRecMsg?.Invoke(recBytes, this);
            }
        }
        public bool Send(string msgStr)
        {
            _socket.Send(Encoding.UTF8.GetBytes(msgStr));
            return true;
        }
        public bool Send(string msgStr, Encoding encoding)
        {
            _socket.Send(encoding.GetBytes(msgStr));
            return true;
        }
        public void Close()
        {
            try
            {
                _isRec = false;
                _socket.Disconnect(false);
                HandleClientClose?.Invoke(this);
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
            finally
            {
                _socket.Dispose();
                GC.Collect();
            }
        }
    }
}