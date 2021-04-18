using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace L2Proxy.Proxy
{
    public class ProxyService : BackgroundService
    {
        private readonly IEnumerable<L2Proxy> _proxies;

        public ProxyService(IEnumerable<L2Proxy> proxies)
        {
            _proxies = proxies;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var proxyTasks = _proxies.Select(x => x.StartAsync(stoppingToken));
            return Task.WhenAll(proxyTasks);
        }
    }
}
