FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MonitorAgent/MonitorAgent.csproj MonitorAgent/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore MonitorAgent/MonitorAgent.csproj

COPY MonitorAgent/ MonitorAgent/
RUN dotnet publish MonitorAgent/MonitorAgent.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MonitorAgent.dll"]
