using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.Extensions;

namespace JAN0837_react.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            builder.Services.AddSpaStaticFiles(options =>
            {
                // musí ukazovat na dist složku, kam Vite buildí
                options.RootPath = Path.Combine("..", "JAN0837_react.client", "dist");
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                /*
                app.UseSpa(spa =>
                {
                    // relativní cesta z *.Server -> *.client
                    spa.Options.SourcePath = Path.Combine(Directory.GetCurrentDirectory(), "../JAN0837_react.client");

                    // pro Vite použijeme npm run dev
                    spa.UseProxyToSpaDevelopmentServer("npm run dev");
                    // pokud byste mìli CRA:
                    // spa.UseReactDevelopmentServer("start");
                });
                */
            }
            else
            {
                /*
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "dist";  // nebo kam Vite vybuildí
                });
                */
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // SPA proxy
            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/app"), spaApp =>
            {
                if (app.Environment.IsDevelopment())
                {
                    spaApp.UseSpa(spa =>
                    {
                        // relativnì vùèi výstupu serveru (bin/Debug/...)
                        spa.Options.SourcePath = Path.Combine("..", "JAN0837_react.client");
                        // spustí 'npm run dev' a bude proxyovat /app/* na Vite (5173)
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173"); // 
                    });
                }
                else
                {
                    spaApp.UseSpa(spa =>
                    {
                        // v produkci servírujeme už vybuildìné soubory
                        spa.Options.SourcePath = Path.Combine("..", "JAN0837_react.client", "dist");
                    });
                }
            });


            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
