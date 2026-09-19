FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

COPY ./FFXIVVenues.WebHookService /src/FFXIVVenues.WebHookService
COPY ./FFXIVVenues.VenueService.Client /src/FFXIVVenues.VenueService.Client
COPY ./FFXIVVenues.DomainData /src/FFXIVVenues.DomainData

RUN dotnet publish /src/FFXIVVenues.WebHookService/FFXIVVenues.WebHookService.csproj -c Release -o /src/build

#-----

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

ENV TZ=Etc/Utc
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update
RUN apt install -y tzdata
WORKDIR /app

WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.WebHookService.dll"]
