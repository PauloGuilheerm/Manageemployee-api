FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EmployeeManager.sln ./
COPY EmployeeManager.Api/EmployeeManager.Api.csproj EmployeeManager.Api/
COPY EmployeeManager.Domain/EmployeeManager.Domain.csproj EmployeeManager.Domain/
COPY EmployeeManager.Application/EmployeeManager.Application.csproj EmployeeManager.Application/
COPY EmployeeManager.Infrastructure/EmployeeManager.Infrastructure.csproj EmployeeManager.Infrastructure/
COPY EmployeeManager.Tests/EmployeeManager.Tests.csproj EmployeeManager.Tests/

RUN dotnet restore EmployeeManager.Api/EmployeeManager.Api.csproj

COPY . .

RUN dotnet publish EmployeeManager.Api/EmployeeManager.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmployeeManager.Api.dll"]
