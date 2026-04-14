FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# Copy everything from host Siffrum.Ecom
COPY . .

# Work inside Foundation folder
WORKDIR /app/Siffrum.Ecom.Foundation

# Restore and publish
RUN dotnet restore Siffrum.Ecom.Foundation.sln
RUN dotnet publish Siffrum.Ecom.Foundation.sln -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Siffrum.Ecom.Foundation.dll"]

