# Home OS

Privatni "kućni operativni sistem" — jedna aplikacija koja okuplja cjelokupnu administraciju domaćinstva (zadaci, kalendar, podsjetnici, kanban, bilješke, liste za kupovinu, finansije, kućna administracija) na jedno mjesto, dijeljeno među članovima doma, uz e-mail obavještenja i sinhronizaciju u realnom vremenu. Sve je povezano i zamišljeno kao **platforma**: novi modul se dodaje kao ravnopravan građanin, bez izmjena postojećih.

## Tehnološki stek

- **ASP.NET Core 8 MVC** (server-rendered Razor Views), jedan projekat
- **Entity Framework Core 8** (Code First, migracije) + **SQL Server LocalDB**
- **ASP.NET Core Identity** (autentikacija, e-mail kao korisničko ime)
- **Bootstrap 5** (responzivan dizajn), **SortableJS** (kanban drag-and-drop), **FullCalendar** (kalendar)
- **SignalR** (sinhronizacija u realnom vremenu)
- **MailKit / Gmail SMTP** (e-mail obavještenja)
- **Lokalizacija** (`en` primarni, `bs`) preko `IStringLocalizer`/`IViewLocalizer` + `.resx`

## Pokretanje

### Preduslovi
- .NET 8 SDK
- SQL Server LocalDB (dolazi uz Visual Studio; instanca `(localdb)\mssqllocaldb`)

