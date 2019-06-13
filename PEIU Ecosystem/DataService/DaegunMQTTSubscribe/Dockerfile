FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DataService/DaegunMQTTSubscribe/DaegunMQTTSubscribe.csproj", "DataService/DaegunMQTTSubscribe/"]
COPY ["../PEIUModels/PEIUModels.csproj", "../PEIUModels/"]
COPY ["PEIUToolkit.Core/PEIUToolkit.Core.csproj", "PEIUToolkit.Core/"]
RUN dotnet restore "DataService/DaegunMQTTSubscribe/DaegunMQTTSubscribe.csproj"
COPY . .
WORKDIR "/src/DataService/DaegunMQTTSubscribe"
RUN dotnet build "DaegunMQTTSubscribe.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DaegunMQTTSubscribe.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DaegunMQTTSubscribe.dll"]