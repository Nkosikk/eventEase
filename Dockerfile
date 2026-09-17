FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EventEase/EventEase.csproj", "EventEase/"]
RUN dotnet restore "EventEase/EventEase.csproj"

COPY . .
WORKDIR "/src/EventEase"
RUN dotnet publish "EventEase.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "exec dotnet EventEase.dll --urls http://0.0.0.0:${PORT:-10000}"]
