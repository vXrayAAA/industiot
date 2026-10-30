namespace IndustIoT.Tests;

public class AlertasControllerTests : IClassFixture<IndustIoTApiFactory>
{
    private readonly HttpClient _client;

    public AlertasControllerTests(IndustIoTApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/alertas";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Esta linha verifica se o status code é 200
    }
}
