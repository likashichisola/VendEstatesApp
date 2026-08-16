FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Render provides the PORT environment variable at runtime; default to 8080 for local `docker run`.
ENV PORT=8080
EXPOSE 8080
# Render's sandboxed containers have a very low inotify instance limit, which crashes the
# host during WebApplication.CreateBuilder() when it tries to watch appsettings*.json for
# changes (System.IO.IOException: configured user limit (128) on inotify instances reached).
# This env var is read by the generic host BEFORE any user code runs, disabling that watcher.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VendEstatesApp/VendEstatesApp.csproj", "VendEstatesApp/"]
RUN dotnet restore "VendEstatesApp/VendEstatesApp.csproj"
COPY . .
WORKDIR "/src/VendEstatesApp"
RUN dotnet build "VendEstatesApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VendEstatesApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Use shell form so ${PORT:-8080} is expanded at container start, allowing Render to
# inject a dynamic port while still working with `docker run` locally (defaults to 8080).
ENTRYPOINT ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet VendEstatesApp.dll
