FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["mindlog_api.csproj", "./"]
RUN dotnet restore "mindlog_api.csproj"

COPY . .
RUN dotnet publish "mindlog_api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "mindlog_api.dll"]