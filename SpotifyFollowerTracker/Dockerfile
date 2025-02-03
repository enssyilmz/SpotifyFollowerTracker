# Temel imajý al
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Proje dosyasýný kopyala
COPY . .

# Baðýmlýlýklarý yükle
RUN dotnet restore /app/SpotifyFollowerTracker.sln

# Uygulamayý derle
RUN dotnet build /app/SpotifyFollowerTracker.sln -c Release -o /app/build

# Çalýþtýrma aþamasý
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=base /app/build .
ENTRYPOINT ["dotnet", "SpotifyFollowerTracker.dll"]
