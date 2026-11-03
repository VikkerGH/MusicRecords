# MusicRecords API i Postman

## Start API'et

Start `BackendMusicRecords.API` i Visual Studio med profilen `https`, eller kør fra repository-roden:

```powershell
dotnet run --project BackendMusicRecords.API/BackendMusicRecords.API.csproj --launch-profile https
```

API'et lytter på `https://localhost:7297`. Hvis HTTPS-certifikatet ikke er betroet, kan det oprettes og betros med `dotnet dev-certs https --trust`.

## Importér collectionen

1. Åbn Postman, vælg **Import**, og vælg `postman/MusicRecords.postman_collection.json`.
2. Åbn collectionen **MusicRecords**, og kontrollér at variablen `baseUrl` er `https://localhost:7297`.
3. Hvis API'et kører på en anden adresse, ændres collection-variablen `baseUrl`. Undgå en environment-variabel med samme navn, som overskriver collectionens værdi.

## Kør alle HTTP-requests og tests samlet

1. Start API'et som beskrevet ovenfor, og lad det køre under hele testen.
2. Åbn menuen ved collectionen **MusicRecords**, og vælg **Run collection**.
3. Markér alle seks requests i Collection Runner, og vælg én iteration.
4. Tryk **Run MusicRecords**, og kontrollér, at alle seks requests og alle 28 tests består i kørselsoversigten.

Den oprindelige request uden filtre er bevaret med navnet **Hent alle plader uden filtre**. En enkelt request kan også køres med **Send**; resultaterne vises under **Test Results**.

| Filtre | Forventet antal | Forventede id'er |
| --- | --- | --- |
| Ingen | 2 | 1 og 2 |
| `title=Citrus` | 1 | 2 |
| `artist=Appelsin` | 2 | 1 og 2 |
| `title=Citrus&artist=Ukendt` | 0 | Ingen (AND) |
| `title=Ukendt` | 0 | Ingen |
| `title=citrus` | 1 | 2 (uden forskel på store og små bogstaver) |

Hver request kontrollerer statuskode 200, at Content-Type indeholder `application/json`, at svaret er et array, og det forventede antal elementer. Ikke-tomme resultater matches efter `id`, ikke rækkefølge, og kontrolleres mod alle felter i repositoryets eksempeldata:

| id | title | artist | duration | publicationYear |
| --- | --- | --- | --- | --- |
| 1 | Ta på mine frugter | Appelsinsovs | 3660 | 2025 |
| 2 | Citrusdrømme | Appelsinsovs | 2823 | 2026 |

Søgningen matcher begyndelsen af titel og kunstner uden forskel på store og små bogstaver. Når begge filtre er angivet, skal begge matche.

## Kontrollér kun JSON-filen

Kør fra repository-roden i PowerShell:

```powershell
Get-Content -Raw -Encoding UTF8 postman/MusicRecords.postman_collection.json | ConvertFrom-Json -ErrorAction Stop | Out-Null
```

Hvis kommandoen afsluttes uden fejl, kan filen parses som JSON. Denne kontrol sender **ingen HTTP-requests**, kører **ikke Postman-tests** og validerer **ikke collectionen mod v2.1-skemaet**. Faktisk kontrol af API-svar kræver kørslen i Collection Runner ovenfor.
