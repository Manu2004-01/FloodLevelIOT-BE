# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY HCM_Flood_Level/HCM_Flood_Level.sln HCM_Flood_Level/

# Copy project files first for better layer caching
COPY HCM_Flood_Level/Core/Core.csproj HCM_Flood_Level/Core/
COPY HCM_Flood_Level/Infrastructure/Infrastructure.csproj HCM_Flood_Level/Infrastructure/
COPY HCM_Flood_Level/WebAPI/WebAPI.csproj HCM_Flood_Level/WebAPI/

# Restore dependencies
WORKDIR /src/HCM_Flood_Level
RUN dotnet restore

# Copy all source files recursively (including all .cs files)
COPY HCM_Flood_Level/Core/ HCM_Flood_Level/Core/
COPY HCM_Flood_Level/Infrastructure/ HCM_Flood_Level/Infrastructure/

# Copy WebAPI files - explicitly copy Program.cs first
COPY HCM_Flood_Level/WebAPI/Program.cs HCM_Flood_Level/WebAPI/Program.cs
COPY HCM_Flood_Level/WebAPI/*.cs HCM_Flood_Level/WebAPI/ 2>/dev/null || true
COPY HCM_Flood_Level/WebAPI/Controllers/ HCM_Flood_Level/WebAPI/Controllers/
COPY HCM_Flood_Level/WebAPI/Extensions/ HCM_Flood_Level/WebAPI/Extensions/
COPY HCM_Flood_Level/WebAPI/Errors/ HCM_Flood_Level/WebAPI/Errors/
COPY HCM_Flood_Level/WebAPI/Helpers/ HCM_Flood_Level/WebAPI/Helpers/
COPY HCM_Flood_Level/WebAPI/Middleware/ HCM_Flood_Level/WebAPI/Middleware/
COPY HCM_Flood_Level/WebAPI/Models/ HCM_Flood_Level/WebAPI/Models/
COPY HCM_Flood_Level/WebAPI/Properties/ HCM_Flood_Level/WebAPI/Properties/
COPY HCM_Flood_Level/WebAPI/wwwroot/ HCM_Flood_Level/WebAPI/wwwroot/
COPY HCM_Flood_Level/WebAPI/*.json HCM_Flood_Level/WebAPI/
COPY HCM_Flood_Level/WebAPI/*.http HCM_Flood_Level/WebAPI/ 2>/dev/null || true

# Verify Program.cs exists and list all files in WebAPI directory
RUN ls -la /src/HCM_Flood_Level/WebAPI/ | head -20
RUN test -f /src/HCM_Flood_Level/WebAPI/Program.cs && echo "Program.cs found!" || echo "Program.cs NOT found!"

# Build the application (build without output to verify it compiles)
WORKDIR /src/HCM_Flood_Level
RUN dotnet build HCM_Flood_Level.sln -c Release --no-incremental

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/HCM_Flood_Level
RUN dotnet publish WebAPI/WebAPI.csproj -c Release -o /app/publish

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

