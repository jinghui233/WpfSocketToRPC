using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketLib.SimpleSocket
{
    public class SimpleSocketClient
    {
        private string _ip;
        private int _port;
        private Socket _socket = null;
        private byte[] buffer = new byte[1024 * 1024 * 2];
        Action<string> infoLog;
        public SimpleSocketClient(string ip, int port)
        {
            this._ip = ip;
            this._port = port;
        }
        public bool StartClient(Action<string> action)
        {
            try
            {
                //1.0 实例化套接字(IP4寻址地址,流式传输,TCP协议)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //2.0 创建IP对象
                IPAddress address = IPAddress.Parse(_ip);
                //3.0 创建网络端口包括ip和端口
                IPEndPoint endPoint = new IPEndPoint(address, _port);
                //4.0 建立连接
                _socket.Connect(endPoint);
                Thread thread = new Thread(ReceiveMessage);
                thread.Start();
                infoLog = action;
                return true;
            }
            catch (Exception ex)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool SendMessage(string msg)
        {
            try
            {
                _socket.Send(Encoding.UTF8.GetBytes(msg));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    int length = _socket.Receive(buffer);
                    string receivedStr = Encoding.UTF8.GetString(buffer, 0, length);
                    infoLog.Invoke($@"Receive Form Server:{receivedStr}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    break;
                }
                Thread.Sleep(0);
            }
        }
    }
}
