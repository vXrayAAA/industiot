using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IndustIoT.AcceptanceTests.Support;

/// <summary>
/// Estado de um cenário (injetado pelo Reqnroll em todas as classes de steps).
/// Guarda a última resposta HTTP, o token do usuário logado e os dados criados no "Dado".
/// </summary>
public class ContextoApi
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public HttpResponseMessage? Resposta { get; private set; }
    public string CorpoBruto { get; private set; } = string.Empty;
    public JsonNode? Corpo { get; private set; }

    public string? Token { get; set; }
    public string? UsuarioLogado { get; set; }

    /// <summary>Equipamento criado no "Dado" do cenário (id + TAG).</summary>
    public int EquipamentoId { get; set; }
    public string EquipamentoTag { get; set; } = string.Empty;
    public string SetorDoCenario { get; } = $"Setor-{Guid.NewGuid():N}"[..14];

    /// <summary>Alerta gerado durante o cenário.</summary>
    public long AlertaId { get; set; }

    public HttpStatusCode Status =>
        Resposta?.StatusCode ?? throw new InvalidOperationException("Nenhuma requisição foi enviada neste cenário.");

    public Task<HttpResponseMessage> PostAsync(string rota, object corpo, bool autenticado = false)
        => EnviarAsync(HttpMethod.Post, rota, corpo, autenticado);

    public Task<HttpResponseMessage> GetAsync(string rota, bool autenticado = false)
        => EnviarAsync(HttpMethod.Get, rota, null, autenticado);

    public Task<HttpResponseMessage> PatchAsync(string rota, bool autenticado = false)
        => EnviarAsync(HttpMethod.Patch, rota, null, autenticado);

    public async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string rota, object? corpo, bool autenticado)
    {
        using var requisicao = new HttpRequestMessage(metodo, rota.TrimStart('/'));
        if (corpo is not null)
            requisicao.Content = JsonContent.Create(corpo, options: Json);
        if (autenticado && Token is not null)
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

        Resposta = await ApiDriver.Client.SendAsync(requisicao);
        CorpoBruto = await Resposta.Content.ReadAsStringAsync();
        Corpo = string.IsNullOrWhiteSpace(CorpoBruto) ? null : JsonNode.Parse(CorpoBruto);

        Console.WriteLine($"[HTTP] {metodo} /{rota.TrimStart('/')} -> {(int)Resposta.StatusCode} {Resposta.StatusCode}");
        if (Corpo is not null)
            Console.WriteLine($"[HTTP] corpo: {Resumir(ParaLog(Corpo))}");

        return Resposta;
    }

    private static readonly JsonSerializerOptions JsonLog = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>JSON legível no log (acentos sem escape) e token JWT mascarado — segredo não vai para log.</summary>
    private static string ParaLog(JsonNode corpo)
    {
        var copia = corpo.DeepClone();
        if (copia is JsonObject obj && obj["token"] is JsonValue t && t.TryGetValue<string>(out var token) && token.Length > 16)
            obj["token"] = token[..16] + "…(mascarado)";
        return copia.ToJsonString(JsonLog);
    }

    /// <summary>Navega no JSON por caminho com pontos e índices: "items[0].severidade", "alerta.id".</summary>
    public JsonNode? Campo(string caminho)
    {
        JsonNode? atual = Corpo;
        foreach (var parte in caminho.Split('.'))
        {
            if (atual is null) return null;
            var nome = parte;
            int? indice = null;
            var abre = parte.IndexOf('[');
            if (abre >= 0)
            {
                nome = parte[..abre];
                indice = int.Parse(parte[(abre + 1)..parte.IndexOf(']')]);
            }

            if (nome.Length > 0)
                atual = atual is JsonObject obj && obj.TryGetPropertyValue(nome, out var filho) ? filho : null;
            if (indice is { } i)
                atual = atual is JsonArray arr && i < arr.Count ? arr[i] : null;
        }
        return atual;
    }

    private static string Resumir(string texto) => texto.Length <= 600 ? texto : texto[..600] + "…";
}
