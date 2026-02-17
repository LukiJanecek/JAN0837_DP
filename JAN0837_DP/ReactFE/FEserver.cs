using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JAN0837_DP.Log;
using Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using JAN0837_DP.Data;

namespace JAN0837_DP.ReactFE
{
    public class FEserver
    {
        private FEcommunicationControl _feCommunication;
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
                // API middleware for data endpoints
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

                // Static files from React build - only if directory exists
                if (Directory.Exists(buildFolderPath))
                {
                    var fileSystem = new PhysicalFileSystem(buildFolderPath);
                    app.UseFileServer(new FileServerOptions
                    {
                        EnableDefaultFiles = true,
                        FileSystem = fileSystem,
                        StaticFileOptions = { FileSystem = fileSystem },
                        DefaultFilesOptions = { DefaultFileNames = new[] { "index.html" } }
                    });
                    Console.WriteLine($"Serving static files from: {buildFolderPath}");
                }
                else
                {
                    Console.WriteLine($"Warning: wwwroot folder not found at {buildFolderPath}. Static files will not be served.");
                    Logger.LogWarning($"wwwroot folder not found at {buildFolderPath}. Static files will not be served.");
                }
            });

            Console.WriteLine($"FE server running on {url}");
            Console.WriteLine($"Accessible at: http://{internalVariables.LocalIP}:{internalVariables.apiPort}");
            internalVariables.communicationServerStarted = true;
            return Task.CompletedTask;
        }

        public Task serverStop()
        {
            _webApp?.Dispose();
            Console.WriteLine("FE server stopped.");
            internalVariables.feServerStarted = false;
            return Task.CompletedTask;
        }
    }
}
