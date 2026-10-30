# IndústIoT — Testes Automatizados BDD (Gherkin + Reqnroll)

Testes de aceitação da API IndústIoT (Tema ESG 1 — eficiência energética) escritos em **Gherkin (português)**
e executados com **Reqnroll** (sucessor open source do SpecFlow) sobre **xUnit**, cobrindo **todas as 9 APIs** com:

- validação de **status code**;
- validação do **corpo da resposta (JSON)** — campo a campo;
- **testes de contrato com JSON Schema** (draft 2020-12) em todas as respostas com corpo.

---

## 1. Estrutura

```
IndustIoT.AcceptanceTests/
├── Features/                         Cenários Gherkin (5 funcionalidades, 39 cenários executados)
│   ├── 01_Autenticacao.feature
│   ├── 02_Equipamentos.feature
│   ├── 03_LeiturasEAlertaAutomatico.feature
│   ├── 04_GestaoDeAlertas.feature
│   └── 05_ComandoDesligamentoRemoto.feature
├── StepDefinitions/                  Implementação dos passos (Dado/Quando/Então)
│   ├── RespostaSteps.cs              status code, campos do JSON, contrato (reutilizáveis)
│   ├── AutenticacaoSteps.cs
│   ├── EquipamentoSteps.cs
│   ├── LeituraEAlertaSteps.cs
│   └── ComandoSteps.cs
├── Schemas/                          Contratos JSON Schema (11 arquivos)
├── Support/
│   ├── ApiDriver.cs                  escolhe o modo: InProcess ou API externa (API_BASE_URL)
│   ├── ContextoApi.cs                estado do cenário + cliente HTTP
│   ├── ValidadorContrato.cs          validação JSON Schema (JsonSchema.Net)
│   ├── DadosTeste.cs                 massa de dados (TAGs únicas, usuários demo)
│   └── Hooks.cs
├── reqnroll.json                     idioma pt-BR + relatório HTML
└── xunit.runner.json
Dockerfile.tests                      imagem que roda os cenários contra a API em container
docker-compose.tests.yml              adiciona o container de testes ao docker-compose.yml
scripts/run-tests.ps1 | run-tests.sh  execução em um comando
.github/workflows/testes-bdd.yml      pipeline de CI (GitHub Actions)
```

## 2. Pré-requisitos

| Modo | Precisa de |
|---|---|
| InProcess (padrão) | [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Docker | Docker Desktop (com Docker Compose v2) |
| Externo | .NET 8 SDK + API já rodando (ex.: `docker compose up`) |

## 3. Como executar

### Modo InProcess — mais rápido (≈5 s)
Sobe a API real em memória (`WebApplicationFactory<Program>`: mesmo `Program.cs`, middlewares, JWT,
FluentValidation e controllers), trocando só o SQL Server por EF InMemory.

```powershell
# Windows
.\scripts\run-tests.ps1

# Linux/macOS/Git Bash
./scripts/run-tests.sh

# ou direto
dotnet test IndustIoT.AcceptanceTests
```

### Modo Docker — API + SQL Server reais
Os **mesmos cenários** rodam em um container contra a API publicada com SQL Server 2022.

```powershell
.\scripts\run-tests.ps1 -Modo Docker
# equivalente a:
docker compose -f docker-compose.yml -f docker-compose.tests.yml up --build --abort-on-container-exit --exit-code-from acceptance-tests
docker compose -f docker-compose.yml -f docker-compose.tests.yml down -v
```

### Modo Externo — contra uma API já no ar
```powershell
docker compose up -d --build                       # sobe API + banco
.\scripts\run-tests.ps1 -Modo Externo -BaseUrl http://localhost:8080
```
(No bash: `API_BASE_URL=http://localhost:8080 dotnet test IndustIoT.AcceptanceTests`.)

### Filtrar por tag
```powershell
dotnet test IndustIoT.AcceptanceTests --filter "Category=negativo"
dotnet test IndustIoT.AcceptanceTests --filter "Category=esg-governanca"
dotnet test IndustIoT.AcceptanceTests --filter "Category=contrato"
```

## 4. Relatórios gerados

Ficam em `TestResults/<modo>/`:

| Arquivo | Uso |
|---|---|
| `relatorio-bdd.html` | relatório Cucumber/Reqnroll — abra no navegador (cenários, passos, status) |
| `resultado-bdd.trx` | Visual Studio / Azure DevOps |
| `junit-bdd.xml` | GitHub Actions, Jenkins, GitLab |

## 5. Pipeline de CI (GitHub Actions)

`.github/workflows/testes-bdd.yml` roda a cada push/PR em 3 jobs encadeados:

1. **Build + testes xUnit** existentes (`IndustIoT.Tests`)
2. **BDD (InProcess)** — publica o resultado na aba *Checks* e os relatórios como artefato
3. **BDD (API + SQL Server em Docker)** — mesmos cenários contra o ambiente containerizado

Para usar: suba a pasta da solução para um repositório GitHub (o `.github/` precisa ficar na raiz do repositório)
e acompanhe em **Actions → Testes BDD - IndústIoT**.

## 6. Cobertura das APIs

| Endpoint | Cenários positivos | Cenários negativos | Contrato |
|---|---|---|---|
| `POST /api/auth/login` | 200 gestor/operador | 401 senha errada, 401 usuário inexistente | login-response, problem-details |
| `POST /api/equipamentos` | 201 + Location | 422 TAG duplicada, 400 ×4 validações | equipamento, problem-details, validation-problem |
| `GET /api/equipamentos` | 200 paginado + filtro setor | — | paged-equipamentos |
| `GET /api/equipamentos/{id}` | 200 | 404 | equipamento, problem-details |
| `GET /api/equipamentos/{id}/consumo` | 200 ordenado | 404 | paged-consumo, problem-details |
| `POST /api/equipamentos/{id}/comando-desligamento` | 200 + auditoria | 422 já desligado, 403 operador, 401 sem token, 404, 400 motivo longo | comando, problem-details, validation-problem |
| `POST /api/leituras` | 201 sem alerta ×2, 201 com alerta ×5 (severidades/fronteiras) | 404 TAG, 400 ×4 validações | leitura-registrada (+ alerta), problem-details, validation-problem |
| `GET /api/alertas` | 200 filtro severidade/resolvido | — | paged-alertas |
| `PATCH /api/alertas/{id}/resolver` | 200 | 422 repetido, 401 sem token, 404 | alerta, problem-details |

## 7. Relação com ESG

| Pilar | O que os testes garantem |
|---|---|
| **E — Ambiental** | alerta automático de desperdício com severidade proporcional (fronteiras 20%/50%), histórico de consumo, desligamento remoto de equipamento ocioso |
| **S — Social** | mensagens de erro claras em português (ProblemDetails), perfis de usuário distintos (operador × gestor) |
| **G — Governança** | controle de acesso por role (401/403), trilha de auditoria (`solicitadoPor`), integridade dos dados (validações 400, unicidade 422), contratos estáveis (JSON Schema) |

## 8. Credenciais usadas

Os usuários de demonstração vêm de `IndustIoT.Api/appsettings.json`. Para outro ambiente, sobrescreva com
`BDD_GESTOR_USUARIO`, `BDD_GESTOR_SENHA`, `BDD_OPERADOR_USUARIO`, `BDD_OPERADOR_SENHA`.

## 9. Equipe

| Integrante | RM |
|---|---|
| Nicolas Mantovani | 562306 |
| Rafaela Alves | 563377 |
| Giovanni Assunção Wanderley | 558985 |
| Othon Godoy | 563437 |
| Vinícius Branco | 565389 |
