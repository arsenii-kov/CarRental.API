# 1 этап: Сборка и публикация
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем csproj файлы и восстанавливаем зависимости (для кэширования слоев)
COPY ["CarRental.API/CarRental.API.csproj", "CarRental.API/"]
COPY ["CarRental.Tests/CarRental.Tests.csproj", "CarRental.Tests/"]
RUN dotnet restore "CarRental.API/CarRental.API.csproj"

# Копируем весь исходный код и собираем
COPY . .
WORKDIR "/src/CarRental.API"
RUN dotnet publish "CarRental.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2 этап: Легковесный Runtime образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CarRental.API.dll"]