FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/Orchestrator.fsproj .

RUN dotnet restore

COPY ./src .

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Orchestrator.dll"]
