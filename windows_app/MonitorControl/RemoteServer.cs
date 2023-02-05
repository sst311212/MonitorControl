using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonitorControl
{
    internal class RemoteServer
    {
        private TcpListener server { get; set; }

        public RemoteServer(int port)
        {
#if DEBUG
            server = new TcpListener(System.Net.IPAddress.Loopback, port);
#else
            server = TcpListener.Create(port);
#endif
        }

        public void Start()
        {
            server.Start();
            server.BeginAcceptTcpClient(new AsyncCallback(TcpClientCallback), server);
        }

        private static void TcpClientCallback(IAsyncResult ar)
        {
            TcpListener server = ar.AsyncState as TcpListener;
            using (TcpClient client = server.EndAcceptTcpClient(ar))
            {
                if (client.Connected && IsLocalAddress(client.Client.RemoteEndPoint as IPEndPoint))
                    ProcessClient(client);
            }
            server.BeginAcceptTcpClient(new AsyncCallback(TcpClientCallback), server);
        }

        private static void ProcessClient(TcpClient client)
        {
            try
            {
                using (Stream ss = client.GetStream())
                {
                    byte[] recvBuff = new byte[client.ReceiveBufferSize];
                    ss.Read(recvBuff, 0, recvBuff.Length);
                    string recvData = Encoding.UTF8.GetString(recvBuff);

                    if (recvData.Contains("[MRC]:"))
                    {
                        string message = recvData.Split(':').LastOrDefault();
                        HotkeyEvent.Hotkey_EventTrigger(null, new CustomEventArgs(message));
                    }
                    else
                    {
                        string sendData = "<h1>MonitorRemoteControl</h1>";
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"HTTP/1.1 200 OK\r\nContent-Length: {sendData.Length}\r\n");
                        sb.AppendLine(sendData);
                        byte[] sendBuff = Encoding.UTF8.GetBytes(sb.ToString());
                        ss.Write(sendBuff, 0, sendBuff.Length);
                    }
                }
            } catch { }
        }

        private static bool IsLocalAddress(IPEndPoint remoteIP)
        {
            byte[] ipData = remoteIP.Address.GetAddressBytes();
            Array.Reverse(ipData);
            long ipAddr = BitConverter.ToUInt32(ipData, 0);
            // 127.0.0.1
            if (ipAddr == 2130706433)
                return true;
            // 10.0.0.0/8
            if (ipAddr > 167772160 && ipAddr < 184549376)
                return true;
            // 172.16.0.0/12
            if (ipAddr > 2886729728 && ipAddr < 2886729728)
                return true;
            // 192.168.0.0/24
            if (ipAddr > 3232235520 && ipAddr < 3232301056)
                return true;
            return false;
        }
    }
}
