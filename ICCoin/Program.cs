
using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using Data.Models;
using ICCoin.Workers;
using Microsoft.EntityFrameworkCore;

namespace ICCoin;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        

        var configuration = builder.Configuration;

        var services = builder.Services;
        // Add services to the container.
        //builder.Services.AddDbContext<ICContext>(options =>
        //          options.UseSqlServer("Server=51.12.247.74;Database=Ocean;User Id=sa;Password=zeka7744;MultipleActiveResultSets=true;TrustServerCertificate=True"));
        var optionBuilder = new DbContextOptionsBuilder<ICContext>();
        optionBuilder.UseSqlServer("Server=51.12.247.74;Database=Ocean;User Id=sa;Password=zeka7744;MultipleActiveResultSets=true;TrustServerCertificate=True");
        services.AddTransient<ICContext>(d => new ICContext(optionBuilder.Options));

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHostedService<Worker>();


        builder.Services.AddBinance();

        BinanceRestClient.SetDefaultOptions(options =>
        {
            options.ApiCredentials = new ApiCredentials(configuration.GetSection("AppSettings:apiKey").Value!, configuration.GetSection("AppSettings:secretKey").Value!); 
        });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
