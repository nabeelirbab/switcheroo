FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY API/*.csproj ./API/
RUN dotnet restore switcheroo-server.sln

# Copy everything else and build
COPY . ./
RUN dotnet publish switcheroo-server.sln -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
 
# Copy static content
COPY ./API/Assets/ /app/Assets/

ENTRYPOINT ["dotnet", "API.dll"]