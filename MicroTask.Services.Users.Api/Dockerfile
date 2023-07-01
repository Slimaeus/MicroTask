#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MicroTask.Services.Users.Api/MicroTask.Services.Users.Api.csproj", "MicroTask.Services.Users.Api/"]
COPY ["MicroTask.Serivces.Users.Infrastructure/MicroTask.Services.Users.Infrastructure.csproj", "MicroTask.Serivces.Users.Infrastructure/"]
COPY ["MicroTask.Services.Users.Domain/MicroTask.Services.Users.Domain.csproj", "MicroTask.Services.Users.Domain/"]
RUN dotnet restore "MicroTask.Services.Users.Api/MicroTask.Services.Users.Api.csproj"
COPY . .
WORKDIR "/src/MicroTask.Services.Users.Api"
RUN dotnet build "MicroTask.Services.Users.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroTask.Services.Users.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroTask.Services.Users.Api.dll"]