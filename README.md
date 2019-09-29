# orleans-angular-aspnetcore

This is a test project using [Microsoft Orleans](https://github.com/dotnet/orleans), Asp.NET Core and the Orleans' Direct Client functionality that 
"allows co-hosting a client and silo in a way that let the client communicate more efficiently with not just the silo it's attached to, but the entire cluster."
To give a complete overview it implements:
- a WebApi controller,
- DI example, injecting a custom Transient service in the controller,
- grain storage using Azure Blob and Azure Table,
- ProtobufNet serialization and a custom serialization for DateTimeOffset

### orleans Direct client

Co-hosting cluster and http client can be achieved using the Hosted client. This means that a Orleans Silo and a Http API may be the same process, so no serialization would be done in that case.
Direct client is enabled by default from Orleans 2.3. 
The documentation is coming, take a look at the [github issue here](https://github.com/dotnet/orleans/issues/5144). 
In the meantime take a look to this [stackoverflow thread](https://stackoverflow.com/questions/54841844/orleans-direct-client-in-asp-net-core-project/54842916#54842916).

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
