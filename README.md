# IndústIoT — API de Eficiência Energética (ESG)

API RESTful em **.NET 8** para monitoramento de consumo de energia em equipamentos industriais, com alertas automáticos quando o consumo ultrapassa o limite configurado e **comando remoto de desligamento** de equipamentos ociosos (Tema ESG 1 — Eficiência energética e sustentabilidade).

Inclui um **dashboard web** servido pela própria API, no estilo do protótipo IndústIoT.

![Testes BDD](https://github.com/vXrayAAA/industiot/actions/workflows/testes-bdd.yml/badge.svg)

## O que tem neste repositório

| Pasta | Conteúdo | Fase FIAP |
|---|---|---|
| `IndustIoT.Api/` | API REST .NET 8 (EF Core, JWT, FluentValidation, dashboard web) | Desafio .NET Core 8 / ESG |
| `IndustIoT.Tests/` | Testes de integração xUnit | Desafio .NET Core 8 / ESG |
| `IndustIoT.AcceptanceTests/` | Testes BDD em Gherkin (Reqnroll) + contratos JSON Schema — ver [README-TESTES.md](README-TESTES.md) | Testes Automatizados |
| `.github/workflows/` | Pipeline de CI (xUnit → BDD InProcess → BDD Docker + SQL Server) | Testes Automatizados |
| `Postman/` | Collection para testar a API manualmente | Desafio .NET Core 8 / ESG |
| `evidencias/` | Prints de configuração e de execução dos testes | Testes Automatizados |
| `docs/entregas/` | Documentação entregue (PDF) da fase de testes automatizados | Testes Automatizados |
| `docs/startup-one/` | Atividade de prototipação (Cap. 08) e prints das telas | Startup One — Fase 04 |
| `prototipo/` | Protótipo navegável (HTML) — publicado em [vxrayaaa.github.io/industiot-prototipo](https://vxrayaaa.github.io/industiot-prototipo/) | Startup One — Fase 04 |

### Equipe

Nicolas Mantovani (RM 562306) · Giovanni Assunção Wanderley (RM 558985) · Othon Godoy (RM 563437) · Vinícius Branco (RM 565389)

## Arquitetura (MVVM)

```
IndustIoT.Api/
├── Models/          Entidades de domínio (Equipamento, LeituraSensor, Alerta, ComandoAtuacao)
├── ViewModels/      Contratos da API (requests, responses, PagedResult<T>)
├── Services/        Lógica de negócio (alerta automático, severidade, comandos)
├── Controllers/     Endpoints REST (apenas orquestração)
├── Validators/      FluentValidation (requests inválidos → 400)
├── Middlewares/     Tratamento global de exceções (ProblemDetails → 404/422/500)
├── Data/            AppDbContext (EF Core + SQL Server, índices de performance)
├── Migrations/      Migrations do banco
└── wwwroot/         Dashboard web (index.html)

IndustIoT.Tests/     Testes de integração xUnit (WebApplicationFactory + EF InMemory)
IndustIoT.AcceptanceTests/  Testes BDD (Gherkin + Reqnroll) + contratos JSON Schema
```

---

## Opção 1 — Rodar com Docker (mais simples)

**Pré-requisito:** Docker Desktop instalado e rodando.

```bash
docker compose up --build
```

O compose sobe o **SQL Server 2022** e a **API**, aplicando as migrations automaticamente na inicialização (`AplicarMigracoes=true`).

| Recurso | URL |
|---|---|
| Dashboard | http://localhost:8080/ |
| Swagger | http://localhost:8080/swagger |

---

## Opção 2 — Rodar local (SDK + SQL Server LocalDB)

**Pré-requisitos:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (incluído no Visual Studio, ou instalador standalone do SQL Server Express → opção LocalDB)

### Passo a passo

```powershell
# 1. Instalar a ferramenta de migrations (uma única vez)
dotnet tool install --global dotnet-ef

# 2. Garantir que a instância LocalDB existe e está iniciada
sqllocaldb create MSSQLLocalDB -s

# 3. Aplicar as migrations (cria o banco IndustIoT)
cd IndustIoT.Api
dotnet ef database update

# 4. Subir a API
dotnet run --urls "http://localhost:5180"
```

| Recurso | URL |
|---|---|
| Dashboard | http://localhost:5180/ |
| Swagger | http://localhost:5180/swagger |

> A connection string padrão aponta para `(localdb)\MSSQLLocalDB` e está em `IndustIoT.Api/appsettings.json`. Para usar outro SQL Server, ajuste `ConnectionStrings:DefaultConnection`.

---

## Rodar os testes

```powershell
dotnet test                      # xUnit (5 testes) + BDD (39 cenários Gherkin)
.\scripts\run-tests.ps1          # só os cenários BDD, com relatórios HTML/TRX/JUnit
.\scripts\run-tests.ps1 -Modo Docker   # cenários BDD contra API + SQL Server em containers
```

- `IndustIoT.Tests/` — 5 testes de integração xUnit (`WebApplicationFactory` + InMemory).
- `IndustIoT.AcceptanceTests/` — testes de aceitação **BDD em Gherkin (Reqnroll)** cobrindo todas as APIs:
  status code, corpo JSON e contrato JSON Schema. Detalhes em **[README-TESTES.md](README-TESTES.md)**.
- Pipeline de CI: `.github/workflows/testes-bdd.yml` (GitHub Actions).

---

## Credenciais de demonstração

| Usuário | Senha | Role | Pode |
|---|---|---|---|
| `rodrigo.m` | `IndustIoT@2026` | Gestor | tudo, inclusive comando de desligamento |
| `operador.l2` | `Operador@2026` | Operador | resolver alertas; comando de desligamento → **403** |

---

## Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/auth/login` | — | Emite o token JWT |
| POST | `/api/equipamentos` | — | Cadastra equipamento (TAG única; duplicada → 422) |
| GET | `/api/equipamentos` | — | Lista paginada (`pageNumber`, `pageSize`, filtro `setor`) |
| GET | `/api/equipamentos/{id}` | — | Detalhe do equipamento |
| GET | `/api/equipamentos/{id}/consumo` | — | Histórico de consumo paginado (filtros `inicio`/`fim`) |
| POST | `/api/equipamentos/{id}/comando-desligamento` | **JWT Gestor** | Desligamento remoto, com auditoria de quem solicitou |
| POST | `/api/leituras` | — | Recebe leitura do sensor; consumo > limite gera alerta automático na mesma transação |
| GET | `/api/alertas` | — | Lista paginada (filtros `resolvido`, `severidade`), ordenada por severidade |
| PATCH | `/api/alertas/{id}/resolver` | JWT | Marca alerta como resolvido (repetido → 422) |

### Regra de severidade do alerta

Quando `consumoKwh > limiteConsumoKwh` do equipamento:

| Excesso sobre o limite | Severidade |
|---|---|
| até 20% | `Atencao` |
| 20% a 50% | `Alta` |
| acima de 50% | `Critica` |

---

## Testar com o Postman

1. Importe `Postman/IndustIoT_Postman_Collection.json`.
2. Ajuste a variável `base_url` da collection se necessário (padrão: `http://localhost:8080`; local: `http://localhost:5180`).
3. Execute **01 - Autenticação → Login (Gestor)** — o token JWT é salvo automaticamente na variável `{{token}}`.
4. Siga a ordem das pastas: criar equipamento → registrar leituras → ver alertas → comando de desligamento.

Para demonstrar o **403**, execute **Login (Operador)** e repita o comando de desligamento.

---

## Fluxo de demonstração sugerido (dashboard)

1. Acesse o dashboard e logue como `rodrigo.m`.
2. Cadastre um equipamento (ex.: TAG `TRN-01`, limite 30 kWh).
3. Clique **▸ Simular leitura** algumas vezes — consumos aleatórios entre 50% e 180% do limite; os que excedem geram alertas com a severidade calculada pela API.
4. Abra **Consumo** para ver o histórico paginado.
5. Clique **Desligar** — comando autenticado; o status muda para `Desligado` e o segundo clique seria bloqueado pela API (422).
6. **Resolver** os alertas na tabela.
7. Saia e logue como `operador.l2`: os botões **Desligar** ficam desabilitados (e a API retornaria 403 mesmo se forçado).

---

## Decisões técnicas relevantes

- **Paginação otimizada**: `PagedResult<T>` traduz para `OFFSET/FETCH` no SQL Server; `pageSize` limitado a 100. O índice composto `(EquipamentoId, DataHoraLeitura DESC)` casa com a ordenação da consulta de consumo — o banco resolve a página direto no índice, sem sort.
- **Alerta transacional**: leitura e alerta são persistidos no mesmo `SaveChangesAsync` — ou ambos entram, ou nenhum.
- **Snapshot no alerta**: `ConsumoRegistradoKwh` e `LimiteConfiguradoKwh` são gravados no momento do disparo; alterar o limite do equipamento depois não corrompe o histórico.
- **Exceções de domínio**: `RecursoNaoEncontradoException` → 404 e `RegraNegocioException` → 422, mapeadas pelo middleware global em `ProblemDetails` (RFC 7807), sem vazar stack trace.
- **Consultas somente-leitura** com `AsNoTracking` e projeção (`Select`) antes do `Skip/Take` — uma única query SQL retornando apenas as colunas necessárias.

---

*Projeto desenvolvido para FIAP — Desafio .NET Core 8 / Temas ESG (2026).*
