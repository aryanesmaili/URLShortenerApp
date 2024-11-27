# Use the official .NET Core SDK as a parent image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore any dependencies (use .csproj for the project name)
COPY ./SharedDataModels ./SharedDataModels/
COPY ./URLShortenerAPI/*.csproj ./URLShortenerAPI/
RUN dotnet restore ./URLShortenerAPI/*.csproj

# Copy the rest of the application code and Publish the application
COPY ./URLShortenerAPI ./URLShortenerAPI/
WORKDIR /app/URLShortenerAPI/
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app/URLShortenerAPI/
COPY --from=build /app/URLShortenerAPI/out ./

# Set the environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production
# set this for better container performance
ENV ASPNETCORE_URLS=http://0.0.0.0:5261

# Expose the port your application will run on
EXPOSE 5261

# Start the application
ENTRYPOINT ["dotnet", "URLShortenerAPI.dll"]