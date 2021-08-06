#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Application.ChatApi/Application.ChatApi.csproj", "src/Application.ChatApi/"]
RUN dotnet restore "src/Application.ChatApi/Application.ChatApi.csproj"
COPY . .
WORKDIR "/src/src/Application.ChatApi"
RUN dotnet build "Application.ChatApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Application.ChatApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Application.ChatApi.dll"]