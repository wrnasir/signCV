# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SignLearn.Api/SignLearn.Api.csproj SignLearn.Api/
RUN dotnet restore SignLearn.Api/SignLearn.Api.csproj
COPY SignLearn.Api/ SignLearn.Api/
RUN dotnet publish SignLearn.Api -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "SignLearn.Api.dll"]