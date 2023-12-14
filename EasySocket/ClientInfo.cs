using System.Net.Sockets;

namespace EasySocket
{
    public class ClientInfo
    {
        public TcpClient TcpClient { get; set; }
        public string ClientId { get; set; }
    }
}
