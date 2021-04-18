using System.Collections.Generic;
using System.Linq;
using L2Proxy.Proxy;
using L2Proxy.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace L2Proxy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHostedService<ProxyService>();
            services.AddSingleton<IBlacklistService, BlacklistService>();
            services.AddSingleton<IEnumerable<Proxy.L2Proxy>>(x =>
            {
                var proxySettings = x.GetRequiredService<IOptions<ProxySettings>>().Value;
                var logger = x.GetRequiredService<ILogger<Proxy.L2Proxy>>();
                var blacklistService = x.GetRequiredService<IBlacklistService>();
                return proxySettings.Proxies.Select(xx => new Proxy.L2Proxy(logger, xx, blacklistService)).ToList();
            });
            services.Configure<ProxySettings>(Configuration.GetSection(nameof(ProxySettings)));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
