using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Owin;
using Microsoft.Owin.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Host.HttpListener;

namespace JAN0837_DP.ReactFE
{
    public class FEserver
    {
        //private IHost _host;
        private IDisposable _webApp;

        public Task StartAsync(string url = "http://localhost:5000", string buildFolderPath = "wwwroot")
        {
            _webApp = WebApp.Start(url, app =>
            {
                // 1) CORS pro SignalR
                app.UseCors(CorsOptions.AllowAll);

                // 2) SignalR hub mapping
                app.Map("/signalr", map =>
                {
                    var hubConfig = new HubConfiguration
                    {
                        EnableDetailedErrors = true
                    };
                    map.RunSignalR(hubConfig);
                });

                // 3) Statické soubory z React build
                var fileSystem = new PhysicalFileSystem(buildFolderPath);
                app.UseFileServer(new FileServerOptions
                {
                    EnableDefaultFiles = true,
                    FileSystem = fileSystem,
                    StaticFileOptions = { FileSystem = fileSystem },
                    DefaultFilesOptions = { DefaultFileNames = new[] { "index.html" } }
                });
            });

            Console.WriteLine($"FE server běží na {url}, servíruje: {buildFolderPath}");
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _webApp?.Dispose();
            Console.WriteLine("FE server zastaven.");
            return Task.CompletedTask;
        }
    }
}
