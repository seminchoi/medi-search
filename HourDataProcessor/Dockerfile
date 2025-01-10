FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HourDataProcessor/HourDataProcessor.csproj", "HourDataProcessor/"]
RUN dotnet restore "HourDataProcessor/HourDataProcessor.csproj"
COPY . .
WORKDIR "/src/HourDataProcessor"
RUN dotnet build "HourDataProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HourDataProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HourDataProcessor.dll"]
