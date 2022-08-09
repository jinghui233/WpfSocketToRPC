using System;
using System.Text;
using System.Windows;

namespace SocketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketLib.Socket1.SocketClient socketClient;
        public MainWindow()
        {
            InitializeComponent();
            socketClient = new SocketLib.Socket1.SocketClient(txtIP.Text, int.Parse(txtPort.Text));
            socketClient.HandleRecMsg = OnRecMsg;
        }

        private void OnRecMsg(byte[] arg1, SocketLib.Socket1.SocketClient arg2)
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

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
           if(socketClient.StartClient())
            {
                richtxtLog.AppendText("connected\n");
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
           if(socketClient.Send(txtMsg.Text))
            {
                richtxtLog.AppendText($"send:{txtMsg.Text}\n");
            }
        }
    }
}
