# 02 — Pravila programiranja

> **Status: OBAVEZUJUĆI DOKUMENT.**
> Ovo definiše *kako* se piše kod u ovom projektu — arhitektura, imenovanje, struktura foldera, stil, Git konvencije. Prilagođeno je realnom vremenskom budžetu od 4 dana: dovoljno strukturirano da pokaže ozbiljan pristup i lakoću budućeg proširenja, ali bez arhitektonskog "overengineering-a" koji troši vrijeme bez koristi za ovaj rok.

---

## 1. Opšta arhitektura

Stek: **ASP.NET Core MVC** (jedan projekat, server-rendered Razor Views) + **Bootstrap 5** (responzivan dizajn) + **SortableJS** (drag-and-drop) + **FullCalendar** (kalendar prikaz) + **SignalR** (sinhronizacija u realnom vremenu) + **SQL Server / LocalDB** + **Entity Framework Core** + **Gmail SMTP / MailKit** (e-mail) + **Visual Studio** + **GitHub**.

### 1.1 Zašto jedan projekat, a ne razdvojeni slojevi (Clean Architecture na više assembly-ja)

Za dugoročan projekat, razdvajanje na `Domain`/`Application`/`Infrastructure`/`Api` kao posebne class library projekte ima smisla. **Za ovaj 4-dnevni rok to je namjerno pojednostavljeno**: previše vremena ide na postavku projekata i referenci umjesto na funkcionalnost. Umjesto toga:

- **Jedan MVC projekat**, ali sa **jasnom organizacijom po folderima** koja i dalje razdvaja odgovornosti — to je ono što se u kodu vidi i ocjenjuje, ne broj `.csproj` fajlova.
- Ova struktura je namjerno napravljena tako da se **kasnije lako razdvoji** u posebne projekte ako se odluči nastaviti razvoj nakon testa (folderi već prate buduće granice slojeva).

### 1.2 Struktura foldera (unutar jednog projekta)

```
/Models
  /Tasks          → TaskItem.cs, SubTask.cs
  /Reminders      → Reminder.cs
  /Calendar       → Event.cs
  /Notes          → Note.cs
  /Finance        → Transaction.cs, Category.cs, Budget.cs, Bill.cs
  /LifeAdmin      → Document.cs, Contact.cs
  /Kanban         → (nema entiteta — tabla je projekcija Tasks-a po statusu; KanbanModule.cs descriptor)
  /Notifications  → NotificationCategory.cs, MemberNotificationPreference.cs
  /Household      → Household.cs, Member.cs
  /Common         → BaseEntity.cs (Id, HouseholdId, OwnerId, Visibility, CreatedAtUtc), Visibility.cs (Private/Household/SpecificMembers), ItemShare.cs

/Data
  ApplicationDbContext.cs
  /Configurations  → Fluent API konfiguracija po entitetu (opciono, po potrebi)

/Services            ← "core" servisi koje moduli dijele, ne dupliraju
  IEmailSender.cs / GmailEmailSender.cs (MailKit, Gmail SMTP)
  IAppUrlBuilder.cs / AppUrlBuilder.cs  ← apsolutni URL-ovi za linkove u e-mailu
  /Realtime → HouseholdHub.cs, HouseholdBroadcastFilter.cs (SignalR)
  IRecurrenceService.cs / RecurrenceService.cs
  ISearchService.cs / SearchService.cs
  ITaskWorkflowService.cs / TaskWorkflowService.cs      ← promjena statusa + spawn ponavljajućeg (dijele Tasks i Kanban)
  INotificationPreferenceService.cs / ...               ← lične kategorije obavještenja (provjera prije slanja)
  IItemSharingService.cs / ...                          ← dijeljenje stavki sa specifičnim osobama

/Controllers
  TasksController.cs
  RemindersController.cs
  CalendarController.cs
  KanbanController.cs
  ...

/Views
  /Tasks
    Index.cshtml
    Index.en.resx        ← .resx živi direktno uz View kojem pripada (vlasnik: modul Tasks, vidi sekciju 5)
    Index.bs.resx
    Create.cshtml
    Create.en.resx
    Create.bs.resx
  /Reminders
    Index.cshtml
    Index.en.resx        ← vlasnik: modul Reminders, potpuno nezavisno od Tasks
    Index.bs.resx
  /Calendar
  ...
  /Shared
    _Layout.cshtml
    _Layout.en.resx       ← vlasnik: Shell (navigacija, jezički prekidač), ne pripada nijednom modulu
    _Layout.bs.resx
    _QuickCapture.cshtml     ← partial view, koristi se sa svakog ekrana

/wwwroot
  /css
  /js
```

