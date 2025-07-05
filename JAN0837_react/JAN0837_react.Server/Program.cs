using Microsoft.AspNetCore.SpaServices;
//using Microsoft.AspNetCore.SpaServices.Extensions;

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

            //app.UseSpaStaticFiles();

            // Configure the HTTP request pipeline.
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

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
