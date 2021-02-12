FROM mcr.microsoft.com/dotnet/sdk:5.0 AS builder

ARG SERVICE=Meetup.UserProfile/

WORKDIR /app

COPY ${SERVICE}*.csproj ${SERVICE}

RUN dotnet restore ${SERVICE}

COPY ${SERVICE} ${SERVICE}

RUN dotnet publish ${SERVICE} -o /app/publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:5.0 

WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "Meetup.UserProfile.dll"]