**Pravilo:** svaki modul iz specifikacije ima svoj folder unutar `/Models`, `/Controllers`, `/Views`. Novi modul (kad-tad, van ovog roka) se dodaje kao novi folder na istom nivou — ovo je direktna, jednostavnija primjena principa "nove aplikacije su ravnopravni građani" iz specifikacije, prilagođena jednom projektu.

### 1.3 Core servisi — ne duplicirati po modulu

Sljedeće **uvijek** živi u `/Services` i moduli ga samo pozivaju:
- Provjera vidljivosti/pristupa — `VisibleTo(memberId)` extension nad `IQueryable<BaseEntity>` (dom + vlastito). Za dijeljenje sa specifičnim osobama postoji preopterećenje `VisibleTo(memberId, shares, type)` koje kroz `EXISTS` podupit dodaje i stavke iz `ItemShare` (bez dodatnog round-tripa). Pisanje dijeljenja ide kroz `IItemSharingService`.
- `IRecurrenceService` — ponavljanje (koriste ga i Zadaci, i Kalendar, i Podsjetnici, i Finance/Bill).
- `ITaskWorkflowService` — promjena statusa zadatka + spawn sljedeće instance ponavljajućeg zadatka; dijele ga `TasksController` i `KanbanController` da logika "Done → sljedeća instanca" ne postoji na dva mjesta.
- `INotificationPreferenceService` — jedno mjesto koje svaki kanal obavještenja pita smije li slati (lične kategorije, opt-out). Provjerava se u `ReminderNotificationService` i `TaskAssignedEmailHandler`.
- `IEmailSender` — Gmail SMTP integracija (MailKit); koristi je Reminders, Finance (upozorenja o računima), obavještenja o dodijeljenom zadatku. Provajder se mijenja jednom klasom (nova implementacija `IEmailSender`), bez ijedne izmjene u pozivaocima — kako je i urađeno pri prelasku Resend → Gmail.

Ako se tokom rada primijeti potreba za "još jednim recurrence rješenjem" u drugom modulu — to je znak da treba iskoristiti postojeći `IRecurrenceService`, ne pisati novi. Isto vrijedi za dijeljenje (`IItemSharingService`) i provjeru obavještenja (`INotificationPreferenceService`) — mehanizam se pruža jednom, moduli ga samo koriste.

### 1.4 Pretraga — moduli se sami prijavljuju (dopuna, odlučeno tokom Dana 2/3)

Za razliku od "retrofit" pristupa (jedan `SearchService` koji na kraju direktno upituje fiksnu listu tabela), pretraga je riješena tako da **svaki modul sam sebe registruje**, u skladu s principom iz `00_Specifikacija_Izvor.md` ("nova aplikacija je automatski vidljiva ... u pretrazi" bez da Shell unaprijed zna za nju):

