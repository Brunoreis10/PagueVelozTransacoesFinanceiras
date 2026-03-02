FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto (melhora cache do Docker)
COPY ["src/Backend/TransacoesFinanceiras.API/TransacoesFinanceiras.API.csproj", "src/Backend/TransacoesFinanceiras.API/"]
COPY ["src/Backend/TransacoesFinanceiras.Application/TransacoesFinanceiras.Application.csproj", "src/Backend/TransacoesFinanceiras.Application/"]
COPY ["src/Backend/TransacoesFinanceiras.Domain/TransacoesFinanceiras.Domain.csproj", "src/Backend/TransacoesFinanceiras.Domain/"]
COPY ["src/Backend/TransacoesFinanceiras.Infrastructure/TransacoesFinanceiras.Infrastructure.csproj", "src/Backend/TransacoesFinanceiras.Infrastructure/"]
COPY ["src/Shared/TransacoesFinanceiras.Exceptions/TransacoesFinanceiras.Exceptions.csproj", "src/Shared/TransacoesFinanceiras.Exceptions/"]

# Restore
RUN dotnet restore "src/Backend/TransacoesFinanceiras.API/TransacoesFinanceiras.API.csproj"

# Copiar todo o código
COPY . .

# Build
WORKDIR "/src/src/Backend/TransacoesFinanceiras.API"
RUN dotnet build "TransacoesFinanceiras.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
WORKDIR "/src/src/Backend/TransacoesFinanceiras.API"
RUN dotnet publish "TransacoesFinanceiras.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Diretório para logs
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "TransacoesFinanceiras.API.dll"]