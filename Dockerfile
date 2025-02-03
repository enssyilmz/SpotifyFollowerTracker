FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SpotifyFollowerTracker/SpotifyFollowerTracker.csproj", "SpotifyFollowerTracker/"]
RUN dotnet restore "SpotifyFollowerTracker/SpotifyFollowerTracker.csproj"
COPY . .
WORKDIR "/src/SpotifyFollowerTracker"
RUN dotnet build "SpotifyFollowerTracker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpotifyFollowerTracker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpotifyFollowerTracker.dll"]
