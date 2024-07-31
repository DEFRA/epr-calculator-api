FROM defradigital/dotnetcore-development:dotnet8.0 AS base
 
USER root
ARG PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}
 
RUN apk update && apk --no-cache add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
 
FROM defradigital/dotnetcore-development:dotnet8.0 AS build
USER root
 
# Install dotnet-ef tool
RUN dotnet tool install dotnet-ef --version 8.* --tool-path "/root/.dotnet/tools"
 
# Add the tools folder to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"
 
WORKDIR /src
COPY ["EPR.Calculator.API/EPR.Calculator.API.csproj", "EPR.Calculator.API/"]
COPY ["EPR.Calculator.API.Data/EPR.Calculator.API.Data.csproj", "EPR.Calculator.API.Data/"]

RUN dotnet restore "./EPR.Calculator.API/EPR.Calculator.API.csproj"

COPY --chown=dotnet:dotnet EPR.Calculator.API/. ./EPR.Calculator.API/.
COPY --chown=dotnet:dotnet EPR.Calculator.API.Data/. ./EPR.Calculator.API.Data/.

WORKDIR "/src/EPR.Calculator.API"
RUN dotnet build "./EPR.Calculator.API.csproj" -c Release /p:AzureBuild=true -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EPR.Calculator.API.csproj" -c Release /p:AzureBuild=false -o /app/publish

FROM base AS final
USER dotnet
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EPR.Calculator.API.dll"]