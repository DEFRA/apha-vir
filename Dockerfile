# -------- Base runtime image --------
FROM defradigital/dotnetcore-development AS base
WORKDIR /app
EXPOSE 8080
USER app

# -------- Build image with SDK --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files for caching
# COPY src/Apha.VIR.sln ./
# COPY src/Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj Apha.VIR/Apha.VIR.Web/
# COPY src/Apha.VIR/Apha.VIR.Core/Apha.VIR.Core.csproj Apha.VIR/Apha.VIR.Core/
# COPY src/Apha.VIR/Apha.VIR.Application/Apha.VIR.Application.csproj Apha.VIR/Apha.VIR.Application/
# COPY src/Apha.VIR/Apha.VIR.DataAccess/Apha.VIR.DataAccess.csproj Apha.VIR/Apha.VIR.DataAccess/
# (skip UnitTests for now unless you're testing in Docker)

# Copy full source
COPY src/. .

# Restore dependencies
RUN dotnet restore Apha.VIR.sln



# Build
RUN dotnet build Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj \
    -c $BUILD_CONFIGURATION -o /app/build



# -------- Publish stage --------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------- Final runtime image --------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Apha.VIR.Web.dll"]
