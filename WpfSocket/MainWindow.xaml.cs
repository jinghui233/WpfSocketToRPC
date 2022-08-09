using SocketLib.Socket1;
using System;
using System.Text;
using System.Windows;

namespace SocketServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketLib.Socket1.SocketServer socketServer;
        public MainWindow()
        {
            InitializeComponent();
            socketServer = new SocketLib.Socket1.SocketServer(txtIP.Text, int.Parse(txtPort.Text));
            socketServer.HandleRecMsg = OnRecMsg;
        }

        private void OnRecMsg(byte[] arg1, SocketConnection arg2, SocketLib.Socket1.SocketServer arg3)
        {
            string txt = Encoding.UTF8.GetString(arg1);
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.Invoke(new Action(() => { richtxtLog.AppendText($"Receive:{txt}\n"); }));
            }
            else
            {
                richtxtLog.AppendText($"Receive:{txt}\n");
            }
        }

        private void btnListen_Click(object sender, RoutedEventArgs e)
        {
            if (socketServer.StartServer())
            {
                richtxtLog.AppendText("On Listening\n");
            }
        }
    }
}
