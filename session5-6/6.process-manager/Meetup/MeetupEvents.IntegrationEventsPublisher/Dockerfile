FROM mcr.microsoft.com/dotnet/sdk:5.0 AS builder

ARG SERVICE=MeetupEvents.IntegrationEventsPublisher/
ARG CONTRACTS=MeetupEvents.Contracts/

WORKDIR /app

COPY ${SERVICE}*.csproj ${SERVICE}
COPY ${CONTRACTS}*.csproj ${CONTRACTS}

RUN dotnet restore ${SERVICE}

COPY ${SERVICE} ${SERVICE}
COPY ${CONTRACTS} ${CONTRACTS}

RUN dotnet publish ${SERVICE} -o /app/publish -c Release


FROM mcr.microsoft.com/dotnet/aspnet:5.0 

WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "MeetupEvents.IntegrationEventsPublisher.dll"]
