# Test project using Asp.NET Core, Microsoft Orleans and the Direct Client functionality that 
# "allows co-hosting a client and silo in a way that let the client communicate more efficiently with not just the silo it's attached to, but the entire cluster."
# To give a complete overview it implements:
# - a WebApi controller,
# - DI example, injecting a custom Transient service in the controller,
# - grain storage using Azure Blob and Azure Table,
# - ProtobufNet serialization and a custom serialization for DateTimeOffset

## Pre-requisites

Install Asp Net Core 2.2 SDK (> 2.2.301)
Install Azure Storage Emulator
(optional) Install [Azure Storage Explorer (> 1.6.2)](https://azure.microsoft.com/en-us/features/storage-explorer/)

## Run it yourself

1. Install dotnet core 2.2 (or higher).
2. Run the Azure Storage Emulator
3. Run the WebApi: in src/06-Frontends/WebApi > run `dotnet run`
4. Navigate to http://localhost:5000/api/v1/products to hit the WebApi GET endpoint
5. Navigate to http://localhost:8080/ for the Orleans Dashboard
6. (optional) Run the test client: in src/06-Frontends/TestClient > run `dotnet run`

## Disclaimer

This code should be considered experimental. It works, however the project may have rough edges and has not been thoroughly tested.
I welcome feedback!

-- Mauro
