# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy source code and publish the application
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Serve with Nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy the published Blazor WebAssembly files to Nginx's HTML directory
COPY --from=build /app/publish/wwwroot ./

# Optional: Add custom Nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Ensure permissions are set correctly
RUN chmod -R 755 /usr/share/nginx/html

# Expose the default HTTP port
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]