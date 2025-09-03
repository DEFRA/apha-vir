# -------- Set base image version --------
ARG PARENT_VERSION=dotnet8.0

# ================================
# -------- Development Stage --------
# ================================
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
ARG PARENT_VERSION=dotnet8.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}
WORKDIR /home/dotnet/src

# Copy all source code into the image under /home/dotnet/src
COPY --chown=dotnet:dotnet src/. .

# Restore, build and publish the Web project
RUN dotnet restore ./Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj
RUN dotnet publish ./Apha.VIR/Apha.VIR.Web -c Release -o /home/dotnet/out /p:UseAppHost=false

# Remove write permissions (files: 444, dirs: 555)
RUN find . -type d -exec chmod 555 {} \; && \
    find . -type f -exec chmod 444 {} \;

# ================================
# -------- Production Stage --------
# ================================
ARG PARENT_VERSION=dotnet8.0
FROM defradigital/dotnetcore:$PARENT_VERSION AS production
ARG PARENT_VERSION=dotnet8.0

LABEL uk.gov.defra.parent-image=defra-dotnetcore:${PARENT_VERSION}

USER 0
RUN apk update && apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ARG PORT=8080
EXPOSE ${PORT}

USER app
WORKDIR /app
COPY --from=development /home/dotnet/out/ ./

ENTRYPOINT ["dotnet", "Apha.VIR.Web.dll"]
