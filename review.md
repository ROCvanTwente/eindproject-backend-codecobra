# Code Review – eindproject-backend-codecobra

**Datum:** 1 juni 2026  
**Reviewer:** GitHub Copilot  
**Repository:** `ROCvanTwente/eindproject-backend-codecobra`  
**Technologie:** ASP.NET Core 10 / C# / Entity Framework Core / SQL Server  

---

## Samenvatting

De applicatie is een REST-API backend voor een QR-code toursstop-systeem. Het project maakt gebruik van ASP.NET Core Identity voor authenticatie en autorisatie, Entity Framework Core voor database-toegang en SQL Server als database. De architectuur is over het algemeen overzichtelijk en de controllers zijn redelijk gestructureerd.

Echter zijn er **kritieke beveiligingsproblemen** aangetroffen, in het bijzonder databasewachtwoorden die hardcoded in de broncode zijn opgeslagen en gecommuniceerd naar versiebeheer. Daarnaast ontbreekt autorisatie op de QR-code-eindpunten volledig, zijn er architectuurproblemen en is er geen testinfrastructuur aanwezig. Deze punten vereisen directe aandacht voordat de applicatie in productie gezet kan worden.

---

## Sterke punten

- **Duidelijke mapstructuur:** Controllers, Models, DTOs, Services en Data zijn netjes gescheiden in aparte mappen. Dit bevordert de leesbaarheid en onderhoudbaarheid.
- **Interface-abstractie voor services:** De `IQRCodeStatisticService`-interface zorgt voor een goede ontkoppeling tussen de controller en de service-implementatie, wat unit-testen makkelijker maakt.
- **Gebruik van DTOs:** Invoer via `CreateUserDto` en uitvoer via `UserResponseDto` zorgen voor een goede scheiding tussen intern domeinmodel en de API-contracten.
- **Rollback bij mislukte roltoewijzing:** In `UserController.AddUser` wordt de gebruiker teruggedraaid als de roltoewijzing mislukt — dit is goede transactionele logica.
- **Commentaar op eindpunten:** In `QRCodeController` zijn de endpoints voorzien van Nederlandse XML-commentaren met route en doel, wat de leesbaarheid bevordert.
- **CORS-beleid aanwezig:** Er is een CORS-policy geconfigureerd die alleen de bekende frontend-origins toestaat.

---

## Bevindingen

### 🔴 Kritiek

#### 1. Databasewachtwoord hardcoded in `appsettings.json`
**Bestand:** `backend/appsettings.json`

Het Azure SQL Server-wachtwoord staat volledig in plaintext in de repository:
```json
"DefaultConnection": "Server=db48690.public.databaseasp.net;Database=db48690;User Id=db48690;******;..."
```
Bovenaan het bestand staat ook het wachtwoord in een commentaarregel. Dit is gepushed naar versiebeheer en daarmee zichtbaar voor iedereen met toegang tot de repository. Dit is een ernstige beveiligingsovertreding. De inloggegevens zijn nu gecompromitteerd en moeten onmiddellijk worden geroteerd.

**Oplossing:** Gebruik omgevingsvariabelen, Azure Key Vault, of User Secrets (`dotnet user-secrets`) en voeg `appsettings.json` toe aan `.gitignore` of vervang de waarden door placeholders zoals `<YOUR_CONNECTION_STRING>`.

---

#### 2. Geen autorisatie op QRCode-eindpunten
**Bestand:** `backend/Controllers/QRCodeController.cs`

De `QRCodeController` heeft geen enkel `[Authorize]`-attribuut. Alle eindpunten — inclusief aanmaken (`POST /api/qrcode/add`), verwijderen (`DELETE /api/qrcode/delete/{id}`) en aanmaken van TourStops (`POST /api/qrcode/tourstop/add`) — zijn volledig publiek toegankelijk zonder authenticatie.

**Oplossing:** Voeg `[Authorize]` toe aan de controller (of minimaal aan de schrijf- en verwijderoperaties), overeenkomstig het patroon dat al toegepast wordt in `UserController`.

---

### 🟠 Hoog

#### 3. Verkeerde volgorde van middleware in `Program.cs`
**Bestand:** `backend/Program.cs`

