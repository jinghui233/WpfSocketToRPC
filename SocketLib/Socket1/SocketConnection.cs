using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketLib.Socket1
{
    public class SocketConnection
    {
        private Socket _socket;
        private SocketServer _server = null;
        private byte[] buffer = new byte[1024 * 1024 * 4];
        private bool _isRec = true;
        public Action<byte[], SocketConnection, SocketServer> HandleRecMsg { get; set; }
        public Action<byte[], SocketConnection, SocketServer> HandleSendMsg { get; set; }
        public Action<SocketConnection, SocketServer> HandleClientClose { get; set; }
        public Action<Exception> HandleException { get; set; }
        public SocketConnection(Socket socket, SocketServer server)
        {
            _socket = socket;
            _server = server;
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
        private bool IsSocketConnected()
        {
            bool part1 = _socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (_socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public bool StartRecMsg()
        {
            try
            {
                _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecCallBack, null);
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
            return true;
        }
        private void RecCallBack(IAsyncResult asyncResult)
        {
            try
            {
                int length = _socket.EndReceive(asyncResult);
                byte[] recBytes = new byte[length];
                Array.Copy(buffer, 0, recBytes, 0, length);
                if (length > 0 && _isRec && IsSocketConnected())
                {
                    StartRecMsg();
                    HandleRecMsg?.Invoke(recBytes, this, _server);
                }
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
        }
        public void Close()
        {
            try
            {
                _isRec = false;
                _socket.Disconnect(false);
                _server.RemoveConnection(this);
                HandleClientClose?.Invoke(this, _server);
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
