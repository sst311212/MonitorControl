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

        private byte[] recvBuff { get; set; }

        public RemoteServer(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            recvBuff = new byte[65536];
        }

        public void Start()
        {
            server.Start();
            server.BeginAcceptTcpClient(ClientConnectCallback, server);
        }

        private void ClientConnectCallback(IAsyncResult ar)
        {
            TcpListener server = (TcpListener)ar.AsyncState;
            TcpClient client = server.EndAcceptTcpClient(ar);
            if (client.Connected && IsLocalAddress(client.Client.RemoteEndPoint as IPEndPoint))
            {
                NetworkStream ns = client.GetStream();
                ns.BeginRead(recvBuff, 0, 4096, ReadStreamCallback, ns);
            }
            server.BeginAcceptTcpClient(ClientConnectCallback, server);
        }

        private void ReadStreamCallback(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            string recvText = Encoding.UTF8.GetString(recvBuff, 0, ns.EndRead(ar));

            if (recvText.Contains("[MRC]:"))
            {
                string msg = recvText.Split(':').LastOrDefault();
                HotkeyEvent.Hotkey_EventTrigger(null, new CustomEventArgs(msg));
            }
            else
            {
                string sendText = "<h1>MonitorRemoteControl</h1>";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"HTTP/1.0 200 OK\r\nContent-Length: {sendText.Length}\r\n");
                sb.AppendLine(sendText);
                byte[] sendBuff = Encoding.UTF8.GetBytes(sb.ToString());
                ns.Write(sendBuff, 0, sendBuff.Length);
            }
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
