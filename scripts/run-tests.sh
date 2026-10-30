#!/usr/bin/env bash
# Executa os testes de aceitação BDD da API IndústIoT.
#   ./scripts/run-tests.sh                 -> modo InProcess (só precisa do .NET 8 SDK)
#   ./scripts/run-tests.sh docker          -> SQL Server + API + testes via docker compose
#   ./scripts/run-tests.sh externo URL     -> contra uma API já no ar (ex.: http://localhost:8080)
set -uo pipefail

MODO="${1:-inprocess}"
BASE_URL="${2:-http://localhost:8080}"
RAIZ="$(cd "$(dirname "$0")/.." && pwd)"
cd "$RAIZ"
RESULTADOS="$RAIZ/TestResults/$MODO"
mkdir -p "$RESULTADOS"

rodar_testes() {
  dotnet test IndustIoT.AcceptanceTests/IndustIoT.AcceptanceTests.csproj \
    --results-directory "$RESULTADOS" \
    --logger "trx;LogFileName=resultado-bdd.trx" \
    --logger "junit;LogFilePath=$RESULTADOS/junit-bdd.xml" \
    --logger "console;verbosity=normal"
  local codigo=$?
  cp IndustIoT.AcceptanceTests/bin/Debug/net8.0/TestResults/relatorio-bdd.* "$RESULTADOS"/ 2>/dev/null || true
  return $codigo
}

case "$MODO" in
  inprocess) unset API_BASE_URL; rodar_testes; CODIGO=$? ;;
  externo)   API_BASE_URL="$BASE_URL" rodar_testes; CODIGO=$? ;;
  docker)
    docker compose -f docker-compose.yml -f docker-compose.tests.yml up --build \
      --abort-on-container-exit --exit-code-from acceptance-tests
    CODIGO=$?
    docker compose -f docker-compose.yml -f docker-compose.tests.yml down -v
    ;;
  *) echo "Modo inválido: $MODO (use inprocess | docker | externo)"; exit 2 ;;
esac

echo
echo "Relatórios em: $RESULTADOS  (relatorio-bdd.html, resultado-bdd.trx, junit-bdd.xml)"
exit $CODIGO
