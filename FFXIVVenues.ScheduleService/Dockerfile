FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

COPY ./FFXIVVenues.ScheduleService /src/FFXIVVenues.ScheduleService
COPY ./FFXIVVenues.ScheduleService.Client /src/FFXIVVenues.ScheduleService.Client
COPY ./FFXIVVenues.DomainData /src/FFXIVVenues.DomainData
COPY ./FFXIVVenues.VenueService.Client /src/FFXIVVenues.VenueService.Client

RUN dotnet publish /src/FFXIVVenues.ScheduleService/FFXIVVenues.ScheduleService.csproj -c Release -o /src/build

#-----

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
ENV TZ=Etc/Utc
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update
RUN apt install -y tzdata

WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.ScheduleService.dll"]