### Koraci
1. **Connection string** — `appsettings.Development.json` nije u Gitu; kreirajte ga u `HomeOS/HomeOS/` sa:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HomeOS;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```
2. **Primijenite migracije** (kreira bazu):
   ```bash
   cd HomeOS/HomeOS
   dotnet ef database update
   ```
3. **E-mail (Gmail SMTP)** — opciono, ali potrebno da bi obavještenja stvarno stizala. Na Google nalogu uključite 2-Step Verification i napravite **App Password**, pa ga unesite kroz .NET User Secrets (nikad u fajl koji ide u repo):
   ```bash
   dotnet user-secrets set "Smtp:FromEmail" "vasaadresa@gmail.com"
   dotnet user-secrets set "Smtp:AppPassword" "16-znakovni-app-password"
   ```
   Bez ovoga aplikacija radi normalno — slanje e-maila se samo tiho preskače.
4. **Pokrenite**:
   ```bash
   dotnet run
   ```
   Prvi registrovani korisnik kreira svoje domaćinstvo i postaje **owner**.

## Implementirano po modulima

| Modul | Sažetak |
|---|---|
| **Dashboard ("Danas")** | Agregira doprinose svih uključenih modula (dospjeli zadaci, današnji događaji, aktivni podsjetnici, predstojeći računi, dokumenti koji ističu). Quick Capture (zadatak/bilješka/podsjetnik) sa bilo kog ekrana. Univerzalna pretraga i komandna paleta (Ctrl+K). |
| **Zadaci** | CRUD, podzadaci, tagovi, prioritet/status/rok, filter i sort, indikator prekoračenog roka, ponavljajući zadaci (prebacivanje u Done spawn-uje sljedeću instancu), dodjela članu + e-mail obavještenje. |
| **Kalendar** | FullCalendar prikaz događaja; rokovi zadataka se projektuju read-only (uz dozvolu modula). |
| **Podsjetnici** | CRUD, jednokratni/ponavljajući (resolve spawn-uje sljedeći), više primalaca, snooze, e-mail + in-app obavještenje. |
| **Kanban** | Jedna tabla koja se **automatski formira od zadataka** (kolone = status); drag mijenja status zadatka. Nema vlastitih podataka. |
| **Bilješke** | CRUD, tagovi (dijeljeni sa zadacima), dnevnik (jedan zapis po danu po članu), povezivanje sa zadatkom/događajem. |
| **Liste za kupovinu** | CRUD lista i stavki, čekiranje stavki bez reload-a (AJAX). |
| **Finansije** | Transakcije po kategoriji, mjesečni budžet po kategoriji (progress bar), prihodi/troškovi/neto sažetak, računi sa dospijećem (auto-podsjetnik preko event bus-a), split expense po članu ("podijeli jednako" ili proizvoljno). |
| **Kućna administracija** | Dokumenti (metapodaci + datum isteka → auto-podsjetnik za obnovu) i kontakti. |

**Zajedničke sposobnosti** kroz sve module: vidljivost stavki (privatno / cijelo domaćinstvo / specifične osobe), dodjela članovima, članovi domaćinstva sa owner ulogom i pozivom po e-mailu, RBAC po članu (owner ograničava pristup modulima), individualna podešavanja kategorija obavještenja, lokalizacija (EN/BS), sinhronizacija u realnom vremenu.

## Arhitektura i proširivost

Sistem je platforma; Shell (navbar, pretraga, dashboard, komandna paleta, upravljanje modulima) se **generiše iz registra**, nikad hardkodiran. Dodavanje modula = dodavanje njegovih fajlova + par DI linija u `Program.cs`, bez izmjena na postojećim modulima.

- **Registar modula** — `IModuleDescriptor` (svaki modul se sam opisuje) + `IModuleRegistry` + `ModuleState` (uključi/isključi po domaćinstvu).
- **Univerzalna pretraga** — svaki modul prijavljuje `ISearchable` provider.
- **Dashboard** — svaki modul doprinosi `IDashboardContributor` sekciju.
- **Event bus** — `IEventBus`/`IEventHandler` za kooperaciju bez direktne zavisnosti ("ako ovo, onda ono"): npr. zadatak/račun/dokument sa datumom → Reminders reaguje; dodijeljen zadatak → e-mail handler; sve promjene → SignalR broadcast.
- **Dozvole** — dva odvojena sloja: dozvola *modula na tuđe podatke* (`IPermissionService`/`ModulePermissionState`, npr. Kalendar→Tasks) i pristup *člana modulu* (`IMemberAccessService`/`MemberModuleAccess`).
- **Vidljivost** — `Visibility` (Private/Household/SpecificMembers) + `ItemShare`; jedan `VisibleTo(...)` helper primijenjen dosljedno u svim upitima.
- **Zajednički core servisi** — `IEmailSender` (Gmail SMTP), `IRecurrenceService`, `INotificationPreferenceService`, `IItemSharingService`, `IAppUrlBuilder` — mehanizam se pruža jednom, moduli ga koriste.
- **Real-time** — `HouseholdHub` (grupa po domaćinstvu) + globalni `HouseholdBroadcastFilter` (svaki uspješan POST javi grupi šta se promijenilo) + `wwwroot/js/realtime.js` (osvježi relevantnu stranicu). Event bus je tačka priključka.

Detaljnije: `Docs/02_Pravila_Programiranja.md` (pravila i konvencije), `Docs/04_Model_Podataka.md` (model podataka).

## Svjesna pojednostavljenja (i zašto)

- **Poziv članova** — owner dodaje člana upisom e-maila (pending član); povezivanje pri registraciji tim e-mailom. Slanje same pozivnice e-mailom je pripremljeno; puni invite-token flow je V2.
- **Split expense** — po-članski iznosi (`ExpenseShare`), bez "ko kome duguje" salda i poravnanja (računa se agregacijom; poravnanje je V2).
- **Upload fajlova (Dokumenti)** — samo metapodaci; `FilePath` je pripremljen hook, sam upload je V2.
- **Real-time** — klijent radi ciljani reload relevantne stranice umjesto granularnog DOM patcha; u potpunosti ispunjava "odmah vidljivo svima", a granularno ažuriranje je optimizacija za V2.
- **Provjera dospjelih podsjetnika** radi se pri učitavanju dashboarda; u produkciji bi to bio pozadinski job (`IHostedService`/Hangfire).
- **Rich-text bilješke** — obični textarea; formatiranje je V2.

## Šta bi se uradilo sa više vremena

- Puni invite flow sa tokenima i verifikacijom domene za e-mail.
- Granularni real-time (patch pojedinačnih elemenata umjesto reload-a) kroz istu SignalR infrastrukturu.
- Pozadinski job za podsjetnike + digest (dnevni/sedmični zbirni pregled).
- Poravnanje troškova između članova i izvještaji.
- Upload i pregled fajlova za dokumente.
- Automatizovani testovi (jedinični za `IRecurrenceService`/servise, integracioni za tokove).
