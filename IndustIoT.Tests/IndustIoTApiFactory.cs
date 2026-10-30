using IndustIoT.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IndustIoT.Tests;

/// <summary>
/// Sobe a API real em memória para os testes de integração, trocando o
/// SQL Server por um banco InMemory — os testes não dependem de banco instalado.
/// </summary>
public class IndustIoTApiFactory : WebApplicationFactory<Program>
{
    // Banco isolado por instância da factory: testes de uma classe não enxergam dados de outra.
    private readonly string _databaseName = $"IndustIoT-Tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
