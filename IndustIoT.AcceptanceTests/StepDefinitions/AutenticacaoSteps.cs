using System.Text;
using System.Text.Json;
using IndustIoT.AcceptanceTests.Support;
using Reqnroll;

namespace IndustIoT.AcceptanceTests.StepDefinitions;

[Binding]
public class AutenticacaoSteps
{
    private readonly ContextoApi _ctx;

    public AutenticacaoSteps(ContextoApi ctx) => _ctx = ctx;

    [When("eu faço login como {string}")]
    public async Task QuandoLoginComoPerfil(string perfil)
    {
        var (usuario, senha) = DadosTeste.Credencial(perfil);
        await _ctx.PostAsync("api/auth/login", new { usuario, senha });
    }

    [When("eu faço login com usuário {string} e senha {string}")]
    public async Task QuandoLogin(string usuario, string senha)
        => await _ctx.PostAsync("api/auth/login", new { usuario, senha });

    [Given("que estou autenticado como {string}")]
    public async Task DadoAutenticado(string perfil)
    {
        var (usuario, senha) = DadosTeste.Credencial(perfil);
        var resposta = await _ctx.PostAsync("api/auth/login", new { usuario, senha });
        Assert.True(resposta.IsSuccessStatusCode, $"Pré-condição: login como {perfil} falhou ({(int)resposta.StatusCode}).");

        _ctx.Token = _ctx.Campo("token")!.GetValue<string>();
        _ctx.UsuarioLogado = _ctx.Campo("usuario")!.GetValue<string>();
    }

    [Then("o token retornado deve ser um JWT com expiração futura")]
    public void EntaoTokenValido()
    {
        var token = _ctx.Campo("token")!.GetValue<string>();
        var partes = token.Split('.');
        Assert.Equal(3, partes.Length);

        // Decodifica o payload (base64url) e confere a claim "exp" — sem depender da chave de assinatura.
        var payload = partes[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        using var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));

        var exp = DateTimeOffset.FromUnixTimeSeconds(json.RootElement.GetProperty("exp").GetInt64());
        Assert.True(exp > DateTimeOffset.UtcNow, $"Token já expirado: exp={exp:O}");
        Assert.Equal("IndustIoT.Api", json.RootElement.GetProperty("iss").GetString());
    }

    [Then("o campo {string} da resposta deve ser o usuário logado")]
    public void EntaoCampoUsuarioLogado(string campo)
        => Assert.Equal(_ctx.UsuarioLogado, RespostaSteps.Texto(_ctx.Campo(campo)));
}
