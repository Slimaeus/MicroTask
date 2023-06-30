#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MicroTask.Services.Tasks.Api/MicroTask.Services.Tasks.Api.csproj", "MicroTask.Services.Tasks.Api/"]
COPY ["MicroTask.Services.Tasks.Domain/MicroTask.Services.Tasks.Domain.csproj", "MicroTask.Services.Tasks.Domain/"]
RUN dotnet restore "MicroTask.Services.Tasks.Api/MicroTask.Services.Tasks.Api.csproj"
COPY . .
WORKDIR "/src/MicroTask.Services.Tasks.Api"
RUN dotnet build "MicroTask.Services.Tasks.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroTask.Services.Tasks.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroTask.Services.Tasks.Api.dll"]