# Stage 1: Build application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file
COPY ["TaskFlow.Api.csproj", "./"]


# Restore dependencies
RUN dotnet restore "TaskFlow.Api.csproj"


# Copy remaining source code
COPY . .


# Publish application
RUN dotnet publish "TaskFlow.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false
# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Listen on HTTP port 8080
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Copy published application
COPY --from=build /app/publish .

# Run application
ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]