FROM defradigital/dotnetcore:dotnet8.0 AS base
USER root
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

RUN apk update && apk --no-cache add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

FROM defradigital/dotnetcore-development:dotnet8.0 AS build
USER root
WORKDIR /src
COPY ["EPR.Calculator.API/EPR.Calculator.API.csproj", "EPR.Calculator.API/"]
RUN dotnet restore "./EPR.Calculator.API/EPR.Calculator.API.csproj"
COPY . .
WORKDIR "/src/EPR.Calculator.API"
RUN dotnet build "EPR.Calculator.API.csproj" -c Release /p:AzureBuild=true -o /app/build

FROM build AS publish
RUN dotnet publish "EPR.Calculator.API.csproj" -c Release /p:AzureBuild=true -o /app/publish

RUN dotnet tool install --global dotnet-ef --version 8.0.7
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef database update

FROM base AS final

# Switch to the non-root user
USER dotnet

WORKDIR /app
COPY --from=publish /app/publish .
USER dotnet
ENTRYPOINT ["dotnet", "EPR.Calculator.API.dll"]
