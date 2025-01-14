FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./LogServer.csproj", "LogServer/"]
RUN dotnet restore "LogServer/LogServer.csproj"
WORKDIR /src/LogServer
COPY . .
RUN dotnet build "LogServer.csproj" -o /app/build

FROM build AS publish
RUN dotnet publish "LogServer.csproj" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogServer.dll"]
