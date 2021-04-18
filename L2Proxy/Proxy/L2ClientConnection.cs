using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace L2Proxy.Proxy
{
    public class L2ClientConnection
    {
        private readonly ILogger _logger;
        private readonly TcpClient _remoteClient;
        private readonly IPEndPoint _remoteServerIpEndPoint;
        private readonly Action _onDisconnect;
        private readonly TcpClient _client;

        public L2ClientConnection(ILogger logger, TcpClient remoteClient, IPEndPoint remoteServerIpEndPoint,
            Action onDisconnect)
        {
            _client = new TcpClient();
            _logger = logger;
            _remoteClient = remoteClient;
            _remoteServerIpEndPoint = remoteServerIpEndPoint;
            _onDisconnect = onDisconnect;
            _client.NoDelay = true;
            ClientEndpoint = (IPEndPoint) _remoteClient.Client.RemoteEndPoint;
        }

        public IPEndPoint ClientEndpoint { get; }

        public async Task Handle()
        {
            try
            {
                using (_remoteClient)
                using (_client)
                {
                    await _client.ConnectAsync(_remoteServerIpEndPoint.Address, _remoteServerIpEndPoint.Port);
                    var serverStream = _client.GetStream();
                    var remoteStream = _remoteClient.GetStream();
                    await Task.WhenAny(remoteStream.CopyToAsync(serverStream), serverStream.CopyToAsync(remoteStream));
                }
            }
            catch{}
            finally
            {
                _logger.LogInformation($"Closed {ClientEndpoint} => {_remoteServerIpEndPoint}");
                _onDisconnect.Invoke();
                _remoteClient?.Dispose();
                _client?.Dispose();
            }
        }

        public override string ToString()
        {
            return ClientEndpoint.ToString();
        }

        public void Disconnect()
        {
            _remoteClient?.Close();
            _remoteClient?.Dispose();
            _client?.Close();
            _client?.Dispose();
        }
    }
}
