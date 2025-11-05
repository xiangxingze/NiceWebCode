FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/NiceWebCode.WebApi/NiceWebCode.WebApi.csproj", "src/NiceWebCode.WebApi/"]
COPY ["src/NiceWebCode.Infrastructure/NiceWebCode.Infrastructure.csproj", "src/NiceWebCode.Infrastructure/"]
COPY ["src/NiceWebCode.Application/NiceWebCode.Application.csproj", "src/NiceWebCode.Application/"]
COPY ["src/NiceWebCode.Domain/NiceWebCode.Domain.csproj", "src/NiceWebCode.Domain/"]
RUN dotnet restore "src/NiceWebCode.WebApi/NiceWebCode.WebApi.csproj"
COPY . .
WORKDIR "/src/src/NiceWebCode.WebApi"
RUN dotnet build "NiceWebCode.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NiceWebCode.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NiceWebCode.WebApi.dll"]
