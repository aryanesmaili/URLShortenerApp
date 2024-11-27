# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY ./SharedDataModels ./SharedDataModels/
COPY ./URLShortenerBlazor/*.csproj ./URLShortenerBlazor/
RUN dotnet restore ./URLShortenerBlazor/*.csproj

# Copy source code and publish the application
COPY ./URLShortenerBlazor ./URLShortenerBlazor/
WORKDIR /src/URLShortenerBlazor/
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Serve with Nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy the published Blazor WebAssembly files to Nginx's HTML directory
COPY --from=build /app/publish/wwwroot ./

# Add custom Nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Ensure permissions are set correctly
RUN chmod -R 755 /usr/share/nginx/html

# Expose the default HTTP port
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]