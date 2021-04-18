using System.Net.Sockets;

namespace L2Proxy.Models
{
    public class L2Client
    {
        private readonly TcpClient _remoteClient;

        public L2Client(TcpClient remoteClient)
        {
            _remoteClient = remoteClient;
        }
    }
}
