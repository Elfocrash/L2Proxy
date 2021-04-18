using System.Collections.Generic;

namespace L2Proxy.Settings
{
    public class ProxySettings
    {
        public ICollection<ProxyInfo> Proxies { get; set; }
    }

    public class ProxyInfo
    {
        public string ProxyHost { get; init; }

        public ushort ProxyPort { get; init; }

        public string L2ServerHost { get; init; }

        public ushort L2ServerPort { get; init; }
    }
}