- `ISearchable` (u `/Services`) — ugovor koji svaki modul implementira: `Task<List<SearchResult>> SearchAsync(int householdId, string query)`.
- `SearchResult` — `record(string ModuleName, string Title, string? Snippet, string Url)`. `ModuleName` je stabilan identifikator (npr. `"Tasks"`), **ne** korisnički tekst — UI za pretragu (Dan 4) ga mapira na lokalizovan naziv, isto kao što se enum vrijednosti mapiraju preko resx.
- Provider klasa živi **uz modul koji pretražuje** (npr. `Models/Tasks/TaskSearchProvider.cs`), ne u `/Services` — modul je vlasnik svoje pretrage, isto kao što je vlasnik svojih entiteta.
- Registracija u `Program.cs`: `AddScoped<ISearchable, XSearchProvider>()` — više registracija istog interfejsa je namjerno, `SearchService` (Dan 4) ih dobija sve odjednom kroz `IEnumerable<ISearchable>` i ne treba listu modula unaprijed.
- Kad se dodaje novi modul (Notes, ShoppingLists, Finance, LifeAdmin) — dodaje se i njegov `ISearchable` provider **u istom commit-u** kad se modul pravi, isto kao lokalizacija (vidi sekciju 5.3) — ne čeka se Dan 4.
- `SearchService` (agregator), search traka u navbaru i stranica s rezultatima (`/Search`) su **implementirani ranije** (pomjereno sa Dana 4), jer je pretraga cross-cutting sposobnost koju spec traži na nivou sistema. `SearchService` agregira sve registrovane `ISearchable` i **izbacuje rezultate modula koji su trenutno isključeni** (vidi 1.5) — ne zna ništa o pojedinačnim modulima.

### 1.5 Registar modula — navigacija/pretraga/dashboard se generišu, ne hardkodiraju (dopuna)

Ovo je direktna primjena principa "Nove aplikacije su ravnopravni građani" iz `00_Specifikacija_Izvor.md` (automatski vidljiva u navigaciji i pretrazi, bez izmjena na postojećim modulima; instalacija/uklanjanje čisti i reverzibilni):

- `IModuleDescriptor` (u `/Services`) — svaki modul opisuje sam sebe: `Key` (stabilan id, npr. `"Tasks"`, poklapa se sa `SearchResult.ModuleName`), `Controller`, `Icon` (emoji), `SortOrder`, `DisplayName` (lokalizovan, iz **modulovog vlastitog** resx-a). Descriptor klasa živi uz modul (`Models/Tasks/TasksModule.cs`) i duplo služi kao lokalizacioni marker (`TasksModule.{culture}.resx`, ključ `NavLabel`).
- `IModuleRegistry` / `ModuleRegistry` (u `/Services`) — agregira sve registrovane descriptor-e (`IEnumerable<IModuleDescriptor>`) sa upisanim stanjem uključenosti. `GetEnabledAsync` koristi navbar i pretraga; `GetAllAsync` koristi stranica za upravljanje.
- `ModuleState` entitet (`Models/Modules/`) — uključenost po domaćinstvu. Nepostojanje reda = modul uključen (default); red sa `IsEnabled=false` ga čisto skriva iz navigacije/pretrage, **bez brisanja podataka** ("uklanjanje reverzibilno").
- **Navbar, komandna paleta i pretraga se generišu** iz `GetEnabledAsync` — nema hardkodirane liste modula. Sama "Danas" stranica ostaje Shell home (ne može se deinstalirati jer agregira ostale module), ali se **njen sadržaj** generiše iz modula (vidi 1.6).
- **Stranica `/Modules`** (Shell-ov `ModulesController`) — uključi/isključi svaki modul + pregled/opoziv dozvola (vidi 1.9); lista se generiše iz registra pa raste automatski kad se doda novi modul.
- Kad se dodaje novi modul: doda se `IModuleDescriptor` (+ `NavLabel` resx) i registruje `AddScoped<IModuleDescriptor, XModule>()` u `Program.cs`, **u istom commit-u** kad se modul pravi. Time se automatski pojavljuje u navbaru, komandnoj paleti, na `/Modules` stranici, i (ako ima `ISearchable`/`IDashboardContributor`) u pretrazi/dashboardu — bez ijedne izmjene Shell-a.
- Runtime učitavanje zasebnih DLL-ova (pravi plugin sistem) je **svjesno van obima** za ovaj rok — moduli su kompajlirani u jedan projekat, ali se *deklarišu* sami umjesto da su ušiveni u Shell. Ovo demonstrira princip proširivosti bez rizika/vremena punog plugin sistema.

### 1.6 Dashboard se sastavlja iz doprinosa modula (dopuna)

