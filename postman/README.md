# MusicRecords API i Postman

## Opsæt User Secrets i Visual Studio 2026

1. Åbn `MusicRecords.slnx`. Højreklik på **BackendMusicRecords.API** i **Solution Explorer**, og vælg **Set as Startup Project**.
2. Højreklik på samme projekt, og vælg **Manage User Secrets**. Projektets `UserSecretsId` er allerede konfigureret. Visual Studio åbner `secrets.json` uden for repositoryet.
3. Vælg **Tools > Command Line > Developer PowerShell**, og generér en tilfældig nøgle på 32 bytes. Følgende kommando kopierer Base64-værdien til udklipsholderen uden at skrive den i terminalen:

```powershell
$bytes = New-Object byte[] 32; $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create(); $rng.GetBytes($bytes); $rng.Dispose(); [Convert]::ToBase64String($bytes) | Set-Clipboard
```

4. Indsæt følgende struktur i **Manage User Secrets**, erstat alle pladsholdere lokalt, og gem filen. Indsæt den genererede nøgle og vælg to forskellige lokale testadgangskoder:

```json
{
  "Jwt": {
	"SigningKey": "INDSÆT_GENERERET_BASE64_NØGLE"
  },
  "LocalUsers": {
	"User": {
	  "Password": "VÆLG_LOKAL_USER_ADGANGSKODE"
	},
	"Admin": {
	  "Password": "VÆLG_LOKAL_ADMIN_ADGANGSKODE"
	}
  }
}
```

5. Ryd udklipsholderen efter indsættelsen. Gem aldrig adgangskoder eller nøgler i `appsettings.json`, kildekode eller eksporterede Postman-filer. User Secrets er lokal udviklingskonfiguration, ikke krypteret produktionslagring.

De to brugere hedder som standard `user` og `admin` med rollerne **User** og **Admin** og id 1 og 2. Brugernavne samt issuer `MusicRecords.Api` og audience `MusicRecords.Frontend` står i `appsettings.json` og kan ændres der uden hemmeligheder. Repositoryet bygger listen ved opstart og gemmer kun ASP.NET Core PasswordHasher-hashes; inputadgangskoderne findes kun i lokal konfiguration. Manglende/ugyldig nødvendig konfiguration stopper opstarten med en fejl, der angiver konfigurationsnøglen. Ingen fallback-signering findes.

## Start API'et

Efter opsætning af User Secrets: vælg profilen **https** ved startknappen i Visual Studio, og tryk **F5** eller **Ctrl+F5**. Alternativt køres fra repository-roden:

```powershell
dotnet run --project BackendMusicRecords.API/BackendMusicRecords.API.csproj --launch-profile https
```

API'et lytter på `https://localhost:7297`. Hvis HTTPS-certifikatet ikke er betroet, kan det oprettes og betros med `dotnet dev-certs https --trust`.

## Importér collectionen

1. Åbn Postman, vælg **Import**, og vælg `postman/MusicRecords.postman_collection.json`.
2. Åbn collectionen **MusicRecords**, og kontrollér at variablen `baseUrl` er `https://localhost:7297`.
3. Hvis API'et kører på en anden adresse, ændres collection-variablen `baseUrl`. Undgå en environment-variabel med samme navn, som overskriver collectionens værdi.

## Udfyld brugeroplysninger lokalt i Postman

Collectionen eksporterer alle brugeroplysninger med tomme værdier. Opret et lokalt environment, fx **MusicRecords lokalt**, vælg det øverst til højre, og tilføj følgende variabler med lokale værdier (i ældre Postman-versioner: **Current Value**, ikke **Initial Value**). Del/synkronisér ikke værdierne med **Share**:

| Variabel | Lokal værdi |
| --- | --- |
| `userUsername` | `user` eller det konfigurerede User-brugernavn |
| `userPassword` | User-adgangskoden fra dine User Secrets |
| `adminUsername` | `admin` eller det konfigurerede Admin-brugernavn |
| `adminPassword` | Admin-adgangskoden fra dine User Secrets |
| `invalidPassword` | En ikke-tom adgangskode, der er forskellig fra User-adgangskoden |
| `unknownUsername` | Et ikke-tomt brugernavn, der hverken er User eller Admin |

Environment-værdierne overskriver collectionens tomme pladsholdere. Eksportér ikke et udfyldt environment, adgangskoder eller tokens til repositoryet. Vælg gerne følsom/secret visning for adgangskodevariablerne.

## Login i Postman

Alle login-requests bruger **POST** `{{baseUrl}}/api/auth/login` med JSON-felterne `username` og `password`:

| Request | Forventet status |
| --- | --- |
| Login som User – korrekt login og lokalt token | 200, rolle User |
| Login som Admin – korrekt login og lokalt token | 200, rolle Admin |
| Login med forkert adgangskode – generel fejl | 401 |
| Login med ukendt bruger – samme generelle fejl | 401 |
| Login uden felter – valideringsfejl | 400 |

Korrekt login returnerer kun `token`, `expiresAtUtc`, `username` og `role`. Forkert brugernavn og adgangskode giver samme besked: **Forkert brugernavn eller adgangskode.** Manglende, tomme eller kun whitespace-felter giver 400 med valideringsfejl. For at prøve tomme felter manuelt kan du duplikere requesten uden felter og sende `{"username":"","password":""}`.

