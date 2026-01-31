# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Threa.sln ./
COPY GameMechanics/GameMechanics.csproj GameMechanics/
COPY GameMechanics.Messaging.InMemory/GameMechanics.Messaging.InMemory.csproj GameMechanics.Messaging.InMemory/
COPY Threa.Dal/Threa.Dal.csproj Threa.Dal/
COPY Threa.Dal.SqlLite/Threa.Dal.Sqlite.csproj Threa.Dal.SqlLite/
COPY Threa.Admin/Threa.Admin.csproj Threa.Admin/
COPY Threa/Threa/Threa.csproj Threa/Threa/
COPY Threa/Threa.Client/Threa.Client.csproj Threa/Threa.Client/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish Threa/Threa/Threa.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create data directory for SQLite (writable by app user)
RUN mkdir -p /app/data && chown -R app:app /app/data

# Copy published app
COPY --from=build /app/publish .

# Switch to non-root user (built-in 'app' user in .NET images)
USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "Threa.dll"]
