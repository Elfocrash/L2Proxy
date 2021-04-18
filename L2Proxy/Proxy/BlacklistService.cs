using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace L2Proxy.Proxy
{
    public class BlacklistService : IBlacklistService
    {
        private const string BlacklistFilePath = "./blacklist.txt";
        private readonly List<string> _blacklistedIps = new();
        private readonly ILogger<BlacklistService> _logger;

        public BlacklistService(ILogger<BlacklistService> logger)
        {
            _logger = logger;
            if (!File.Exists(BlacklistFilePath))
            {
                using (File.Create(BlacklistFilePath)){}
                logger.LogInformation("Created a new blacklist.txt file");
            }

            foreach (var ipLine in File.ReadAllLines(BlacklistFilePath))
            {
                _blacklistedIps.Add(ipLine.ToLowerInvariant());
            }

            _blacklistedIps = _blacklistedIps.Distinct().ToList();
            logger.LogInformation($"Loaded {_blacklistedIps.Count} blacklisted ips");
        }

        public async Task BlacklistIp(string ip)
        {
            if (!IsBlacklisted(ip))
            {
                _blacklistedIps.Add(ip.ToLowerInvariant());
                await File.WriteAllLinesAsync(BlacklistFilePath, _blacklistedIps);
                _logger.LogInformation($"Blacklisted IP {ip}");
            }
        }

        public IEnumerable<string> GetBlacklistedIps()
        {
            return _blacklistedIps;
        }

        public bool IsBlacklisted(EndPoint endpoint)
        {
            if (endpoint is null)
            {
                return false;
            }

            var ip = StripPortFromEndPoint(endpoint.ToString());

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrEmpty(ip))
            {
                return false;
            }

            return _blacklistedIps.Contains(ip.ToLowerInvariant());
        }

        public bool IsBlacklisted(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return false;
            }

            var ipWithoutPort = StripPortFromEndPoint(ip);

            if (string.IsNullOrWhiteSpace(ipWithoutPort) || string.IsNullOrEmpty(ipWithoutPort))
            {
                return false;
            }

            return _blacklistedIps.Contains(ipWithoutPort.ToLowerInvariant());
        }

        public async Task RemoveBlacklistedIp(string ip)
        {
            if (!IsBlacklisted(ip.ToLowerInvariant()))
            {
                return;
            }

            _blacklistedIps.Remove(ip.ToLowerInvariant());
            await File.WriteAllLinesAsync(BlacklistFilePath, _blacklistedIps);
            _logger.LogInformation($"Removed IP {ip} from the blacklist");
        }

        private string StripPortFromEndPoint(string endPoint)
        {
            var splitList = endPoint.Split(':');
            endPoint = splitList.Length switch
            {
                > 2 => IPAddress.Parse(endPoint).ToString(),
                2 => splitList[0],
                _ => endPoint
            };

            return endPoint;
        }
    }

    public interface IBlacklistService
    {
        Task BlacklistIp(string ip);

        IEnumerable<string> GetBlacklistedIps();

        bool IsBlacklisted(EndPoint ip);

        bool IsBlacklisted(string ip);

        Task RemoveBlacklistedIp(string ip);
    }
}
