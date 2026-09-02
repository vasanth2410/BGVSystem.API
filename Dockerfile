FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BGVSystem.API/BGVSystem.API.csproj", "BGVSystem.API/"]
COPY ["BGVSystem.Application/BGVSystem.Application.csproj", "BGVSystem.Application/"]
COPY ["BGVSystem.Domain/BGVSystem.Domain.csproj", "BGVSystem.Domain/"]
COPY ["BGVSystem.Infrastructure/BGVSystem.Infrastructure.csproj", "BGVSystem.Infrastructure/"]
COPY ["BGVSystem.Persistence/BGVSystem.Persistence.csproj", "BGVSystem.Persistence/"]
COPY ["BGVSystem.Shared/BGVSystem.Shared.csproj", "BGVSystem.Shared/"]
RUN dotnet restore "BGVSystem.API/BGVSystem.API.csproj"
COPY . .
WORKDIR "/src/BGVSystem.API"
RUN dotnet publish "BGVSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN apt-get update && apt-get install -y libfontconfig1 libfreetype6 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
EXPOSE 8080
ENTRYPOINT ["dotnet", "BGVSystem.API.dll"]
