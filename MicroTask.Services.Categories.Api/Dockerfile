#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MicroTask.Services.Categories.Api/MicroTask.Services.Categories.Api.csproj", "MicroTask.Services.Categories.Api/"]
COPY ["MicroTask.Services.Categories.Domain/MicroTask.Services.Categories.Domain.csproj", "MicroTask.Services.Categories.Domain/"]
RUN dotnet restore "MicroTask.Services.Categories.Api/MicroTask.Services.Categories.Api.csproj"
COPY . .
WORKDIR "/src/MicroTask.Services.Categories.Api"
RUN dotnet build "MicroTask.Services.Categories.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroTask.Services.Categories.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroTask.Services.Categories.Api.dll"]