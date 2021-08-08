#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Application.AgentCoodinatorService/Application.AgentCoodinatorService.csproj", "src/Application.AgentCoodinatorService/"]
RUN dotnet restore "src/Application.AgentCoodinatorService/Application.AgentCoodinatorService.csproj"
COPY . .
WORKDIR "/src/src/Application.AgentCoodinatorService"
RUN dotnet build "Application.AgentCoodinatorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Application.AgentCoodinatorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Application.AgentCoodinatorService.dll"]