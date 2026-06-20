FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY File/File.csproj File/
COPY Application/Application.csproj Application/
COPY Persistence/Persistence.csproj Persistence/
COPY Entities/Entities.csproj Entities/
COPY Resource/Resource.csproj Resource/
COPY Utility/Utility.csproj Utility/

RUN --mount=type=cache,id=pastil-file-nuget,target=/root/.nuget/packages \
    dotnet restore File/File.csproj \
    --disable-parallel \
    --verbosity minimal

COPY . .

RUN --mount=type=cache,id=pastil-file-nuget,target=/root/.nuget/packages \
    dotnet publish File/File.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "File.dll"]