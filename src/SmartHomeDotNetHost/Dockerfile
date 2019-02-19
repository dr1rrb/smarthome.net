FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["SmartHomeDotNetHost/SmartHomeDotNetHost.csproj", "SmartHomeDotNetHost/"]"
RUN dotnet restore "SmartHomeDotNetHost/SmartHomeDotNetHost.csproj"
COPY . .
WORKDIR "/src/SmartHomeDotNetHost"
RUN dotnet build "SmartHomeDotNetHost.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SmartHomeDotNetHost.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SmartHomeDotNetHost.dll"]
