namespace IndustIoT.Tests;

public class EquipamentosControllerTests : IClassFixture<IndustIoTApiFactory>
{
    private readonly HttpClient _client;

    public EquipamentosControllerTests(IndustIoTApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/equipamentos";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Esta linha verifica se o status code é 200
    }
}
