using System.Net.Http.Json;
using IndustIoT.Api.ViewModels.Requests;

namespace IndustIoT.Tests;

public class AuthControllerTests : IClassFixture<IndustIoTApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(IndustIoTApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_ReturnsHttpStatusCode200()
    {
        // Arrange
        var login = new LoginRequest { Usuario = "rodrigo.m", Senha = "IndustIoT@2026" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);

        // Assert
        response.EnsureSuccessStatusCode(); // Esta linha verifica se o status code é 200
    }
}
