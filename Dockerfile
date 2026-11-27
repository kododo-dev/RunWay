FROM node:24-alpine AS spa
WORKDIR /spa
COPY src/RunWay.Dashboard/SPA/package*.json ./
RUN npm ci
COPY src/RunWay.Dashboard/SPA/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY src/ ./
COPY --from=spa /spa/dist ./RunWay.Dashboard/SPA/dist
RUN dotnet publish RunWay.Demo.WebApp/RunWay.Demo.WebApp.csproj \
    --configuration Release \
    --framework net11.0 \
    --output /app

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Kododo.RunWay.Demo.WebApp.dll"]
