FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY . /src
WORKDIR "/src/SmartHomeDotNetHost"
RUN dotnet build "SmartHomeDotNetHost.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SmartHomeDotNetHost.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SmartHomeDotNetHost.dll"]
