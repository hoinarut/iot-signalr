param([string] $Name)
dotnet ef migrations add $Name --startup-project ./IotSignalR.Server/IotSignalR.Server.csproj --project ./IotSignalR.Persistence/IotSignalR.Persistence.csproj