Ved korrekt login kontrolleres JWT-format, bruger/rolle og UTC-udløb omkring 30 minutter frem (med et minuts tolerance i Postman); derefter gemmes `token` med `pm.variables.set`. Variablen er **runtime-lokal**, ikke eksporteret eller delt. Den kan bruges som **Bearer Token** `{{token}}` i senere requests i **samme Collection Runner-kørsel**. Admin-login overskriver User-tokenet, når begge køres. Ved enkeltstående **Send** findes variablen kun i denne request-kørsel; til senere separate Send-requests kan tokenet kopieres til en lokal environment-variabel `token`, som slettes efter brug og aldrig deles/eksporteres. De eksisterende GET-requests kræver fortsat intet token.

## Kør alle HTTP-requests og tests i to kørsler

Collectionen indeholder **25 requests og 62 tests**. Kør først de 11 eksisterende requests og derefter oprettelsesmappens 14 requests:

1. Start API'et som beskrevet ovenfor med de to oprindelige eksempelplader, og lad det køre under begge kørsler.
2. Åbn menuen ved collectionen **MusicRecords**, og vælg **Run collection**.
3. Vælg det udfyldte lokale environment og én iteration. Markér kun de **11 eksisterende requests** i collectionens rod; fravælg mappen **Oprettelse af plader – kør sidst**.
4. Tryk **Run MusicRecords**, og kontrollér, at alle **43 tests** består: 28 liste-/søgetests og 15 logintests.
5. Start derefter en ny Runner-kørsel med samme lokale environment og én iteration. Markér kun hele mappen **Oprettelse af plader – kør sidst**, og kør dens **14 requests i den eksisterende rækkefølge**. Kontrollér, at alle **19 tests** består.

Oprettelsesmappen logger selv ind som Admin og User og gemmer sine tokens runtime-lokalt i denne kørsel. Den kontrollerer gyldig Admin-oprettelse (201), offentlig søgning efter den oprettede plade, ugyldige data (400), manglende token (401), User uden Admin-rolle (403) og til sidst, at listen kun indeholder de to eksempelplader og den ene godkendte oprettelse.

**Stop og start API'et før en ny samlet kørsel af begge dele.** Oprettelsen ændrer listen i hukommelsen, og genstart gendanner de to eksempelplader. Genstart ikke mellem de to kørsler ovenfor, og opret ikke ekstra plader undervejs. Der bruges hverken reset- eller slettefunktion.

Den oprindelige request uden filtre er bevaret med navnet **Hent alle plader uden filtre**. En enkelt request kan også køres med **Send**; resultaterne vises under **Test Results**.

| Filtre | Forventet antal | Forventede id'er |
| --- | --- | --- |
| Ingen | 2 | 1 og 2 |
| `title=Citrus` | 1 | 2 |
| `artist=Appelsin` | 2 | 1 og 2 |
| `title=Citrus&artist=Ukendt` | 0 | Ingen (AND) |
| `title=Ukendt` | 0 | Ingen |
| `title=citrus` | 1 | 2 (uden forskel på store og små bogstaver) |

Hver liste-/søgrequest kontrollerer statuskode 200, at Content-Type indeholder `application/json`, at svaret er et array, og det forventede antal elementer. Ikke-tomme resultater matches efter `id`, ikke rækkefølge, og kontrolleres mod alle felter i repositoryets eksempeldata:

| id | title | artist | duration | publicationYear |
| --- | --- | --- | --- | --- |
| 1 | Ta på mine frugter | Appelsinsovs | 3660 | 2025 |
| 2 | Citrusdrømme | Appelsinsovs | 2823 | 2026 |

Søgningen matcher begyndelsen af titel og kunstner uden forskel på store og små bogstaver. Når begge filtre er angivet, skal begge matche.

## Logout og afgrænsning

Login og logout er implementeret i frontend. Logout sker ved at fjerne tokenet i frontend; der er intet logout-endpoint. Et allerede udstedt token forbliver gyldigt indtil udløbet efter 30 minutter, også hvis det kopieres før logout.

Oprettelse af musikplader er implementeret via **POST /api/records** og kræver Admin-rollen. Pladerne gemmes kun i hukommelsen. Sletning, opdatering, database og Azure er ikke implementeret. Token-revocation, registrering og brugeradministration er heller ikke implementeret.

## Byg og kør unit tests

I Visual Studio: vælg **Build > Build Solution**, åbn **Test > Test Explorer**, og vælg **Run All Tests**. Alternativt fra repository-roden:

```powershell
dotnet build MusicRecords.slnx
dotnet test MusicRecords.slnx --no-build
```

Unit tests bruger kun egne testadgangskoder og tilfældigt genererede testnøgler. De kræver ikke User Secrets og starter ikke HTTP-serveren.

## Kontrollér kun JSON-filen

Kør fra repository-roden i PowerShell:

```powershell
Get-Content -Raw -Encoding UTF8 postman/MusicRecords.postman_collection.json | ConvertFrom-Json -ErrorAction Stop | Out-Null
```

Hvis kommandoen afsluttes uden fejl, kan filen parses som JSON. Denne kontrol sender **ingen HTTP-requests**, kører **ikke Postman-tests** og validerer **ikke collectionen mod v2.1-skemaet**. Faktisk kontrol af API-svar kræver kørslen i Collection Runner ovenfor.
