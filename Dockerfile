FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /repo

COPY . .
WORKDIR /repo/src/Numerous.Web
RUN dotnet restore -v detailed
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Numerous.Web.dll"]
