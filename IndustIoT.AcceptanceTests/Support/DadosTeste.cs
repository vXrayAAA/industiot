namespace IndustIoT.AcceptanceTests.Support;

/// <summary>
/// Massa de dados dos cenários. TAGs aleatórias no padrão de planta (AAA-999)
/// deixam os cenários independentes entre si — inclusive quando rodam várias vezes
/// contra o mesmo SQL Server no modo externo.
/// </summary>
public static class DadosTeste
{
    private const string Letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string NovaTag()
    {
        var letras = new string(Enumerable.Range(0, 3).Select(_ => Letras[Random.Shared.Next(Letras.Length)]).ToArray());
        return $"{letras}-{Random.Shared.Next(100, 1000)}";
    }

    /// <summary>
    /// Usuários de demonstração definidos em IndustIoT.Api/appsettings.json (Auth:Usuarios).
    /// Podem ser sobrescritos por variáveis de ambiente para rodar contra outro ambiente.
    /// </summary>
    public static (string Usuario, string Senha) Credencial(string perfil) => perfil.ToLowerInvariant() switch
    {
        "gestor" => (Env("BDD_GESTOR_USUARIO", "rodrigo.m"), Env("BDD_GESTOR_SENHA", "IndustIoT@2026")),
        "operador" => (Env("BDD_OPERADOR_USUARIO", "operador.l2"), Env("BDD_OPERADOR_SENHA", "Operador@2026")),
        _ => throw new ArgumentException($"Perfil desconhecido: {perfil}")
    };

    private static string Env(string nome, string padrao)
        => Environment.GetEnvironmentVariable(nome) is { Length: > 0 } v ? v : padrao;
}