- `IDashboardContributor` (u `/Services`) — svaki modul doprinosi svoju "Danas" sekciju: `ModuleKey`, `SortOrder`, `BuildAsync(householdId, memberId)` vraća `DashboardWidget` (naslov, prazan-tekst, lista `DashboardItem`). Contributor živi uz modul (`Models/Tasks/TasksDashboardContributor.cs`) sa vlastitim resx-om za naslov/prazno stanje.
- `HomeController.Index` agregira sve contributor-e **filtrirane po uključenim modulima** iz registra — isključen modul nestaje i sa dashboarda, ne samo iz navigacije/pretrage.
- Novi modul dobija sekciju na dashboardu čim registruje `AddScoped<IDashboardContributor, XDashboardContributor>()` — bez izmjene Shell-a.

### 1.7 Komandna paleta (Ctrl+K)

- Overlay u `_Layout.cshtml` + logika u `wwwroot/js/site.js` (Shell ponašanje). Ciljevi (moduli za skok) generišu se iz istog registra (`navModules`) serijalizovanog u JSON — novi modul se automatski pojavi. Uz skok na modul, paleta uvijek nudi i punu pretragu za upisani tekst.

### 1.8 Kooperacija bez direktne zavisnosti — event bus (dopuna)

- `IEventBus` / `InProcessEventBus` + `IEventHandler<TEvent>` / `IIntegrationEvent` (u `/Services/Events`). Moduli objavljuju "ključne trenutke", drugi reaguju kroz DI-registrovane handler-e — bez direktnog poziva tuđeg servisa.
- Event-ugovori žive u modul-neutralnom `Models/Events/` da pretplatnik reaguje na ugovor, ne na modul koji objavljuje.
- Konkretno: Tasks objavljuje `TaskWithDueDateCreatedEvent`; Reminders reaguje (`TaskWithDueDateCreatedHandler`) i automatski kreira podsjetnik na datum roka — istovremeno demonstrira "gradi na postojećem" i sjeme "ako ovo, onda ono".
- Drugi primjer: Tasks objavljuje `TaskAssignedEvent` kad je zadatak dodijeljen drugom članu; `TaskAssignedEmailHandler` reaguje i šalje e-mail dodijeljenom (poštujući lične kategorije obavještenja). Tasks nikad ne zove e-mail servis direktno.
- Nova pretplata = jedna `AddScoped<IEventHandler<T>, XHandler>()` linija. Bus izoluje greške pojedinačnog handler-a (jedan modul ne ruši druge).
- Za produkciju: prelazak na MediatR/pravi message bus bez izmjene ugovora (vidi sekciju 9).
- **Real-time (implementirano):** SignalR `HouseholdHub` + globalni `HouseholdBroadcastFilter` emituju promjene grupi po `HouseholdId`, a `wwwroot/js/realtime.js` osvježava relevantne ekrane bez reload-a. Filter je namjerno **jedna tačka** presretanja svih uspješnih POST-ova umjesto ručnog objavljivanja iz svakog kontrolera — vidi `01_Roadmap.md`, "Sinhronizacija u realnom vremenu".

### 1.9 Kontrola i privatnost — dozvole po modulu (dopuna)

- `IModuleDescriptor.RequestedPermissions` — modul deklariše koje **tuđe** podatke traži (`ModulePermission(Key, DisplayName)`); većina modula ne traži ništa (default prazna lista preko default interface implementacije).
- `IPermissionService` / `PermissionService` — `HasPermissionAsync` provjerava prije cross-modul pristupa; `ModulePermissionState` (`Models/Modules/`) pamti grant/opoziv po domaćinstvu (nepostojanje reda = dato po defaultu za ugrađene module).
- **Provođenje**: `CalendarController.Events` čita Tasks samo ako je `Calendar → Tasks.Read` dozvola data; opoziv stvarno prekida pristup (rokovi zadataka nestaju s kalendara, događaji ostaju).
- **Pregled/opoziv**: `/Modules` stranica prikazuje tražene dozvole svakog modula sa Daj/Opozovi dugmadima.
- Svjesno pojednostavljenje: default je **granted** za ugrađene (povjerljive) module radi glatkog prvog pokretanja; stroži default-deny za module treće strane je prirodno proširenje. Puno provođenje kroz svaku putanju pristupa je tačka proširenja — trenutno je provedeno na stvarnoj cross-modul putanji (Kalendar→Tasks) kao dokaz mehanizma.

