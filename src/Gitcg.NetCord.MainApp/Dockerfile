FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/Gitcg.NetCord.MainApp/Gitcg.NetCord.MainApp.csproj", "src/Gitcg.NetCord.MainApp/"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "src/ServiceDefaults/"]
RUN dotnet restore "src/Gitcg.NetCord.MainApp/Gitcg.NetCord.MainApp.csproj"
COPY . .
WORKDIR "/src/src/Gitcg.NetCord.MainApp"
RUN dotnet build "./Gitcg.NetCord.MainApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gitcg.NetCord.MainApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gitcg.NetCord.MainApp.dll"]
