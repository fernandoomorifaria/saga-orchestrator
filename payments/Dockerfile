FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/Payments.fsproj .

RUN dotnet restore

COPY ./src .

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble

WORKDIR /app

RUN apt update && apt -y upgrade libkrb5-3 libgssapi-krb5-2

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Payments.dll"]
