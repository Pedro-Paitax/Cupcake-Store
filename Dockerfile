# Imagem da API + painel + loja web (usada no deploy gratuito do Render)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY CupcakeStore.Api/CupcakeStore.Api.csproj CupcakeStore.Api/
RUN dotnet restore CupcakeStore.Api/CupcakeStore.Api.csproj
COPY CupcakeStore.Api/ CupcakeStore.Api/
RUN dotnet publish CupcakeStore.Api/CupcakeStore.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production \
    PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CupcakeStore.Api.dll"]
