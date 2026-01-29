FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY File/File.csproj File/
RUN dotnet restore File/File.csproj

COPY . .
RUN dotnet publish File/File.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8081
EXPOSE 8081

ENTRYPOINT ["dotnet", "File.dll"]