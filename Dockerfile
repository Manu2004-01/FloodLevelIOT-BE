# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/HCM_Flood_Level

# Copy entire solution source first to avoid missing files due to caching
COPY HCM_Flood_Level/ .

# Restore dependencies
RUN dotnet restore

# Optional: list WebAPI contents and verify Program.cs exists
RUN ls -la /src/HCM_Flood_Level/WebAPI/ | head -50
RUN test -f /src/HCM_Flood_Level/WebAPI/Program.cs && echo "Program.cs found!" || (echo "Program.cs NOT found!" && ls -la /src/HCM_Flood_Level/WebAPI/)

# Build only the WebAPI project to avoid solution-level surprises
RUN dotnet build WebAPI/WebAPI.csproj -c Release --no-incremental

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/HCM_Flood_Level
RUN dotnet publish WebAPI/WebAPI.csproj -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user (optional but recommended)
RUN addgroup --system appuser && adduser --system --ingroup appuser appuser || true

# Copy published files
COPY --from=publish /app/publish .

# Change ownership
RUN chown -R appuser:appuser /app || true

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

