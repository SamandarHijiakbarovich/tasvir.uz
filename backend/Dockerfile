# Tasvir.uz API (barcha modullar bitta hostda). Render, Railway va boshqa Docker xostinglar uchun.
# Qurish: docker build -t tasvir-api backend

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Hosts/Tasvir.Api/Tasvir.Api.csproj -c Release -o /app --nologo

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# Npgsql (PostgreSQL) GSS/Kerberos kutubxonasini qidiradi; bo'lmasa jarayon yiqilishi mumkin
RUN apt-get update     && apt-get install -y --no-install-recommends libgssapi-krb5-2     && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
# Render PORT o'zgaruvchisini beradi (odatda 10000); boshqa xostinglarda ham shu port ishlatiladi
ENV ASPNETCORE_HTTP_PORTS=10000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_GCHeapHardLimitPercent=70
EXPOSE 10000
USER $APP_UID
ENTRYPOINT ["dotnet", "Tasvir.Api.dll"]