**Dva odvojena sloja pristupa (bitno ne pomiješati):**
1. **Modul → tuđi podaci** (`ModulePermissionState`, nivo domaćinstva) — *ovo je gore opisano i napravljeno.* Odgovara na: "smije li modul X čitati podatke modula Y". Vezano za spec "aplikacija dobija pristup podacima za koje je dobila dozvolu".
2. **Član → modul** (`MemberModuleAccess`, nivo osobe) — **planirano za Dan 3** uz upravljanje članovima (dogovoreno na prelazu Dan 2/3). Odgovara na: "smije li član Amina otvoriti/vidjeti modul Kalendar". Owner domaćinstva dodjeljuje. `IModuleRegistry.GetEnabledAsync` će filtrirati i po pristupu trenutnog člana (modul vidljiv ako je uključen za domaćinstvo **I** dozvoljen članu). Ovo je RBAC po članu i ne postoji dok se ne otvore članovi — vidi `01_Roadmap.md`, sekcija 3.4.

---

## 2. Imenovanje i stil (C# / .NET)

- **PascalCase** za klase, metode, javna svojstva, fajlove (`TasksController.cs`).
- **camelCase** za lokalne varijable, privatna polja sa `_` prefiksom (`_context`).
- **Async svuda gdje se dodiruje baza** — sufiks `Async` (`GetTasksForTodayAsync`).
- **ViewModel-i, ne direktno entiteti, u Views** — kontroleri mapiraju entitet u jednostavan ViewModel prije slanja u View (čak i ručno mapiranje je dovoljno za ovaj obim, ne treba AutoMapper ako oduzima vrijeme).
- **Nullable reference types uključeni** — eksplicitno rukovanje null vrijednostima.
- **Validacija** kroz Data Annotations na modelima (`[Required]`, `[MaxLength]`) — dovoljno za ovaj rok, brže od FluentValidation-a za postavku.
- **Konfiguracija (Gmail SMTP App Password, connection string) kroz `appsettings.json` + User Secrets**, čitano preko `IConfiguration`/`IOptions<T>` u servisima — ne hardkodirati nigdje u kodu.

### 2.1 MVC konvencije
- Standardni REST-ish routing: `/Tasks`, `/Tasks/Create`, `/Tasks/Edit/{id}`, `/Tasks/Delete/{id}`.
- AJAX endpoint-i (Kanban drag-drop, shopping list checkbox) vraćaju `JsonResult` sa minimalnim payload-om (`{ success: true }` ili sličan), ne cijeli HTML.
- Svaki kontroler eksplicitno provjerava da entitet pripada trenutnom `HouseholdId` prije prikaza/izmjene — ovo se radi kroz zajedničku helper metodu ili action filter, ne ručno kopirano u svakom action-u.

---

## 3. Baza podataka (SQL Server / LocalDB + EF Core)

- Migracije za svaku promjenu šeme (`Add-Migration <Opis>` u Package Manager Console, ili `dotnet ef migrations add`).
- Svaki entitet koji predstavlja korisnički sadržaj nasljeđuje/sadrži: `Id` (Guid ili int — int je brži za postavku i dovoljan za test), `HouseholdId`, `OwnerId`, `Visibility`, `CreatedAtUtc`.
- Nazivi tabela u engleskom, PascalCase, množina (`Tasks`, `Reminders`).
- LocalDB je dovoljan za test — nema potrebe za punim SQL Server instancom, ovo štedi vrijeme na postavci.

---

## 4. Frontend (Razor Views + Bootstrap 5 + malo JS-a)

### 4.1 Moderan, responzivan dizajn — pravila koja važe od prvog ekrana

