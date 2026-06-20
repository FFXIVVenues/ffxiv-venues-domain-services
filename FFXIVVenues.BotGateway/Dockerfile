FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

COPY ./FFXIVVenues.BotGateway /src/FFXIVVenues.BotGateway
COPY ./FFXIVVenues.DomainData /src/FFXIVVenues.DomainData
COPY ./FFXIVVenues.FlagService.Client /src/FFXIVVenues.FlagService.Client

RUN dotnet publish /src/FFXIVVenues.BotGateway/FFXIVVenues.BotGateway.csproj -c Release -o /src/build

#-----

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
ENV TZ=Etc/Utc
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update
RUN apt install -y tzdata

WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.BotGateway.dll"]
