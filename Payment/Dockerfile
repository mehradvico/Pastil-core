FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Payment/Payment.csproj Payment/
COPY Application/Application.csproj Application/
COPY Persistence/Persistence.csproj Persistence/
COPY Entities/Entities.csproj Entities/
COPY Resource/Resource.csproj Resource/
COPY Utility/Utility.csproj Utility/

RUN --mount=type=cache,id=pastil-payment-nuget,target=/root/.nuget/packages \
    dotnet restore Payment/Payment.csproj \
    --disable-parallel \
    --verbosity minimal

COPY . .

RUN --mount=type=cache,id=pastil-payment-nuget,target=/root/.nuget/packages \
    dotnet publish Payment/Payment.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Payment.dll"]