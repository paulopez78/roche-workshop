# SDK
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS sdk-builder

COPY ./MeetupEvents ./MeetupEvents
RUN dotnet publish ./MeetupEvents -o /publish



# PUBLISH + DOTNET RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

COPY --from=sdk-builder /publish .

ENTRYPOINT [ "dotnet", "MeetupEvents.dll" ]