- **Mobile-first pristup** kroz Bootstrap grid (`col-12 col-md-6 col-lg-4` i sl.) — ne dodavati responzivnost naknadno.
- **Zajednički Bootstrap layout** (`_Layout.cshtml`) sa navbar-om koji sadrži i Quick Capture dugme — koristi se na svakoj stranici, ne pravi se poseban layout po modulu.
- **Partial Views** za ponavljajuće komponente (npr. `_TaskCard.cshtml`, `_QuickCapture.cshtml`) — izbjeći kopiranje istog HTML-a u više View-ova.
- **Konzistentna paleta boja** definisana kroz Bootstrap CSS varijable u jednom custom CSS fajlu (`wwwroot/css/site.css`), ne ad-hoc inline stilovi po stranici.
- **Svaki novi ekran provjeren na 3 širine** (mobilni/tablet/desktop) prije nego što se stavka u roadmap-u označi završenom.
- **Prazna stanja i loading indikatori** dio svakog ekrana od početka.

### 4.2 JavaScript — minimalno i svrsishodno
- Čist JS (ili jQuery, koji dolazi ugrađen sa ASP.NET Core MVC template-om) za AJAX pozive — nema potrebe za frontend frameworkom.
- **SortableJS** (CDN) isključivo za Kanban drag-and-drop.
- **FullCalendar** (CDN) isključivo za Kalendar prikaz.
- JS kod po modulu ide u poseban fajl (`wwwroot/js/tasks.js`, `wwwroot/js/kanban.js`) — ne sve u jednom globalnom `site.js`.

---

## 5. Lokalizacija (internacionalizacija / i18n)

**Princip:** Engleski je primarni jezik razvoja — sav kod, imenovanje (klase, metode, promjenljive, commit poruke, baza podataka) je i ostaje na engleskom, bez izuzetka. Ono što se lokalizuje je **isključivo korisnički interfejs** (tekst koji korisnik vidi u browseru).

**Podjela odgovornosti (ovo je arhitektonsko pravilo, ne samo organizacija fajlova):**
- **Svaki modul lokalizuje sam sebe.** Prijevodi za tekst koji pripada Zadacima žive uz Zadatke, prijevodi za Podsjetnike žive uz Podsjetnike, itd. Modul je vlasnik svojih `.resx` fajlova, isto kao što je vlasnik svojih entiteta i kontrolera.
- **"Glavna" aplikacija (Shell/Core) radi samo jednu stvar u vezi jezika: nudi mehanizam biranja jezika** — dropdown u navbaru, čuvanje odabrane kulture (cookie), fallback logiku i listu podržanih kultura. Shell **ne sadrži prijevode sadržaja modula** i ne zna unaprijed koji će moduli postojati — on samo omogućava da bilo koji instalirani modul, ako ima svoje `.resx` fajlove, ispravno radi u odabranom jeziku.
- Ovo je direktna primjena istog principa kao i za ostale zajedničke sposobnosti iz specifikacije (podsjetnici, obavještenja, e-mail, članovi) — **platforma pruža mehanizam jednom, a sadržaj/logiku pruža svaki modul za sebe.** Jezički prekidač je zajednička sposobnost; sami prijevodi nisu.
- **Posljedica za proširivost:** kad se doda novi modul (Faza 4 iz duže verzije roadmapa, ili bilo koji budući modul), on donosi svoje `.resx` fajlove i time automatski postaje dvojezičan (ili višejezičan) — bez potrebe da se Shell mijenja ili da "zna" za novi modul unaprijed. Ako novi modul svoje `.resx` fajlove ne donese, aplikacija i dalje radi ispravno — taj modul se jednostavno prikazuje na engleskom (fallback), u skladu sa principom "aplikacije moraju funkcionisati stabilno i kada nešto očekivano nije prisutno".

### 5.1 Šta ostaje na engleskom (uvijek, bez izuzetka)
- Nazivi klasa, metoda, promjenljivih, foldera, fajlova, tabela i kolona u bazi.
- Commit poruke (već definisano u sekciji 6).
- Komentari u kodu.
- Interni logovi/greške namijenjene developeru (ne korisniku).

### 5.2 Šta se lokalizuje (prevodi se)
- Sav tekst koji korisnik vidi: labele, dugmad, naslovi stranica, poruke o greškama/uspjehu prikazane korisniku, e-mail sadržaj koji se šalje.
- Izuzetak su tekstovi koji pripadaju samom Shell-u (npr. naziv aplikacije u navbaru, sam naziv jezičkog prekidača) — oni se lokalizuju uz Shell, ne uz neki modul, jer ne pripadaju nijednom pojedinačnom modulu.

