FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY publisher/ .
COPY config/.Production.env config/.Production.env

EXPOSE 7777

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:7777

ENTRYPOINT ["dotnet", "Application.dll"]
