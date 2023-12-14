using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace EasySocket
{
    public class Server
    {
        public TcpListener tcpListener;
        public List<ClientInfo> clients;

        public event Action<ClientInfo> ClientConnected;
        public event Action<ClientInfo> ClientDisconnected;
        public event Action<ClientInfo, Data> MessageReceived;

        public Server(string ip, int port)
        {
            tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            clients = new List<ClientInfo>();
        }

        public void Start()
        {
            tcpListener.Start();
            while (true)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                ClientInfo clientInfo = new ClientInfo { TcpClient = tcpClient, ClientId = Guid.NewGuid().ToString() };
                clients.Add(clientInfo);

                ClientConnected?.Invoke(clientInfo);

                var clientHandler = new Thread(() => HandleClientCommunication(clientInfo));
                clientHandler.Start();
            }
        }

        private void HandleClientCommunication(ClientInfo clientInfo)
        {
            try
            {
                NetworkStream clientStream = clientInfo.TcpClient.GetStream();
                byte[] messageBuffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = clientStream.Read(messageBuffer, 0, messageBuffer.Length)) > 0)
                {
                    var recBuf = new byte[bytesRead];
                    Array.Copy(messageBuffer, recBuf, bytesRead);

                    MemoryStream mStream = new MemoryStream();
                    BinaryFormatter binaryFormatter = new BinaryFormatter();

                    mStream.Write(recBuf, 0, recBuf.Length);
                    mStream.Position = 0L;

                    Dictionary<string, object> myObject = binaryFormatter.Deserialize(mStream) as Dictionary<string, object>;
                    Data data = new Data
                    {
                        data = myObject
                    };

                    MessageReceived?.Invoke(clientInfo, data);
                }
            }
            catch (Exception ex)
            {
                ClientDisconnected?.Invoke(clientInfo);
            }
        }

        public void DisconnectClient(ClientInfo clientInfo)
        {
            clients.Remove(clientInfo);
            clientInfo.TcpClient.Close();
            ClientDisconnected?.Invoke(clientInfo);
        }

        public void SendMessageToClient(ClientInfo clientInfo, Data data)
        {
            try
            {
                NetworkStream clientStream = clientInfo.TcpClient.GetStream();
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream mStream = new MemoryStream();
                binaryFormatter.Serialize(mStream, data.data);
                byte[] bytes = mStream.ToArray();

                clientStream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                ClientDisconnected?.Invoke(clientInfo);
            }
        }
    }
}
