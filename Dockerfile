# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY src/App/App.csproj src/App/
COPY src/Common/Common.csproj src/Common/
COPY src/Modules/Access/Access.csproj src/Modules/Access/
COPY src/Modules/Files/Files.csproj src/Modules/Files/
COPY src/Modules/Users/Users.csproj src/Modules/Users/

# Restore dependencies
RUN dotnet restore src/App/App.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/src/App
RUN dotnet build App.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish App.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Run the API without root privileges.
RUN mkdir -p /app/uploads/files && chown -R $APP_UID /app
USER $APP_UID

ENTRYPOINT ["dotnet", "App.dll"]

