#!/usr/bin/env bash
# Entrypoint do container de testes: executa os cenários BDD contra $API_BASE_URL
# e publica os relatórios em /test-results (volume mapeado para ./TestResults/docker).
set -uo pipefail

RESULTADOS=/test-results
mkdir -p "$RESULTADOS"

echo "==> Testes BDD contra ${API_BASE_URL}"
dotnet test /src/IndustIoT.AcceptanceTests/IndustIoT.AcceptanceTests.csproj \
  -c Release --no-build \
  --results-directory "$RESULTADOS" \
  --logger "trx;LogFileName=resultado-bdd.trx" \
  --logger "junit;LogFilePath=$RESULTADOS/junit-bdd.xml" \
  --logger "console;verbosity=normal"
CODIGO=$?

# Relatório HTML do Reqnroll (Cucumber HTML formatter) gerado na pasta de saída do build
cp /src/IndustIoT.AcceptanceTests/bin/Release/net8.0/TestResults/relatorio-bdd.* "$RESULTADOS"/ 2>/dev/null || true

echo "==> Relatórios em $RESULTADOS (código de saída: $CODIGO)"
exit $CODIGO
