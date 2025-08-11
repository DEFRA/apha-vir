# -------- Set base image version --------
ARG PARENT_VERSION=latest

# ================================
# -------- Development Stage --------
# ================================
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development

LABEL uk.gov.defra.parent-image=defra-dotnetcore-development:${PARENT_VERSION}
WORKDIR /home/dotnet/src

# Copy all source code into the image under /home/dotnet/src
COPY --chown=dotnet:dotnet src/. .

# Restore, build and publish the Web project
RUN dotnet restore ./Apha.VIR/Apha.VIR.Web/Apha.VIR.Web.csproj
RUN dotnet publish ./Apha.VIR/Apha.VIR.Web -c Release -o /home/dotnet/out /p:UseAppHost=false

ARG PORT=8080
ENV PORT=${PORT}
EXPOSE ${PORT}

# ================================
# -------- Production Stage --------
# ================================
FROM defradigital/dotnetcore:$PARENT_VERSION AS production

LABEL uk.gov.defra.parent-image=defra-dotnetcore:${PARENT_VERSION}

ARG PORT=8080
EXPOSE ${PORT}

USER dotnet
WORKDIR /home/dotnet/app
COPY --from=development /home/dotnet/out/ ./

ENTRYPOINT ["dotnet", "Apha.VIR.Web.dll"]
