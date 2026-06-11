FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BusTrackingAPI/BusTrackingAPI/BusTrackingAPI.csproj BusTrackingAPI/BusTrackingAPI/
RUN dotnet restore BusTrackingAPI/BusTrackingAPI/BusTrackingAPI.csproj

COPY BusTrackingAPI/BusTrackingAPI/ BusTrackingAPI/BusTrackingAPI/
RUN dotnet publish BusTrackingAPI/BusTrackingAPI/BusTrackingAPI.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BusTrackingAPI.dll"]
