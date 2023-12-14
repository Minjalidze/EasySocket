using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasySocket
{

    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream clientStream;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<Data> MessageReceived;

        public bool isListening = false;

        public void Connect(string ipAddress, int port)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, port);

            clientStream = tcpClient.GetStream();
            Connected?.Invoke();

            isListening = true;
        }
        public IEnumerator ListenToServerCoroutine()
        {
            try
            {
                if (clientStream.DataAvailable)
                {
                    byte[] messageBuffer = new byte[4096];
                    int bytesRead = clientStream.Read(messageBuffer, 0, messageBuffer.Length);

                    if (bytesRead > 0)
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

                        MessageReceived?.Invoke(data);
                    }
                }
            }
            catch
            {
                Disconnected?.Invoke();
            }

            yield return null;
        }

        public void Disconnect()
        {
            isListening = false;

            tcpClient.Close();
            Disconnected?.Invoke();
        }

        public void SendMessageToServer(Data data)
        {
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream mStream = new MemoryStream();
                binaryFormatter.Serialize(mStream, data.data);
                byte[] bytes = mStream.ToArray();

                clientStream.Write(bytes, 0, bytes.Length);
            }
            catch
            {
                Disconnected?.Invoke();
            }
        }
    }
}
