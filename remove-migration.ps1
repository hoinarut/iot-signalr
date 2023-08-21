$Env:ASPNETCORE_ENVIRONMENT = "Local"
dotnet ef migrations remove --startup-project ./IotSignalR.Server/IotSignalR.Server.csproj --project ./IotSignalR.Persistence/IotSignalR.Persistence.csproj

