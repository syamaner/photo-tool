FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base

USER $APP_UID
 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src/PhotoTool
COPY ./PhotoTool.sln ./PhotoTool.sln
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore
COPY . .

FROM build AS publish
WORKDIR /src/PhotoTool

ARG BUILD_CONFIGURATION=Release
RUN dotnet publish PhotoTool.Cli/PhotoTool.Cli.csproj  -c $BUILD_CONFIGURATION -o /app/publish  /p:UseAppHost=false

FROM base AS final

USER root
RUN apk add  --update --no-cache  exiftool 
USER $APP_UID

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PhotoTool.Cli.dll"]

