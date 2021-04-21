using System.Collections.Generic;
using System.Linq;
using L2Proxy.Auth;
using Microsoft.AspNetCore.Mvc;

namespace L2Proxy.Controllers
{
    [ApiController]
    [Route("api/proxies")]
    [ApiKey]
    public class ProxyController : ControllerBase
    {
        private readonly IEnumerable<Proxy.L2Proxy> _proxies;

        public ProxyController(IEnumerable<Proxy.L2Proxy> proxies)
        {
            _proxies = proxies;
        }

        [HttpGet("")]
        public IActionResult GetProxies()
        {
            return Ok(new
            {
                Proxies = _proxies.Select(x=> new
                {
                    x.ProxyInfo.ProxyHost,
                    x.ProxyInfo.ProxyPort,
                    x.ProxyInfo.L2ServerHost,
                    x.ProxyInfo.L2ServerPort,
                    ActiveConnectionCount = _proxies
                        .Single(xx => xx.ProxyInfo.ProxyHost == x.ProxyInfo.ProxyHost && xx.ProxyInfo.ProxyPort == x.ProxyInfo.ProxyPort)
                        .ActiveConnections.Count,
                    x.ProxyInfo.MaxConnections
                }).ToList()
            });
        }

        [HttpGet("{proxyHost}/{proxyPort}")]
        public IActionResult GetActiveConnections(string proxyHost, ushort proxyPort)
        {
            var proxy = _proxies.SingleOrDefault(x =>
                x.ProxyInfo.ProxyHost == proxyHost && x.ProxyInfo.ProxyPort == proxyPort);

            if (proxy is null)
            {
                return NotFound();
            }

            var activeConnections = proxy.ActiveConnections.Values;

            return Ok(new
            {
                ActiveConnectionCount = activeConnections.Count,
                ActiveConnections = activeConnections.Select(x => x.ClientEndpoint.ToString()).ToList(),
                proxy.ProxyInfo.MaxConnections
            });
        }

        [HttpDelete("{proxyHost}/{proxyPort}/{clientHost}/{clientPort}")]
        public IActionResult DisconnectClient(string proxyHost, ushort proxyPort, string clientHost, ushort clientPort)
        {
            var proxy = _proxies.SingleOrDefault(x =>
                x.ProxyInfo.ProxyHost == proxyHost && x.ProxyInfo.ProxyPort == proxyPort);

            if (proxy is null)
            {
                return NotFound();
            }

            var connectionKey = $"{clientHost}:{clientPort}|{proxy.ProxyInfo.L2ServerHost}:{proxy.ProxyInfo.L2ServerPort}";
            proxy.ActiveConnections.TryGetValue(connectionKey, out var connection);
            connection?.Disconnect();
            return NoContent();
        }
    }
}
