# Home OS

Privatni "kućni operativni sistem" — jedna aplikacija koja okuplja cjelokupnu administraciju domaćinstva (zadaci, kalendar, podsjetnici, kanban, bilješke, liste za kupovinu, finansije, kućna administracija) na jedno mjesto, dijeljeno među članovima doma, uz e-mail obavještenja i sinhronizaciju u realnom vremenu. Sve je povezano i zamišljeno kao **platforma**: novi modul se dodaje kao ravnopravan građanin, bez izmjena postojećih.

## Sadržaj
- [Tehnološki stek](#tehnološki-stek)
- [Pokretanje (development)](#pokretanje-development)
- [Podešavanje konekcije na bazu](#podešavanje-konekcije-na-bazu)
- [Podešavanje mail servera](#podešavanje-mail-servera)
- [Deployment na produkciju (IIS)](#deployment-na-produkciju-iis)
- [Implementirano po modulima](#implementirano-po-modulima)
- [Arhitektura i proširivost](#arhitektura-i-proširivost)
- [Vodič: dodavanje novog modula](#vodič-dodavanje-novog-modula)
- [Svjesna pojednostavljenja](#svjesna-pojednostavljenja)
- [Šta bi se uradilo sa više vremena](#šta-bi-se-uradilo-sa-više-vremena)

## Tehnološki stek

- **ASP.NET Core 8 MVC** (server-rendered Razor Views), jedan projekat
- **Entity Framework Core 8** (Code First, migracije) + **SQL Server LocalDB** (dev) / SQL Server (prod)
- **ASP.NET Core Identity** (autentikacija, e-mail kao korisničko ime)
- **Bootstrap 5** (responzivan dizajn), **SortableJS** (kanban drag-and-drop), **FullCalendar** (kalendar)
- **SignalR** (sinhronizacija u realnom vremenu)
- **MailKit / Gmail SMTP** (e-mail obavještenja)
- **Lokalizacija** (`en` primarni, `bs`) preko `IStringLocalizer`/`IViewLocalizer` + `.resx`

## Pokretanje (development)

Preduslovi: .NET 8 SDK, SQL Server LocalDB (dolazi uz Visual Studio; instanca `(localdb)\mssqllocaldb`).

```bash
cd HomeOS/HomeOS
# 1) kreiraj appsettings.Development.json (vidi sekciju ispod) — NIJE u Gitu
# 2) primijeni migracije (kreira bazu)
dotnet ef database update
# 3) (opciono) unesi Gmail App Password za e-mail
dotnet user-secrets set "Smtp:FromEmail" "tvojnalog@gmail.com"
dotnet user-secrets set "Smtp:AppPassword" "16-znakovni-app-password"
# 4) pokreni
dotnet run
```

Prvi registrovani korisnik kreira svoje domaćinstvo i postaje **owner**.

## Podešavanje konekcije na bazu

Connection string se čita iz `ConnectionStrings:DefaultConnection`. Tajne se **ne drže u Gitu**.

**Development** — `HomeOS/HomeOS/appsettings.Development.json` (ignorisan u `.gitignore`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HomeOS;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Produkcija** — u `appsettings.Production.json` na serveru (vidi Deployment), sa pravim SQL Serverom, npr.:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SQLSERVER\\INSTANCA;Database=HomeOS;User Id=homeos;Password=...;TrustServerCertificate=True"
  }
}
```

## Podešavanje mail servera

E-mail ide preko Gmail SMTP-a (MailKit). Konfiguracija je u sekciji `Smtp`; klasa `SmtpOptions`:

| Ključ | Opis | Default |
|---|---|---|
| `Smtp:Host` | SMTP server | `smtp.gmail.com` |
| `Smtp:Port` | Port (STARTTLS) | `587` |
| `Smtp:FromEmail` | Gmail adresa (ujedno korisničko ime) | — |
| `Smtp:FromName` | Prikazano ime pošiljaoca | `Home OS` |
| `Smtp:AppPassword` | **Google App Password** (nalog mora imati 2FA) | — |

**Bitno:** koristi se Gmail **App Password**, ne obična lozinka (Google → Security → 2-Step Verification → App passwords). Bez `AppPassword` aplikacija radi normalno — slanje se samo tiho preskoči (`GmailEmailSender` vrati `false`).

- **Development:** kroz user-secrets (komande iznad).
- **Produkcija:** u `appsettings.Production.json` na serveru:
  ```json
  {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "FromEmail": "tvojnalog@gmail.com",
      "FromName": "Home OS",
      "AppPassword": "16-znakovni-app-password"
    }
  }
  ```
  (Alternativa: environment varijable `Smtp__FromEmail`, `Smtp__AppPassword` — dvostruka donja crta za ugniježđene ključeve.)

**Napomene:**
- Izlazni firewall servera mora dozvoliti port 587. Ako je blokiran (čest slučaj na shared hostingu), koristi port 465 ili drugog provajdera.
- Zamjena provajdera (SendGrid, Mailgun, SMTP relej...) je **jedna nova klasa** koja implementira `IEmailSender` + jedna DI linija — nijedan pozivalac se ne mijenja (tako je urađen i prelaz Resend → Gmail).
- Linkovi u e-mailu (npr. "Otvori zadatak") grade se iz aktivnog HTTP zahtjeva, pa rade bez dodatne konfiguracije. `App:BaseUrl` je opcioni fallback za slanje van zahtjeva (npr. budući pozadinski job).

## Deployment na produkciju (IIS)

1. **Objavi:** `dotnet publish -c Release`. Kopiraj izlaz na server.
2. **IIS preduslovi:** instaliran **ASP.NET Core Hosting Bundle**; aplikacijski pool na "No Managed Code"; HTTPS binding.
3. **Konfiguracija na serveru:** `appsettings.Production.json` sa `ConnectionStrings` i `Smtp` (vidi gore). Fajl drži samo na serveru; ne ide u repo, i ne pregazi ga sljedeći publish ako ga ne uključuješ u projekat.
4. **Baza:** pokreni [`HomeOS/HomeOS/migrate.sql`](HomeOS/migrate.sql) na produkcijskoj bazi (SSMS ili `sqlcmd -S SERVER -d HomeOS -i migrate.sql`). Skript je **idempotentan** (primjenjuje samo migracije koje fale). **Napravi backup prije** — migracija `RemoveKanbanBoards` briše stare `Boards`/`Columns`/`Cards` tabele.
   - Skript regenerišeš sa: `dotnet ef migrations script --idempotent -o migrate.sql`.
5. **Restart** aplikacijskog poola.

## Implementirano po modulima

| Modul | Sažetak |
|---|---|
| **Dashboard ("Danas")** | Agregira doprinose svih uključenih modula (dospjeli zadaci, današnji događaji, aktivni podsjetnici, predstojeći računi, dokumenti koji ističu). Quick Capture (zadatak/bilješka/podsjetnik) sa bilo kog ekrana. Univerzalna pretraga i komandna paleta (Ctrl+K). |
| **Zadaci** | CRUD, podzadaci, tagovi, prioritet/status/rok, filter i sort, indikator prekoračenog roka, ponavljajući zadaci (Done → sljedeća instanca), dodjela članu + e-mail obavještenje. |
| **Kalendar** | FullCalendar prikaz događaja; rokovi zadataka se projektuju read-only (uz dozvolu modula). |
| **Podsjetnici** | CRUD, jednokratni/ponavljajući (resolve → sljedeći), više primalaca, snooze, e-mail + in-app obavještenje. |
| **Kanban** | Jedna tabla koja se **automatski formira od zadataka** (kolone = status); drag mijenja status. Nema vlastitih podataka. |
| **Bilješke** | CRUD, tagovi (dijeljeni sa zadacima), dnevnik (jedan zapis po danu po članu), povezivanje sa zadatkom/događajem. |
| **Liste za kupovinu** | CRUD lista i stavki, čekiranje bez reload-a (AJAX). |
| **Finansije** | Transakcije po kategoriji, mjesečni budžet po kategoriji (progress bar), prihodi/troškovi/neto, računi sa dospijećem (auto-podsjetnik preko event bus-a), split expense po članu ("podijeli jednako" ili proizvoljno). |
| **Kućna administracija** | Dokumenti (metapodaci + datum isteka → auto-podsjetnik za obnovu) i kontakti. |

**Zajedničke sposobnosti:** vidljivost (privatno / cijelo domaćinstvo / specifične osobe), dodjela članovima, članovi domaćinstva sa owner ulogom i pozivom po e-mailu, RBAC po članu, individualna podešavanja kategorija obavještenja, lokalizacija (EN/BS), sinhronizacija u realnom vremenu.

## Arhitektura i proširivost

Sistem je platforma; Shell (navbar, pretraga, dashboard, komandna paleta, upravljanje modulima) se **generiše iz registra**, nikad hardkodiran. Dodavanje modula = dodavanje njegovih fajlova + par DI linija u `Program.cs`.

- **Registar modula** — `IModuleDescriptor` + `IModuleRegistry` + `ModuleState` (uključi/isključi po domaćinstvu).
- **Univerzalna pretraga** — svaki modul prijavljuje `ISearchable` provider.
- **Dashboard** — svaki modul doprinosi `IDashboardContributor` sekciju.
- **Event bus** — `IEventBus`/`IEventHandler` za kooperaciju bez direktne zavisnosti: zadatak/račun/dokument sa datumom → Reminders reaguje; dodijeljen zadatak → e-mail; sve promjene → SignalR broadcast.
- **Dozvole** — dva sloja: dozvola *modula na tuđe podatke* (`IPermissionService`/`ModulePermissionState`) i pristup *člana modulu* (`IMemberAccessService`/`MemberModuleAccess`).
- **Vidljivost** — `Visibility` (Private/Household/SpecificMembers) + `ItemShare`; jedan `VisibleTo(...)` helper u svim upitima.
- **Zajednički core servisi** — `IEmailSender`, `IRecurrenceService`, `INotificationPreferenceService`, `IItemSharingService`, `IAppUrlBuilder`, `ITaskWorkflowService`.
- **Real-time** — `HouseholdHub` + globalni `HouseholdBroadcastFilter` + `wwwroot/js/realtime.js`.

Detaljnije: `Docs/02_Pravila_Programiranja.md` (pravila i konvencije), `Docs/04_Model_Podataka.md` (model podataka).

## Vodič: dodavanje novog modula

Cilj: novi modul se ponaša kao ugrađeni — pojavi se u navigaciji, pretrazi, dashboardu, poštuje vidljivost/dozvole i real-time — **bez ijedne izmjene na postojećim modulima ili Shell-u**.

### Koraci

1. **Model** — `Models/<Modul>/` sa entitetima koji nasljeđuju `BaseEntity` (donosi `Id`, `HouseholdId`, `OwnerId`, `Visibility`, `CreatedAtUtc`, `UpdatedAtUtc`, `IsDeleted`). Za novac koristi `decimal` sa `[Column(TypeName="decimal(18,2)")]`; za datume bez vremena `DateOnly`.
2. **DbContext** (`Data/ApplicationDbContext.cs`) — dodaj `DbSet<T>`, a u `OnModelCreating`: `HasIndex(x => x.HouseholdId)` i relacije/indekse. Zatim migracija:
   `dotnet ef migrations add Add<Modul>` pa `dotnet ef database update`.
3. **(Ako je stavka dijeljiva)** dodaj vrijednost u `ShareableType` enum (`Models/Common/ItemShare.cs`).
4. **Descriptor** — `<Modul>Module : IModuleDescriptor` (`Key`, `Controller`, `Icon`, `SortOrder`, lokalizovan `DisplayName`) + resx par `<Modul>Module.{en,bs}.resx` sa ključem `NavLabel`.
5. **Kontroler** — `[Authorize]`, injektuj `ICurrentHouseholdService`. **Svaki** upit filtriraj po `HouseholdId && !IsDeleted` i pozovi `.VisibleTo(memberId, _context.ItemShares, ShareableType.<X>)`. Pri kreiranju postavi `HouseholdId`, `OwnerId`, `CreatedAtUtc`. Brisanje je **soft** (`IsDeleted = true`, ne pravo brisanje). Sve vrijeme u **UTC**.
6. **View-ovi** — Razor + Bootstrap, `asp-*` tag helperi. Dodaj **prazno stanje** i za široke tabele `.table-responsive`. Lokalizuj kroz `IViewLocalizer` ili `IStringLocalizer<T>` + resx (`.en.resx`/`.bs.resx`, ključevi na engleskom).
7. **(Opciono) Pretraga** — `<Modul>SearchProvider : ISearchable`, poštuj `VisibleTo`.
8. **(Opciono) Dashboard** — `<Modul>DashboardContributor : IDashboardContributor` (+ resx za naslov/prazno).
9. **(Opciono) Kooperacija** — objavi događaj preko `IEventBus` (record u `Models/Events/` koji implementira `IIntegrationEvent`) umjesto direktnog poziva tuđeg servisa; drugi modul reaguje kroz `IEventHandler<T>`. Primjer: račun sa dospijećem → Reminders pravi podsjetnik.
10. **Registracija** (`Program.cs`) — jedna linija po ulozi:
    ```csharp
    builder.Services.AddScoped<IModuleDescriptor, XModule>();
    builder.Services.AddScoped<ISearchable, XSearchProvider>();          // ako ima
    builder.Services.AddScoped<IDashboardContributor, XDashboardContributor>(); // ako ima
    builder.Services.AddScoped<IEventHandler<SomeEvent>, XHandler>();     // ako reaguje
    ```

### Šta MORAŠ imati na umu

- **Multi-tenancy je obavezan:** nijedan upit bez `HouseholdId` filtera — inače podaci cure između domaćinstava.
- **Vidljivost:** uvijek kroz `VisibleTo(...)`; nikad ne vraćaj tuđe privatne stavke.
- **Soft delete:** postavi `IsDeleted`, ne `Remove()` — omogućava oporavak i ne kvari veze.
- **Ne dupliraj zajedničke servise:** e-mail, recurrence, dijeljenje, obavještenja već postoje — koristi ih (`IEmailSender`, `IRecurrenceService`, `IItemSharingService`, `INotificationPreferenceService`).
- **Kooperacija ide preko event bus-a**, ne direktnim pozivom drugog modula (labava sprega).
- **Ne diraj Shell:** navigacija/pretraga/dashboard se generišu iz registra; tvoj modul se samo prijavi.
- **Tajne nikad u repo:** connection string i App Password idu u user-secrets (dev) ili `appsettings.Production.json`/env (prod).
- **Lokalizacija:** ključevi na engleskom, `en` primarni + `bs` prijevod; nedostajući `bs` ključ pada nazad na `en`.
- **Real-time i RBAC dobijaš besplatno:** globalni broadcast filter već javi promjene tvog kontrolera; pristup člana modulu se filtrira u registru.

## Svjesna pojednostavljenja

- **Poziv članova** — owner dodaje člana upisom e-maila (pending član); povezivanje pri registraciji tim e-mailom. Puni invite-token flow je V2.
- **Split expense** — po-članski iznosi (`ExpenseShare`), bez salda "ko kome duguje" i poravnanja (V2).
- **Upload fajlova (Dokumenti)** — samo metapodaci; `FilePath` je hook, upload je V2.
- **Real-time** — ciljani reload relevantne stranice umjesto granularnog DOM patcha; ispunjava "odmah vidljivo svima", patch je V2 optimizacija.
- **Provjera dospjelih podsjetnika** — pri učitavanju dashboarda; u produkciji bi to bio pozadinski job (`IHostedService`/Hangfire).
- **Rich-text bilješke** — obični textarea; formatiranje je V2.

## Šta bi se uradilo sa više vremena

- Puni invite flow sa tokenima i verifikacijom domene za e-mail.
- Granularni real-time (patch elemenata umjesto reload-a) kroz istu SignalR infrastrukturu.
- Pozadinski job za podsjetnike + digest (dnevni/sedmični zbirni pregled).
- Poravnanje troškova između članova i izvještaji.
- Upload i pregled fajlova za dokumente.
- Automatizovani testovi (jedinični za servise, integracioni za tokove).
