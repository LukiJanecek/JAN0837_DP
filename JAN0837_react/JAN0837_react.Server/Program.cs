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

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();

                // Spouštět klienta z .cs nebudeme,
                // jen proxy na dev-server:
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = clientProjectDirectory;
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
                });
            }

            //app.UseDefaultFiles();
            app.UseHttpsRedirection();
            //app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
