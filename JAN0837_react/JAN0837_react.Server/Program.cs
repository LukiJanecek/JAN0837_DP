using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.Extensions;
using Microsoft.AspNetCore.SpaProxy;

using System.Diagnostics;
using System.IO;

namespace JAN0837_react.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string serverProjectDirectory = builder.Environment.ContentRootPath;
            string solutionDirectory = Directory.GetParent(serverProjectDirectory)!.FullName;

            string clientProjectDirectory = Path.Combine(solutionDirectory, "JAN0837_react.client");

            string serverCsprojPath = Path.Combine(serverProjectDirectory, "JAN0837_react.Server.csproj");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSpaStaticFiles(options =>
            {
                // mus� ukazovat na dist slo�ku, kam Vite build�
                options.RootPath = Path.Combine(clientProjectDirectory, "dist");
            });

            var app = builder.Build();

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /*
            // file path testing 
            Console.WriteLine($"[DEBUG] ContentRootPath = {builder.Environment.ContentRootPath}");
            var clientDir = Path.Combine(builder.Environment.ContentRootPath,
                             "..",       // z net8.0 do bin/Debug/net8.0
                             "..",       // do bin/Debug
                             "..",       // do bin
                             "..",       // do JAN0837_react.Server
                             "jan0837_react.client");  // složka klienta
                var fullClientDir = Path.GetFullPath(clientDir);
                Console.WriteLine($"[DEBUG] Full clientDir = {fullClientDir}");
                Console.WriteLine($"[DEBUG] Exists? {Directory.Exists(fullClientDir)}");
        */
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();

                // start Reacts dev server -> FE
                Process.Start(new ProcessStartInfo
                {
                    FileName = "npm",
                    Arguments = "run dev",
                    WorkingDirectory = clientProjectDirectory,
                    UseShellExecute = true,
                    CreateNoWindow = false
                });

                // start dotnet run server -> BE
                Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{serverCsprojPath}\"",
                    WorkingDirectory = serverProjectDirectory,
                    UseShellExecute = true
                });
            }

            //app.UseDefaultFiles();
            app.UseHttpsRedirection();
            //app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = clientProjectDirectory;

                if (app.Environment.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
                }
                else
                {
                    // in production, serve the files from ../client/dist
                    spa.Options.SourcePath = Path.Combine(clientProjectDirectory, "dist");
                }
            }); 

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