De huidige volgorde is:
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontends");   // ❌ staat vóór UseRouting
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```

Volgens de [Microsoft-documentatie voor ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index) moet `UseCors` **na** `UseRouting` en **vóór** `UseAuthentication` worden geplaatst. De huidige volgorde kan leiden tot CORS-headers die niet correct worden toegevoegd aan responses, wat runtime-problemen met de frontend veroorzaakt.

**Oplossing:**
```csharp
app.UseRouting();
app.UseCors("AllowFrontends");
app.UseAuthentication();
app.UseAuthorization();
```

---

#### 4. `TrustServerCertificate=True` in de connection string
**Bestand:** `backend/appsettings.json`

De connection string bevat `TrustServerCertificate=True`, wat betekent dat het SSL-certificaat van de databaseserver niet wordt gevalideerd. Dit maakt de verbinding kwetsbaar voor man-in-the-middle (MITM) aanvallen.

**Oplossing:** Gebruik een geldig SSL-certificaat op de databaseserver en verwijder `TrustServerCertificate=True`, of zorg dat het certificaat wordt vertrouwd via de juiste kanalen.

---

#### 5. `app.db` gecommuniceerd naar versiebeheer
**Bestand:** `backend/app.db`

Een SQLite-databasebestand is gecommuniceerd naar het repository. Database-bestanden horen niet in versiebeheer: ze bevatten mogelijk testdata of zelfs gevoelige gegevens en zijn binair (niet diff-baar).

**Oplossing:** Voeg `*.db` toe aan `.gitignore` en verwijder het bestand uit de repository-geschiedenis.

---

### 🟡 Middel

#### 6. Dode code: klasse `identity` in `identity.cs`
**Bestand:** `backend/Areas/Identity/Data/identity.cs`

Deze klasse erft van `DbContext` maar is nooit geregistreerd in de DI-container en wordt nergens gebruikt. De werkelijke database-context is `AppDbContext` in `backend/Data/AppDbContext.cs`. Dit is verwarrend en vergroot de codebase onnodig.

**Oplossing:** Verwijder `identity.cs`.

---

#### 7. Model-klassen zonder namespace
**Bestanden:** `backend/Models/QRCode.cs`, `QRCodeStatistic.cs`, `TourStop.cs`, `ScanRequest.cs`, `CreateQRCodeRequest.cs`, `CreateTourStopRequest.cs`

Deze model-klassen zijn gedefinieerd in de globale namespace, terwijl alle andere bestanden in het project een expliciete namespace gebruiken (bijv. `backend.Models`, `backend.Services`, enz.). Dit is inconsistent en kan leiden tot naamconflicten bij grotere codebases.

**Oplossing:** Voeg aan alle modellen een passende namespace toe, bijv. `namespace backend.Models`.

---

#### 8. Race condition in `QRCodeStatisticService.RecordScanAsync`
**Bestand:** `backend/Services/QRCodeStatisticService.cs`

Bij gelijktijdige scan-requests kan zich een race condition voordoen: twee threads vinden tegelijkertijd geen bestaande statistieken en proberen tegelijkertijd een nieuw record aan te maken. Dit resulteert in een database-uniqheidsfout of verlies van een scan-telling.

**Oplossing:** Gebruik een database-level transactie met isolatieniveau of pas een atomaire upsert-operatie toe (bijv. via `ExecuteUpdateAsync`).

---

#### 9. `ScanQRCode` retourneert altijd HTTP 200, ook bij onbekende QR-code
**Bestand:** `backend/Controllers/QRCodeController.cs`, methode `ScanQRCode`

Als een scan binnenkomt voor een QR-code die niet bestaat, doet `RecordScanAsync` stil niets en retourneert de controller toch HTTP 200 OK. Dit maskeert fouten naar de beller.

**Oplossing:** Laat `RecordScanAsync` een boolean teruggeven (of gooi een exception), en retourneer HTTP 404 als de QR-code niet gevonden wordt.

---

#### 10. `UserResponseDto` heeft niet-geïnitialiseerde nullable-properties
**Bestand:** `backend/DTOs/UserResponseDto.cs`

```csharp
public string Id { get; set; }
public string Username { get; set; }
public string Email { get; set; }
public string Role { get; set; }
```

Met `<Nullable>enable</Nullable>` in het project (zie `backend.csproj`) zullen deze properties compilerwaarschuwingen geven over mogelijk-null referenties. Ze ontberen `= default!` initialisaties of een `?`-annotatie.

**Oplossing:** Voeg `= default!` toe aan elke property, of maak ze nullable met `string?`.

---

#### 11. Inconsistentie: CORS-origins dubbel gedefinieerd
**Bestanden:** `backend/Program.cs` en `backend/appsettings.json`

De toegestane CORS-origins zijn hardcoded in `Program.cs` maar staan óók in `appsettings.json` onder `AllowedOrigins`. De waarden in `appsettings.json` worden nooit gelezen; `Program.cs` gebruikt zijn eigen hardcoded lijst. Dit leidt tot een onderhoudsprobleem: bij een wijziging in de origins moet je weten dat je `Program.cs` moet bijwerken, niet `appsettings.json`.

**Oplossing:** Lees de origins in `Program.cs` uit `appsettings.json` via `builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()`.

---

### 🔵 Laag / Verbetersuggesties

#### 12. Geen tests aanwezig
De repository bevat geen enkel testproject. Er zijn geen unit-tests, integratietests of end-to-end-tests. Dit maakt regressie moeilijk te detecteren en herstructurering riskant.

**Aanbeveling:** Voeg minimaal een xUnit-testproject toe met unit-tests voor `QRCodeStatisticService` en integratietests voor de QRCode-controller-endpoints.

---

#### 13. `DeleteQRCode` retourneert HTTP 200 in plaats van HTTP 204
**Bestand:** `backend/Controllers/QRCodeController.cs`

De conventionele HTTP-statuscode voor een succesvolle DELETE-operatie zonder antwoordbody is `204 No Content`. Momenteel wordt `200 OK` met een JSON-body teruggegeven.

**Aanbeveling:** Gebruik `return NoContent()` voor consistentie met REST-conventies.

---

#### 14. Wildcard-versiespecificaties in `backend.csproj`
**Bestand:** `backend/backend.csproj`

Verschillende packages gebruiken `Version="*"`:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="*" />
```

