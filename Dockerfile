# Base image for running the app
FROM defradigital/dotnetcore-development AS base
WORKDIR /app
EXPOSE 8080
USER app

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy the full solution content (includes web app and class libraries)
COPY . .

# Restore the web app (will pull in class library references too)
RUN dotnet restore "Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj"

# Build the app
RUN dotnet build "Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "Apha.VIR.Web.dll"]
