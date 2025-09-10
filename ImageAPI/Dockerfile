# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj & restore (tối ưu cache)
COPY ["ImageAPI.csproj", "ImageAPI/"]
RUN dotnet restore "ImageAPI.csproj"

# Copy source còn lại và build
COPY . .
WORKDIR "/src/ImageAPI"
RUN dotnet build "ImageAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ImageAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
# Copy app published
COPY --from=publish /app/publish .
# Copy entrypoint to set ASPNETCORE_URLS with $PORT
COPY entrypoint.sh ./entrypoint.sh
# set execution permission to run script
RUN chmod +x /app/entrypoint.sh
# Execute with non-root user 
USER app
ENTRYPOINT ["./entrypoint.sh"]