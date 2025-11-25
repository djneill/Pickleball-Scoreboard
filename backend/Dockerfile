# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["PickleballApi/PickleballApi.csproj", "PickleballApi/"]
RUN dotnet restore "PickleballApi/PickleballApi.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/PickleballApi"

# Build the application
RUN dotnet build "PickleballApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PickleballApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PickleballApi.dll"]
