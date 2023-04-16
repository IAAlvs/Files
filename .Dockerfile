FROM mcr.microsoft.com/dotnet/sdk:7.0.x AS build
WORKDIR /src
COPY src/Files/Files.csproj .
RUN dotnet restore
COPY ./src/B2bPolicies .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "B2bPolicies.dll"]