using Reqnroll;

namespace IndustIoT.AcceptanceTests.Support;

[Binding]
public class Hooks
{
    [BeforeTestRun]
    public static async Task AntesDaExecucao()
    {
        Console.WriteLine($"[BDD] Modo de execução: {ApiDriver.Modo}");
        await ApiDriver.IniciarAsync();
    }

    [AfterTestRun]
    public static void DepoisDaExecucao() => ApiDriver.Encerrar();

    [BeforeScenario]
    public void AntesDoCenario(ScenarioContext cenario)
        => Console.WriteLine($"[BDD] Cenário: {cenario.ScenarioInfo.Title} [{string.Join(", ", cenario.ScenarioInfo.Tags)}]");
}
