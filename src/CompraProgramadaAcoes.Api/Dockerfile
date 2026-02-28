FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia solution e csproj primeiro (melhora cache)
COPY CompraProgramadaAcoes.sln ./

COPY src/CompraProgramadaAcoes.Domain/*.csproj src/CompraProgramadaAcoes.Domain/
COPY src/CompraProgramadaAcoes.Application/*.csproj src/CompraProgramadaAcoes.Application/
COPY src/CompraProgramadaAcoes.Infrastructure/*.csproj src/CompraProgramadaAcoes.Infrastructure/
COPY src/CompraProgramadaAcoes.Api/*.csproj src/CompraProgramadaAcoes.Api/

RUN dotnet restore


COPY . .

RUN dotnet publish src/CompraProgramadaAcoes.Api/CompraProgramadaAcoes.Api.csproj \
    -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CompraProgramadaAcoes.Api.dll"]