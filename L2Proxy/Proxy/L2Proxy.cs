using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using L2Proxy.Settings;
using Microsoft.Extensions.Logging;

namespace L2Proxy.Proxy
{
    public class L2Proxy
    {
        private readonly ILogger _logger;
        private readonly IBlacklistService _blacklistService;

        public L2Proxy(ILogger logger, ProxyInfo proxyInfo, IBlacklistService blacklistService)
        {
            _logger = logger;
            ProxyInfo = proxyInfo;
            _blacklistService = blacklistService;
        }

        public ConcurrentDictionary<string, L2ClientConnection> ActiveConnections { get; } = new();

        public ProxyInfo ProxyInfo { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var localIpAddress = string.IsNullOrEmpty(ProxyInfo.ProxyHost) ? IPAddress.IPv6Any : IPAddress.Parse(ProxyInfo.ProxyHost);
            var server = new TcpListener(new IPEndPoint(localIpAddress, ProxyInfo.ProxyPort));
            server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            server.Start();

            _logger.LogInformation($"L2Proxy started {ProxyInfo.ProxyPort} -> {ProxyInfo.L2ServerHost}:{ProxyInfo.L2ServerPort}");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var remoteClient = await server.AcceptTcpClientAsync();
                    remoteClient.NoDelay = true;

                    if (_blacklistService.IsBlacklisted(remoteClient.Client.RemoteEndPoint))
                    {
                        remoteClient?.Close();
                        remoteClient?.Dispose();
                        continue;
                    }

                    var ips = await Dns.GetHostAddressesAsync(ProxyInfo.L2ServerHost);
                    var gameserverIpEndpoint = new IPEndPoint(ips.First(), ProxyInfo.L2ServerPort);
                    var connectionKey = $"{(IPEndPoint) remoteClient.Client.RemoteEndPoint}|{gameserverIpEndpoint}";
                    var client = new L2ClientConnection(_logger, remoteClient, gameserverIpEndpoint, () =>
                    {
                        ActiveConnections.TryRemove(connectionKey, out var removedConnection);
                    });
                    _logger.LogInformation($"Established {(IPEndPoint)remoteClient.Client.RemoteEndPoint} => {gameserverIpEndpoint}");

                    ActiveConnections.TryAdd(connectionKey, client);

#pragma warning disable 4014
                    client.Handle();
#pragma warning restore 4014
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to handle connection.");
                }
            }
        }
    }
}
