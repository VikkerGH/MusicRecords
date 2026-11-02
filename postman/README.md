# MusicRecords API i Postman

## Start API'et

Start `BackendMusicRecords.API` i Visual Studio med profilen `https`, eller kør fra repository-roden:

```powershell
dotnet run --project BackendMusicRecords.API/BackendMusicRecords.API.csproj --launch-profile https
```

API'et lytter på `https://localhost:7297`. Hvis HTTPS-certifikatet ikke er betroet, kan det oprettes og betros med `dotnet dev-certs https --trust`.

## Importér og kør requesten

1. Åbn Postman, vælg **Import**, og vælg `postman/MusicRecords.postman_collection.json`.
2. Åbn collectionen **MusicRecords**, og kontrollér at variablen `baseUrl` er `https://localhost:7297`.
3. Vælg **Get all music records**, og tryk **Send**.
4. Kontrollér svarkoden og testresultaterne under **Test Results**.
