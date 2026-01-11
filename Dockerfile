# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TestNest.Admin.API/TestNest.Admin.API.csproj", "TestNest.Admin.API/"]
COPY ["TestNest.Admin.Application/TestNest.Admin.Application.csproj", "TestNest.Admin.Application/"]
COPY ["TestNest.Admin.Domain/TestNest.Admin.Domain.csproj", "TestNest.Admin.Domain/"]
COPY ["TestNest.Admin.Infrastructure/TestNest.Admin.Infrastructure.csproj", "TestNest.Admin.Infrastructure/"]
COPY ["TestNest.Admin.SharedLibrary/TestNest.Admin.SharedLibrary.csproj", "TestNest.Admin.SharedLibrary/"]

# Restore dependencies
RUN dotnet restore "TestNest.Admin.API/TestNest.Admin.API.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/TestNest.Admin.API"
RUN dotnet build "TestNest.Admin.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "TestNest.Admin.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "TestNest.Admin.API.dll"]
