# ===== Estágio de build: SDK completo para restaurar e publicar =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro: o restore fica cacheado entre builds
# enquanto o código-fonte muda.
# Restaura só a API: a imagem de produção não depende dos projetos de teste
# (os testes de aceitação têm imagem própria — ver Dockerfile.tests).
COPY IndustIoT.Api/IndustIoT.Api.csproj IndustIoT.Api/
RUN dotnet restore IndustIoT.Api/IndustIoT.Api.csproj

COPY . .
RUN dotnet publish IndustIoT.Api/IndustIoT.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

# ===== Estágio final: runtime ASP.NET enxuto, sem SDK =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "IndustIoT.Api.dll"]
