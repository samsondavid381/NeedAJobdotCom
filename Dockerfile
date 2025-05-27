FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY *.sln ./
COPY NeedAJobdotCom.Api/*.csproj ./NeedAJobdotCom.Api/
COPY NeedAJobdotCom.Core/*.csproj ./NeedAJobdotCom.Core/
COPY NeedAJobdotCom.Infrastructure/*.csproj ./NeedAJobdotCom.Infrastructure/

# Restore packages
RUN dotnet restore

# Copy source code
COPY . ./

# Build and publish
RUN dotnet publish NeedAJobdotCom.Api -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

# Render uses PORT environment variable
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

ENTRYPOINT ["dotnet", "NeedAJobdotCom.Api.dll"]