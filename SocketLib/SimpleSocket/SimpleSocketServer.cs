using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLib.SimpleSocket
{
    public class SimpleSocketServer
    {
        private string _ip;
        private int _port;
        private Socket _socket;
        private byte[] buffer = new byte[1024 * 1024 * 2];
        Action<string> infoLog;
        public SimpleSocketServer(string ip, int port)
        {
            this._ip = ip;
            this._port = port;
        }
        public bool StartListen(Action<string> action)
        {
            try
            {
                //1.0 实例化套接字(IP4寻找协议,流式协议,TCP协议)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //2.0 创建IP对象
                IPAddress address = IPAddress.Parse(_ip);
                //3.0 创建网络端口,包括ip和端口
                IPEndPoint endPoint = new IPEndPoint(address, _port);
                //4.0 绑定套接字
                _socket.Bind(endPoint);
                //5.0 设置最大连接数
                _socket.Listen(3);
                //6.0 开始监听
                Thread thread = new Thread(ListenClientConnect);
                thread.Start();
                infoLog = action;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void ListenClientConnect()
        {
            try
            {
                while (true)
                {
                    //Socket创建的新连接
                    Socket clientSocket = _socket.Accept();
                    infoLog.Invoke("Client Connected");
                    clientSocket.Send(Encoding.UTF8.GetBytes("Client Connected"));
                    Thread thread = new Thread(ReceiveMessage);
                    thread.Start(clientSocket);
                    Thread.Sleep(0);
                }
            }
            catch (Exception e)
            {
                infoLog.Invoke("Listen Error");
            }
        }
        private void ReceiveMessage(object socket)
        {
            Socket clientSocket = (Socket)socket;
            while (true)
            {
                try
                {
                    int length = clientSocket.Receive(buffer);
                    string receivedStr = Encoding.UTF8.GetString(buffer, 0, length);
                    infoLog.Invoke($@"Receive Form Client:{receivedStr}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    infoLog.Invoke($@"Receive Loop Error ClientSocket Close");
                    break;
                }
                Thread.Sleep(0);
            }
        }
    }
}