### 5.3 Struktura resursa — decentralizovano po modulu

```
/Resources
  /Shell                        ← lokalizacija samo za layout/navigaciju, vlasnik: glavna aplikacija
    SharedLayout.en.resx
    SharedLayout.bs.resx

/Models, /Controllers, /Views su već organizovani po modulu (sekcija 1.2) — .resx fajlovi
prate isti princip, žive uz Views svakog modula:

/Views
  /Tasks
    Index.cshtml
    Index.en.resx               ← vlasnik: modul Tasks
    Index.bs.resx
    Create.en.resx
    Create.bs.resx
  /Reminders
    Index.cshtml
    Index.en.resx                ← vlasnik: modul Reminders
    Index.bs.resx
  ...
```

**Pravilo:** kad se piše novi ekran unutar modula, `.resx` fajlovi za taj ekran se dodaju **odmah uz njega**, u istom commit-u — lokalizacija nije poseban, odvojen zadatak "za na kraju", nego sastavni dio završetka svakog ekrana (isto kao responzivnost u sekciji 4.1).

### 5.4 Mehanizam — ASP.NET Core ugrađena lokalizacija (bez dodatnih biblioteka)
- Koristiti ugrađeni `IStringLocalizer<T>` / `IViewLocalizer` mehanizam iz `Microsoft.Extensions.Localization` — dio je ASP.NET Core-a, ne treba dodatna zavisnost. `IViewLocalizer` u Razor View-u automatski gleda `.resx` fajl uz taj View — ovo je upravo mehanizam koji omogućava da lokalizacija "prirodno" bude po modulu, bez dodatne konfiguracije po modulu.
- Sufiks kulture na `.resx` fajlu: `.en.resx` (primarni/fallback), `.bs.resx` (bosanski). Novi jezik kasnije = samo novi `.resx` fajl sa istim ključevima uz postojeći ekran, bez izmjene koda ili Views-a.
- Ključevi u `.resx` fajlovima su na engleskom i opisni (npr. `TaskDueDate`, `SaveButton`, `TaskCreatedSuccessMessage`) — ključ je identifikator, ne prijevod, i ne mijenja se kad se dodaje novi jezik.
- U `Program.cs` (dio glavne aplikacije/Shell-a, ne modula) konfigurisati podržane kulture i samu lokalizacionu infrastrukturu od početka:
  ```csharp
  var supportedCultures = new[] { "en", "bs" }; // budući jezici se dodaju ovdje, na jednom mjestu
  ```
- Fallback kultura je **uvijek engleski** — ako prijevod za neki ključ nedostaje u `.bs.resx` (ili budućem jeziku) bilo kojeg modula, prikazuje se engleska verzija tog modula, ne prazan tekst ili greška. Ovo je posebno bitno jer moduli mogu biti lokalizovani neravnomjerno (jedan modul potpuno, drugi djelimično) i sistem to mora tiho podnijeti.

### 5.5 Izbor jezika u UI-ju (jedina stvar koju "glavna" aplikacija radi)
- Jednostavan dropdown/prekidač u navbaru (`_Layout.cshtml`, dio Shell-a) sa dostupnim jezicima — za sada Engleski/Bosanski, raste automatski čim bilo koji modul dobije novi `.resx` jezik.
- Odabrani jezik čuva se u cookie-ju (standardni ASP.NET Core `CookieRequestCultureProvider`) — pamti se između posjeta, bez potrebe za korisničkim nalogom da bi radilo, i važi globalno za cijelu aplikaciju (jedan izbor jezika, primjenjuje se na sve module odjednom).
- Promjena jezika ne zahtijeva ponovno pokretanje aplikacije niti gubitak trenutne stranice — redirect na istu stranicu sa novom kulturom.
- Shell ne provjerava i ne treba da zna koji moduli imaju, a koji nemaju prijevod za odabrani jezik — to je odgovornost svakog modula pojedinačno (uz fallback iz 5.4).

