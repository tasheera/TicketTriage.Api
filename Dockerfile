# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY TicketTriage.Api.csproj .
RUN dotnet restore

# Copy source code and publish (publish implicitly builds)
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TicketTriage.Api.dll"]