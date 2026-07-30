FROM node:23-alpine AS frontend-build
WORKDIR /app
COPY cms.client/package*.json ./
RUN npm ci
COPY cms.client/ .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY CMS.Server/CMS.Server.csproj .
RUN dotnet restore
COPY CMS.Server/ .
COPY --from=frontend-build /app/dist/cms.client/browser ./wwwroot
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=backend-build /publish .
ENTRYPOINT ["dotnet", "CMS.Server.dll"]
