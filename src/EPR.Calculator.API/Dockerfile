FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EPR.Calculator.API/EPR.Calculator.API.csproj", "EPR.Calculator.API/"]
RUN dotnet restore "./EPR.Calculator.API/EPR.Calculator.API.csproj"
COPY . .
WORKDIR "/src/EPR.Calculator.API"
RUN dotnet build "./EPR.Calculator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EPR.Calculator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EPR.Calculator.API.dll"]
