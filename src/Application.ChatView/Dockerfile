#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Application.ChatView/Application.ChatView.csproj", "src/Application.ChatView/"]
RUN dotnet restore "src/Application.ChatView/Application.ChatView.csproj"
COPY . .
WORKDIR "/src/src/Application.ChatView"
RUN dotnet build "Application.ChatView.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Application.ChatView.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Application.ChatView.dll"]