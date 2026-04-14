# ── Stage 1: Build ──
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY Siffrum.Ecom/Siffrum.Ecom.Foundation/Siffrum.Ecom.Foundation.csproj Siffrum.Ecom/Siffrum.Ecom.Foundation/
COPY Siffrum.Ecom/Components/Siffrum.Ecom.BAL/Siffrum.Ecom.BAL.csproj Siffrum.Ecom/Components/Siffrum.Ecom.BAL/
COPY Siffrum.Ecom/Components/Siffrum.Ecom.Config/Siffrum.Ecom.Config.csproj Siffrum.Ecom/Components/Siffrum.Ecom.Config/
COPY Siffrum.Ecom/Components/Siffrum.Ecom.DAL/Siffrum.Ecom.DAL.csproj Siffrum.Ecom/Components/Siffrum.Ecom.DAL/
COPY Siffrum.Ecom/Components/Siffrum.Ecom.DomainModels/Siffrum.Ecom.DomainModels.csproj Siffrum.Ecom/Components/Siffrum.Ecom.DomainModels/
COPY Siffrum.Ecom/Components/Siffrum.Ecom.ServiceModels/Siffrum.Ecom.ServiceModels.csproj Siffrum.Ecom/Components/Siffrum.Ecom.ServiceModels/

RUN dotnet restore Siffrum.Ecom/Siffrum.Ecom.Foundation/Siffrum.Ecom.Foundation.csproj

# Copy everything and build
COPY . .
RUN dotnet publish Siffrum.Ecom/Siffrum.Ecom.Foundation/Siffrum.Ecom.Foundation.csproj \
    -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ──
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Copy the template as fallback config.
# On the server, either:
#   (a) Mount your real appsettings.json via docker volume/copy, OR
#   (b) Pass secrets as environment variables (they override the template)
COPY Siffrum.Ecom/Siffrum.Ecom.Foundation/appsettings.Template.json /app/appsettings.json

# Create directory for uploaded content
RUN mkdir -p /app/wwwroot/content

ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5050

ENTRYPOINT ["dotnet", "Siffrum.Ecom.Foundation.dll"]
