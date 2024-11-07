FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

COPY . .
WORKDIR /repo/src/Numerous.Web
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Numerous.Web.dll"]
