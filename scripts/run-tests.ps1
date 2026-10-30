<#
.SYNOPSIS
  Executa os testes de aceitação BDD da API IndústIoT.

.PARAMETER Modo
  InProcess (padrão) — API em memória (WebApplicationFactory + EF InMemory). Só precisa do .NET 8 SDK.
  Docker             — sobe SQL Server + API + container de testes via docker compose.
  Externo            — roda contra uma API já no ar (informe -BaseUrl, ex.: http://localhost:8080).

.EXAMPLE
  .\scripts\run-tests.ps1
  .\scripts\run-tests.ps1 -Modo Docker
  .\scripts\run-tests.ps1 -Modo Externo -BaseUrl http://localhost:8080
#>
param(
    [ValidateSet("InProcess", "Docker", "Externo")]
    [string]$Modo = "InProcess",
    [string]$BaseUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $PSScriptRoot
Set-Location $raiz
$resultados = Join-Path $raiz "TestResults\$($Modo.ToLower())"
New-Item -ItemType Directory -Force -Path $resultados | Out-Null

function Invoke-TestesBdd {
    dotnet test IndustIoT.AcceptanceTests/IndustIoT.AcceptanceTests.csproj `
        --results-directory $resultados `
        --logger "trx;LogFileName=resultado-bdd.trx" `
        --logger "junit;LogFilePath=$resultados/junit-bdd.xml" `
        --logger "console;verbosity=normal" | Out-Host
    $codigo = $LASTEXITCODE
    Copy-Item "IndustIoT.AcceptanceTests\bin\Debug\net8.0\TestResults\relatorio-bdd.*" $resultados -ErrorAction SilentlyContinue
    return $codigo
}

switch ($Modo) {
    "InProcess" {
        Remove-Item Env:API_BASE_URL -ErrorAction SilentlyContinue
        $codigo = Invoke-TestesBdd
    }
    "Externo" {
        $env:API_BASE_URL = $BaseUrl
        try { $codigo = Invoke-TestesBdd } finally { Remove-Item Env:API_BASE_URL -ErrorAction SilentlyContinue }
    }
    "Docker" {
        docker compose -f docker-compose.yml -f docker-compose.tests.yml up --build `
            --abort-on-container-exit --exit-code-from acceptance-tests
        $codigo = $LASTEXITCODE
        docker compose -f docker-compose.yml -f docker-compose.tests.yml down -v
        $resultados = Join-Path $raiz "TestResults\docker"
    }
}

Write-Host ""
Write-Host "Relatórios em: $resultados" -ForegroundColor Cyan
Write-Host "  - relatorio-bdd.html  (abra no navegador)"
Write-Host "  - resultado-bdd.trx   (Visual Studio / Azure DevOps)"
Write-Host "  - junit-bdd.xml       (GitHub Actions / Jenkins)"
exit $codigo
