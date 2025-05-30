#### Migration Scripts

```bash
dotnet ef migrations add InitialMigration -o Shared/Application/Data/Migrations -c SmartChargingDbContext
dotnet ef database update -c SmartChargingDbContext
```
