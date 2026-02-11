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
using Newtonsoft.Json;
using JAN0837_DP.Data;

namespace JAN0837_DP.ReactFE
{
    public class FEserver
    {
        private FEcommunicationControl _feCommunication;

        //private IHost _host;
        private IDisposable _webApp;

        public FEserver(FEcommunicationControl control)
        {
            _feCommunication = control ?? throw new ArgumentNullException(nameof(control));
        }

        public Task serverStart(string url = null, string buildFolderPath = "wwwroot")
        {
            // Use dynamic URL if not specified
            url ??= $"http://+:{internalVariables.apiPort}";
            
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

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path.Value.Equals("/api/data", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ctx.Request.Method == "GET")
                        {
                            var state = _feCommunication.GetCurrentState();
                            var json = JsonConvert.SerializeObject(state);
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync(json);
                            return;
                        }
                        else if (ctx.Request.Method == "POST")
                        {
                            using var reader = new StreamReader(ctx.Request.Body);
                            var body = await reader.ReadToEndAsync();
                            var incoming = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                            _feCommunication.HandleUpdate(incoming);
                            ctx.Response.StatusCode = 204;
                            return;
                        }
                    }
                    await next();
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

            Console.WriteLine($"FE server running on {url}");
            Console.WriteLine($"Accessible at: http://{internalVariables.LocalIP}:{internalVariables.apiPort}");
            Console.WriteLine($"Serving: {buildFolderPath}");
            internalVariables.communicationServerStarted = true;
            return Task.CompletedTask;
        }

        public Task serverStop()
        {
            _webApp?.Dispose();
            Console.WriteLine("FE server stopped.");
            //internalVariables.communicationServerStarted = false;
            internalVariables.feServerStarted = false;
            return Task.CompletedTask;
        }
    }
}
