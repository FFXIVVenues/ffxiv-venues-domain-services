FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

COPY ./FFXIVVenues.OGCardService /src/FFXIVVenues.OGCardService
COPY ./FFXIVVenues.DomainData /src/FFXIVVenues.DomainData

RUN dotnet publish /src/FFXIVVenues.OGCardService/FFXIVVenues.OGCardService.csproj -c Release -o /src/build

#-----

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

ENV TZ=Etc/Utc
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update
RUN apt install -y tzdata
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://0.0.0.0:80
ENV FFXIV_VENUES_API__HttpsOnly=False
EXPOSE 80

WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.OGCardService.dll"]
