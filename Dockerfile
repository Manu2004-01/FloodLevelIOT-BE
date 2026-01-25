# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY HCM_Flood_Level/HCM_Flood_Level.sln ./

# Copy project files
COPY HCM_Flood_Level/Core/Core.csproj HCM_Flood_Level/Core/
COPY HCM_Flood_Level/Infrastructure/Infrastructure.csproj HCM_Flood_Level/Infrastructure/
COPY HCM_Flood_Level/WebAPI/WebAPI.csproj HCM_Flood_Level/WebAPI/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY HCM_Flood_Level/Core/ HCM_Flood_Level/Core/
COPY HCM_Flood_Level/Infrastructure/ HCM_Flood_Level/Infrastructure/
COPY HCM_Flood_Level/WebAPI/ HCM_Flood_Level/WebAPI/

# Build the application
WORKDIR /src/HCM_Flood_Level/WebAPI
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=publish /app/publish .

# Change ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "WebAPI.dll"]