### 5.6 Praktična napomena za obim ovog testa
- Prioritet: mehanizam (jezički prekidač u Shell-u) mora **raditi i biti vidljiv**, i barem 1-2 modula (npr. Tasks, Dashboard) treba da imaju kompletne `.bs.resx` prijevode kao dokaz da decentralizovani pristup radi u praksi — ne mora svaki modul biti preveden do zadnjeg dana.
- U README-u (Dan 4, sekcija 4.6 u `01_Roadmap.md`) jasno navesti: jezički mehanizam je centralan i radi za cijelu aplikaciju, dok prijevodi po modulu postoje tamo gdje je bilo vremena, a ostali moduli koriste engleski fallback — i objasniti da bi dodavanje prijevoda za preostale module bio čisto dopunski rad (novi `.resx` fajlovi), bez ijedne izmjene arhitekture.

## 6. Git i GitHub

### 5.1 Konvencija commit poruka
Format: `<tip>(<modul>): <kratak opis>`

Tipovi: `feat`, `fix`, `refactor`, `docs`, `chore`, `style`.

Primjeri:
```
feat(tasks): CRUD za zadatke sa podzadacima
feat(reminders): Resend integracija za e-mail obavještenja
style: responsive polish za mobilni prikaz
docs: README sa uputstvom za pokretanje
```

### 5.2 Kada komitovati — ovo je eksplicitno naglašeno u roadmap-u
- **Odmah nakon prvog uspješnog build-a prazne aplikacije** — prvi commit i push, prije bilo koje funkcionalnosti. Ovo je prva stavka u `01_Roadmap.md` (Dan 1, sekcija 1.1) i ne preskače se.
- Nakon svake značajnije završene funkcionalnosti (svaki modul, svaka core komponenta) — mali, česti commit-ovi > jedan veliki na kraju.
- Grananje nije neophodno za ovaj 4-dnevni solo rad — rad direktno na `main` je u redu ako se komituje disciplinovano i često; ako se želi dodatna sigurnost, jedna `feature/` grana po danu je dovoljna.

### 5.3 Tajne — nikad u Gitu
- `appsettings.Development.json` (connection string) — u `.gitignore`.
- Gmail App Password (`Smtp:AppPassword`) — isključivo kroz .NET User Secrets, nikad u fajlu koji ide u repo.
- U repozitoriju ostaje `appsettings.json` sa praznim placeholder vrijednostima za `Smtp` (i `App:BaseUrl`), da je jasno šta treba popuniti pri pokretanju (ovo se navodi i u README-u).

---

## 7. Testiranje

Za ovaj rok, testiranje se svodi na **ručnu provjeru svakog toka** (nema vremena za punu test pokrivenost), ali:
- Ako vrijeme dozvoli (npr. na kraju Dana 4 ako je sve gotovo ranije), 2-3 osnovna unit testa za `IRecurrenceService` pokazuju da je testiranje razmišljano, čak i bez potpune pokrivenosti.
- Prioritet je funkcionalna ispravnost provjerena ručno kroz demo scenario, ne formalni test paket.

---

## 8. Kada se ovo pravilo i specifikacija ne slažu

Ako implementacija zahtijeva odluku koje ovdje nema, a tiče se *stila/strukture koda* — donosi se odluka i **dopisuje se ovdje**. Ako se tiče *funkcionalnosti* — vraćamo se na `00_Specifikacija_Izvor.md`. Ako se tiče *redoslijeda/vremena* — `01_Roadmap.md`.

## 9. Napomena o budućem proširenju (nakon testa)

Ako se projekat nastavi razvijati nakon test roka, prirodni sljedeći koraci ka arhitekturi iz duže verzije ovog dokumenta (koja je razmatrana prije promjene roka):
- Razdvajanje `/Services` i `/Models` u posebne class library projekte (`HomeOS.Application`, `HomeOS.Domain`).
- Uvođenje pravog event bus-a (npr. MediatR notifications) umjesto direktnih poziva servisa, radi pune primjene principa "kooperacija bez direktne zavisnosti".
- Razmatranje odvojenog SPA frontenda (React) **samo** ako produkt zaista zahtijeva bogatu klijentsku interaktivnost koju Razor Views + malo JS-a ne mogu pokriti — do tada, server-rendered pristup je i brži za razvoj i lakši za održavanje za solo developera.
