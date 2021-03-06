FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ARG http_port=80
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${http_port}

FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build
WORKDIR /src
COPY sample.common.props .
COPY aggregators/Silky.GatewayHost /src/aggregators/Silky.GatewayHost
COPY microservices/order/Silky.Order.Application.Contracts /src/microservices/order/Silky.Order.Application.Contracts
COPY microservices/order/Silky.Order.Domain.Shared /src/microservices/order/Silky.Order.Domain.Shared
COPY microservices/account/Silky.Account.Application.Contracts /src/microservices/account/Silky.Account.Application.Contracts
COPY microservices/account/Silky.Account.Domain.Shared /src/microservices/account/Silky.Account.Domain.Shared
COPY microservices/stock/Silky.Stock.Application.Contracts /src/microservices/stock/Silky.Stock.Application.Contracts
COPY microservices/stock/Silky.Stock.Domain.Shared /src/microservices/stock/Silky.Stock.Domain.Shared

RUN dotnet restore /src/aggregators/Silky.GatewayHost/Silky.GatewayHost.csproj && \
    dotnet build --no-restore -c Release /src/aggregators/Silky.GatewayHost/Silky.GatewayHost.csproj

FROM build AS publish
WORKDIR /src/aggregators/Silky.GatewayHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Silky.GatewayHost.dll"]