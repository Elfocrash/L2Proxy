using System.Collections.Generic;
using System.Threading.Tasks;
using L2Proxy.Auth;
using L2Proxy.Proxy;
using Microsoft.AspNetCore.Mvc;

namespace L2Proxy.Controllers
{
    [ApiController]
    [ApiKey]
    [Route("api/blacklist")]
    public class BlacklistController : ControllerBase
    {
        private readonly IBlacklistService _blacklistService;
        private readonly IEnumerable<Proxy.L2Proxy> _proxies;

        public BlacklistController(IBlacklistService blacklistService, IEnumerable<Proxy.L2Proxy> proxies)
        {
            _blacklistService = blacklistService;
            _proxies = proxies;
        }

        [HttpGet("")]
        public IActionResult GetAll()
        {
            var blacklistedIps = _blacklistService.GetBlacklistedIps();
            return Ok(blacklistedIps);
        }

        [HttpGet("{ip}")]
        public IActionResult IsBlacklisted([FromRoute]string ip)
        {
            var isBlacklisted = _blacklistService.IsBlacklisted(ip);
            return Ok(isBlacklisted);
        }

        [HttpPost("{ip}")]
        public async Task<IActionResult> BlacklistIp(string ip)
        {
            await _blacklistService.BlacklistIp(ip);
            foreach (var l2Proxy in _proxies)
            {
                foreach (var (key, value) in l2Proxy.ActiveConnections)
                {
                    if (key.ToLowerInvariant().StartsWith(ip.ToLowerInvariant()))
                    {
                        value.Disconnect();
                    }
                }
            }
            return Ok();
        }

        [HttpDelete("{ip}")]
        public async Task<IActionResult> RemoveBlacklistedIp(string ip)
        {
            await _blacklistService.RemoveBlacklistedIp(ip);
            return Ok();
        }
    }
}