Dit betekent dat bij elke build de nieuwste versie wordt opgehaald, wat breaking changes kan introduceren zonder dat dit zichtbaar is in de commit-history.

**Aanbeveling:** Pin alle package-versies expliciet (bijv. `Version="10.0.8"`).

---

#### 15. Overbodige NuGet-packages
**Bestand:** `backend/backend.csproj`

`NuGet.Packaging` (v6.12.5) en `NuGet.Protocol` (v6.12.5) worden als dependencies toegevoegd. Dit zijn NuGet-client-bibliotheken die geen doel dienen in een web-API backend. Ze vergroten de buildtijd en het deployment-pakket onnodig.

**Aanbeveling:** Verwijder deze packages tenzij ze expliciet ergens worden gebruikt.

---

#### 16. Geen Swagger/OpenAPI-documentatie
De API heeft geen Swagger of OpenAPI-configuratie. Dit maakt het voor consumenten van de API (frontend-ontwikkelaars) moeilijker om de endpoints te ontdekken en te begrijpen. Er is wel een `PostmanCollection.json` aanwezig, maar die wordt niet automatisch gesynchroniseerd met de code.

**Aanbeveling:** Voeg `builder.Services.AddEndpointsApiExplorer()` en `builder.Services.AddSwaggerGen()` toe, en activeer de Swagger-UI in ontwikkelomgevingen.

---

#### 17. `GetCurrentUserInfo` met `[AllowAnonymous]` op een `[Authorize(Roles = "Admin")]`-controller
**Bestand:** `backend/Controllers/UserController.cs`

De `GetCurrentUserInfo`-methode heeft `[AllowAnonymous]` terwijl de controller zelf `[Authorize(Roles = "Admin")]` heeft. Het doel lijkt te zijn dat iedere ingelogde gebruiker zijn eigen info kan ophalen. De huidige implementatie werkt technisch — niet-ingelogde gebruikers krijgen een 401 terug via de null-check — maar de bedoeling is onduidelijk en kan verwarrend zijn voor andere ontwikkelaars.

**Aanbeveling:** Maak de bedoeling explicieter: verplaats `GetCurrentUserInfo` naar een aparte `MeController` of voeg een `[Authorize]`-attribuut zonder rolbeperking toe.

---

## Risico's

| Prioriteit | Risico | Impact |
|---|---|---|
| 🔴 Kritiek | Databasewachtwoord in versiebeheer | Volledige database-toegang voor kwaadwillenden |
| 🔴 Kritiek | Geen autorisatie op QRCode-eindpunten | Iedereen kan QR-codes aanmaken, wijzigen en verwijderen |
| 🟠 Hoog | Verkeerde CORS-middlewarevolgorde | CORS-headers worden mogelijk niet toegepast |
| 🟠 Hoog | `TrustServerCertificate=True` | MITM-kwetsbaarheid op databaseverbinding |
| 🟠 Hoog | `app.db` in repository | Mogelijke datalekkage, binaire conflicten |
| 🟡 Middel | Race condition bij gelijktijdige scans | Datastorruptie of verloren scans bij hoge belasting |
| 🟡 Middel | Geen tests | Regressies worden niet automatisch gedetecteerd |

---

## Aanbevelingen

### Direct uitvoeren (vóór productie-release)

1. **Roteer het databasewachtwoord** onmiddellijk — het is gecompromitteerd door de commit-history.
2. **Verwijder de credentials** uit `appsettings.json` en gebruik omgevingsvariabelen of Azure Key Vault. Voeg een `appsettings.json`-placeholder toe aan het `.gitignore`.
3. **Voeg `[Authorize]` toe** aan alle schrijf- en verwijderoperaties in `QRCodeController`.
4. **Verwijder `app.db`** uit de repository en voeg `*.db` toe aan `.gitignore`.
5. **Herstel de middleware-volgorde** in `Program.cs` (zie bevinding 3).

### Op korte termijn (volgende sprint)

6. Verwijder de dode `identity`-klasse uit `backend/Areas/Identity/Data/identity.cs`.
7. Voeg namespaces toe aan alle model-klassen.
8. Lees CORS-origins uit `appsettings.json` in plaats van hardcoding.
9. Pin alle package-versies expliciet in `backend.csproj`.
10. Voeg een testproject toe met unit-tests voor services en controllers.

### Op langere termijn

11. Voeg Swagger/OpenAPI-documentatie toe.
12. Overweeg het gebruik van een `ILogger` in controllers en services voor betere observability.
13. Implementeer een foutafhandelingsstrategie (bijv. globale exception handler via `UseExceptionHandler` ook in development, of `ProblemDetails`-middleware).
