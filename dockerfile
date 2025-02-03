# .NET 8.0 SDK'yı kullanarak proje inşa et
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Proje dosyasını kopyala ve restore işlemi yap
COPY . ./
RUN dotnet restore

# Projeyi build et
RUN dotnet publish -c Release -o out

# Çalıştırılacak son aşama
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "SpotifyFollowerTracker.dll"]
