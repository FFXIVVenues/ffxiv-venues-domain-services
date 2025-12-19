FROM ubuntu:latest AS base
ENV TZ=Etc/Utc
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update
RUN apt install -y wget libcurl4 tzdata libicu74

RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
RUN chmod +x dotnet-install.sh
RUN ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

FROM base AS build
COPY ./FFXIVVenues.OGCardService /src/FFXIVVenues.OGCardService
COPY ./FFXIVVenues.DomainData /src/FFXIVVenues.DomainData
RUN ./dotnet-install.sh --channel STS --install-dir /usr/share/dotnet
RUN dotnet publish /src/FFXIVVenues.OGCardService/FFXIVVenues.OGCardService.csproj -c Release -o /src/build

FROM base AS final

RUN ./dotnet-install.sh --channel STS --runtime aspnetcore --install-dir /usr/share/dotnet

ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80

WORKDIR /appim g
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.OGCardService.dll"]
