# ─── Stage 1: Base runtime ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ─── Stage 2: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia solo il .csproj per sfruttare la cache Docker dei layer npm/restore
COPY ["BlueHarbor/BlueHarbor.csproj", "BlueHarbor/"]
RUN dotnet restore "BlueHarbor/BlueHarbor.csproj"

# Copia il resto del sorgente
COPY . .

WORKDIR "/src/BlueHarbor"
RUN dotnet build "BlueHarbor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ─── Stage 3: Publish ─────────────────────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BlueHarbor.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ─── Stage 4: Final image ─────────────────────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlueHarbor.dll"]
