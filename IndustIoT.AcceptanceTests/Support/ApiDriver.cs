using IndustIoT.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IndustIoT.AcceptanceTests.Support;

/// <summary>
/// Ponto único de criação do HttpClient usado pelos cenários.
///
/// Modo InProcess (padrão): sobe a API real (Program.cs, middlewares, JWT,
/// FluentValidation, controllers) em memória, trocando só o SQL Server por EF InMemory.
///
/// Modo Externo: se a variável de ambiente API_BASE_URL estiver definida
/// (ex.: http://localhost:8080 ou http://api:8080 no docker compose),
/// os mesmos cenários rodam contra a API publicada com SQL Server real.
/// </summary>
public static class ApiDriver
{
    private static ApiInProcessFactory? _factory;
    private static HttpClient? _client;

    public static string? BaseUrlExterna =>
        Environment.GetEnvironmentVariable("API_BASE_URL") is { Length: > 0 } url ? url.TrimEnd('/') : null;

    public static string Modo => BaseUrlExterna is null ? "InProcess (WebApplicationFactory + EF InMemory)" : $"Externo ({BaseUrlExterna})";

    public static HttpClient Client =>
        _client ?? throw new InvalidOperationException("ApiDriver não iniciado — o hook BeforeTestRun deveria ter rodado.");

    public static async Task IniciarAsync()
    {
        if (BaseUrlExterna is { } url)
        {
            _client = new HttpClient { BaseAddress = new Uri(url + "/"), Timeout = TimeSpan.FromSeconds(30) };
            await AguardarApiDisponivelAsync(_client);
        }
        else
        {
            _factory = new ApiInProcessFactory();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }
    }

    public static void Encerrar()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    /// <summary>
    /// No docker compose a API sobe antes do banco terminar as migrations;
    /// espera até a API responder para não gerar falsos negativos.
    /// </summary>
    private static async Task AguardarApiDisponivelAsync(HttpClient client)
    {
        var limite = DateTime.UtcNow.AddSeconds(
            int.TryParse(Environment.GetEnvironmentVariable("API_WAIT_SECONDS"), out var s) ? s : 120);

        Exception? ultimoErro = null;
        while (DateTime.UtcNow < limite)
        {
            try
            {
                var resposta = await client.GetAsync("api/equipamentos?pageSize=1");
                if (resposta.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[BDD] API disponível em {client.BaseAddress}");
                    return;
                }
            }
            catch (Exception ex)
            {
                ultimoErro = ex;
            }
            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        throw new InvalidOperationException(
            $"API não respondeu em {client.BaseAddress} dentro do tempo limite.", ultimoErro);
    }
}

/// <summary>
/// WebApplicationFactory dos testes de aceitação: mesma API, banco InMemory isolado por execução.
/// </summary>
public class ApiInProcessFactory : WebApplicationFactory<Program>
{
    private readonly string _nomeBanco = $"IndustIoT-BDD-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // Menos ruído no log do teste: só avisos/erros da API aparecem na saída.
        // (regras por categoria vencem o "Default: Information" do appsettings.json)
        builder.ConfigureLogging(logging => logging
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning));
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_nomeBanco));
        });
    }
}
