FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DaegunHub/DaegunHub.csproj", "DaegunHub/"]
RUN dotnet restore "DaegunHub/DaegunHub.csproj"
COPY . .
WORKDIR "/src/DaegunHub"
RUN dotnet build "DaegunHub.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DaegunHub.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DaegunHub.dll"]