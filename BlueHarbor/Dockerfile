FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8082
EXPOSE 8082

# Build frontend
FROM node:20 AS frontend-build
WORKDIR /src/frontend
COPY ["BlueHarbor/wwwroot/frontend/package*.json", "./"]
RUN npm install
COPY BlueHarbor/wwwroot/frontend/ .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BlueHarbor/BlueHarbor.csproj", "BlueHarbor/"]
RUN dotnet restore "BlueHarbor/BlueHarbor.csproj"
COPY . .
# Copy frontend build results
COPY --from=frontend-build /src/frontend/dist ./BlueHarbor/wwwroot/
# Remove source frontend from wwwroot in the final build to keep it clean if desired, 
# but the user asked to implement it in wwwroot, so maybe they want it there.
# However, for the ASP.NET build, we only need the dist contents in the root of wwwroot.
WORKDIR "/src/BlueHarbor"
RUN dotnet build "./BlueHarbor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlueHarbor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlueHarbor.dll"]
