using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAN0837_DP.ReactFE
{
    public class FEserver
    {
        private IHost _host;

        public async Task StartAsync(CancellationToken token = default)
        {
            // Postavíme si hosta
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel()
                        .UseUrls("http://localhost:5000")      // nebo "*:5000", jak potřebuješ
                        .ConfigureServices(services =>
                        {
                            services.AddSignalR();
                            services.AddSingleton<MyApp>();      // tvoje hlavní logika
                            services.AddHostedService<StatePusher>(); // background push
                        })
                        .Configure(app =>
                        {
                            app.UseDefaultFiles();
                            app.UseStaticFiles();               // React build složku musíš mít v wwwroot
                            app.MapHub<MyHub>("/stateHub");
                        });
                })
                .Build();

            // Spustíme webový server
            await _host.StartAsync(token);
        }

        public async Task StopAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
    }
}
