# -------- Base runtime image --------
# Allow parent image version to be set at build time
ARG PARENT_VERSION=dotnet8.0


FROM defradigital/dotnetcore-development:$PARENT_VERSION AS base
WORKDIR /app
EXPOSE 8080
USER app

# -------- Build image with SDK --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


# Copy application source files
COPY src/. .

# Restore dependencies for application
RUN dotnet restore Apha.VIR.sln



# Build
RUN dotnet build Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj \
    -c "$BUILD_CONFIGURATION" -o /app/build



# -------- Publish stage --------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj \
    -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# -------- Final runtime image --------
FROM base AS final

# Redefine work directory 
WORKDIR /app

# Copy published output from the publish stage
COPY --from=publish /app/publish .

# Explicitly specify user again (even though base already has it)
USER app

# Define entry point
ENTRYPOINT ["dotnet", "Apha.VIR.Web.dll"]
