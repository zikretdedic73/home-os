# 01 — Roadmap (4-dnevni test rok)

> **Status: RADNI DOKUMENT — ažurira se tokom razvoja.**
> Ovo NIJE dugoročni roadmap za mjesece razvoja — ovo je **konkretan plan za 4 dana** (3 uzastopna radna dana + nedjelja), sa ciljem pune funkcionalnosti svih 8 modula iz specifikacije, prilagođen realnom vremenskom budžetu i MVC arhitekturi.
>
> Kada je aktivnost završena, označi je (`[x]`). Ako nešto iz `00_Specifikacija_Izvor.md` nije jasno tokom neke aktivnosti — prvo se vrati tom dokumentu prije proizvoljne odluke.

**Tehnički kontekst:** ASP.NET Core **MVC/Razor Pages** (C#, jedan projekat) + SQL Server (LocalDB) + Entity Framework Core, razvoj u Visual Studio, Bootstrap 5 za responzivan dizajn, SortableJS za drag-and-drop, **Gmail SMTP (MailKit) za e-mail**, SignalR za sinhronizaciju u realnom vremenu, GitHub za verzionisanje. *(E-mail je prvobitno bio Resend; prebačen na Gmail SMTP u reviziji nakon Dana 3 — vidi sekciju "Revizija".)*

**Budžet vremena:** ~6-8h po danu × 4 dana = **24-32h ukupno**, od čega je dio Dana 4 rezervisan za polish, README i pripremu demoa.

---

## Dan 1 (radni dan) — Postavka projekta + Zadaci

### 1.1 Postavka projekta (prioritet — radi se prvo, prije bilo kakve funkcionalnosti)
- [x] Kreirati ASP.NET Core MVC projekat u Visual Studiju (najnovija LTS verzija .NET-a).
- [x] Kreirati privatni GitHub repozitorij i povezati ga sa projektom (Visual Studio → Git Changes → Create Git Repository).
- [x] Provjeriti `.gitignore` (bin/obj/appsettings.Development.json isključeni).
- [x] Prvi build projekta (prazan, ali se pokreće) — provjeriti da radi.
- [x] 🔧 **KOMIT I PUSH NA GITHUB ODMAH NAKON PRVOG USPJEŠNOG BUILD-A** — ovo je eksplicitno tražena stavka, ne preskočiti i ne ostavljati za kasnije. Poruka: `chore: initial project setup`.
- [x] Bootstrap 5 layout (navbar, responsive container, osnovna paleta boja) — `_Layout.cshtml`.
- [x] 🔧 Commit: `feat: base layout with Bootstrap`

### 1.2 Baza podataka i core model
- [x] EF Core + SQL Server provider, `ApplicationDbContext`.
- [x] 🔧 **Unijeti connection string** u `appsettings.Development.json` (LocalDB, npr. `Server=(localdb)\\mssqllocaldb;Database=HomeOS;Trusted_Connection=True;`) — ovaj fajl NIJE u Gitu.
- [x] Entiteti: `Household`, `Member` (pojednostavljeno — bez granularnih dozvola za sada, samo `Visibility` enum: `Private`/`Household`).
- [x] Autentifikacija — ASP.NET Core Identity (cookie-based, ugrađeno, brzo za postavku).
- [x] Prva migracija + provjera konekcije na bazu.
- [x] 🔧 Commit: `feat: database setup, Identity auth, Household/Member entities`

### 1.3 Zadaci (Tasks) — prvi pravi modul
- [x] Entitet `TaskItem` (naslov, opis, rok, prioritet, status, odgovorna osoba, tagovi, veza na `SubTask`).
- [x] Entitet `SubTask` (checklist unutar zadatka).
- [x] CRUD kontroler + Razor Views (lista, kreiranje, izmjena, brisanje).
- [x] Filter/sort po roku, prioritetu, odgovornoj osobi.
- [x] Vizuelni indikator zadataka kojima je istekao rok.
- [x] Bootstrap stilizacija — kartice ili tabela, boje po prioritetu, provjera na mobilnoj širini.
- [x] 🔧 Commit: `feat: tasks module (CRUD, subtasks, tags, overdue indicator)`

**Definition of Done za Dan 1:** Projekat je na GitHub-u sa urednom istorijom od prve minute. Zadaci potpuno rade i izgledaju uredno na desktop i mobilnoj širini.

---

## Dan 2 (radni dan) — Dashboard, Podsjetnici, E-mail, Kalendar

### 2.1 Dashboard ("Danas")
- [x] "Danas" ekran — agregira zadatke sa rokom danas/prekoračene, današnje događaje, aktivne podsjetnike, predstojeće račune (prikaz za Finance/Calendar dodaje se kasnije kad ti moduli postoje — Dashboard se dogradi u Danu 4 da uključi i njih).
- [x] Quick Capture komponenta (modalni prozor dostupan sa navbar-a — brzo dodavanje **zadatka, bilješke ili podsjetnika**). *(Bilješka dodana nakon Dana 3, kad je modul Notes postojao — sada sva tri tipa iz izvora.)*
- [x] 🔧 Commit: `feat: dashboard with today view and quick capture`

### 2.2 Podsjetnici (Reminders) + e-mail (Resend)
- [x] Entitet `Reminder` (naslov, datum/vrijeme, jednokratni/ponavljajući, ciljani član, izvor — veza na zadatak/račun/proizvoljno).
- [x] CRUD za podsjetnike.
- [ ] Resend nalog i verifikacija domene (radiš van koda) — **čeka korisnika**, nije nešto što AI agent može uraditi (zahtijeva pristup vlastitom Resend nalogu/domeni).
- [ ] 🔧 **Unijeti Resend API ključ** kroz .NET User Secrets: `dotnet user-secrets set "Resend:ApiKey" "..."` — NE u `appsettings.json`. **Čeka korisnika** (isti razlog kao gore); kod je već pripremljen da radi čim se ključ unese, a bez njega samo tiho izostavlja slanje (vidi `ResendEmailSender.cs`).
- [x] Placeholder u `appsettings.json` (bez prave vrijednosti):
  ```json
  "Resend": { "ApiKey": "", "FromEmail": "onboarding@resend.dev", "FromName": "Home OS" }
  ```
- [x] `IEmailSender` servis + implementacija koja poziva Resend API.
- [x] Slanje e-maila kada je podsjetnik dospio — implementirano kroz provjeru pri učitavanju Dashboard-a, uz napomenu da bi u produkciji ovo bio pozadinski job (npr. `IHostedService`/Hangfire).
- [x] In-app notifikacija (jednostavan badge/lista u navbaru) — dodatni kanal pored e-maila.
- [x] 🔧 Commit: `feat: reminders module with Resend email integration`

### 2.3 Kalendar
- [x] Entitet `Event` (naslov, početak/kraj, lokacija opciono, učesnici).
- [x] Mjesečni/sedmični/dnevni prikaz — za brzinu implementacije koristiti gotovu JS biblioteku (npr. **FullCalendar**, uključuje se preko CDN-a, radi bez potrebe za SPA-om).
- [x] Zadaci sa rokom automatski se prikazuju kao stavke na kalendaru (read-only projekcija — kalendar čita iz Tasks tabele, ne duplicira podatke).
- [x] Zajednički prikaz svih događaja domaćinstva.
- [x] 🔧 Commit: `feat: calendar module with task deadline projection`

**Definition of Done za Dan 2:** Dashboard prikazuje pravo stanje iz 3 modula, podsjetnik realno šalje e-mail preko Resend-a, kalendar prikazuje i događaje i projektovane rokove zadataka.

---

## Međukorak — Proširivost platforme (dograđeno na zahtjev, kraj Dana 2 / prelaz na Dan 3)

Nakon Dana 2, na eksplicitan zahtjev, dograđena je arhitektura proširivosti da se zadovolji sekcija "Proširivost — Arhitektura i principi platforme" iz `00_Specifikacija_Izvor.md` prije nego što se dodaju novi moduli (da ih odmah koriste). Detalji mehanizama: `02_Pravila_Programiranja.md`, sekcije 1.4–1.9.

- [x] **Registar modula** (`IModuleDescriptor` + `IModuleRegistry` + `ModuleState`) — navbar, komandna paleta, pretraga i dashboard se generišu iz registra; novi modul se pojavljuje jednom DI linijom, bez izmjene Shell-a.
- [x] **Uključi/isključi modul** (stranica `/Modules`) — čisto i reverzibilno, isključen modul nestaje iz svih površina bez brisanja podataka.
- [x] **Univerzalna pretraga** (`ISearchable` + `SearchService` + `/Search`) — *pomjereno ranije sa Dana 4 (sekcija 4.3)*; svaki modul se sam prijavljuje, pretraga poštuje isključene module.
- [x] **Dashboard iz doprinosa modula** (`IDashboardContributor`) — svaki modul doprinosi svoju "Danas" sekciju.
- [x] **Komandna paleta** (Ctrl+K) — skok na modul + pretraga, generisano iz registra.
- [x] **Event bus** (`IEventBus`/`IEventHandler`) — moduli objavljuju ključne trenutke, drugi reaguju bez direktne zavisnosti (Tasks→Reminders auto-podsjetnik).
- [x] **Dozvole po modulu** (`IPermissionService` + `ModulePermissionState`) — modul deklariše tražene tuđe podatke; pregled/opoziv na `/Modules`; provedeno na Kalendar→Tasks čitanju.

> **Napomena:** sekcija 4.3 (Univerzalna pretraga) na Danu 4 je ovim **ispunjena ranije** — na Danu 4 ostaje samo dodati `ISearchable` provider za module koji tada nastaju (Finance, Life Admin), što je ionako dio "definition of done" svakog modula.

---

## Dan 3 (radni dan) — Kanban, Bilješke, Liste za kupovinu, Dijeljenje

### 3.1 Kanban tabla
> **Revidirano nakon Dana 3 (usklađivanje sa izvorom).** Prvobitno su postojali zasebni entiteti `Board`/`Column`/`Card` sa više tabli. Izvorni dokument opisuje Kanban kao **"vizuelni prikaz zadataka organizovanih u kolone"** — dakle *pogled* na zadatke, ne zaseban skup podataka. Preurađeno u **jednu tablu koja se automatski formira od zadataka** (kolona = `TaskState`); Kanban više nema vlastite tabele. Time nestaje i rizik od nesinhronizovanog stanja između `Card.ColumnId` i `TaskItem.Status`.
- [x] Kanban je projekcija zadataka po statusu (kolone To do / In progress / Done, lokalizovano) — bez `Board`/`Column`/`Card` entiteta.
- [x] Drag-and-drop: **SortableJS** (CDN) + AJAX endpoint (`POST /Kanban/MoveTask`) koji mijenja `TaskItem.Status` bez reload-a. **Verifikovano.**
- [x] Promjena statusa ide kroz zajednički `ITaskWorkflowService` (isti kod kao `Tasks/Edit`), pa ponavljajući zadatak prebačen u *Done* automatski spawn-uje sljedeću instancu i sa Kanban table. **Verifikovano.**
- [x] Modul integrisan: `KanbanModule` descriptor; nema vlastite pretrage (zadaci se već pretražuju kroz `TaskSearchProvider`), poštuje vidljivost/RBAC preko `Tasks` upita.
- [x] 🔧 Commit: `feat: kanban board with drag-and-drop (SortableJS)` (+ revizija: `Kanban: single auto-formed board projected from tasks`)

### 3.2 Bilješke (Notes)
- [x] Entitet `Note` (naslov, sadržaj, tagovi, tip: obična/dnevnik) + `NoteTag` koji **dijeli isti `Tag`** kao Tasks.
- [x] CRUD + jednostavan textarea editor (rich-text je V2).
- [x] Dnevni dnevnik — jedna bilješka po danu po članu (`/Notes/Journal`), brz pristup sa Dashboard-a (widget). **Verifikovano** (ponovni poziv otvara isti zapis).
- [x] Povezivanje bilješke sa zadatkom/događajem (dropdown; Bill/Document kad ti moduli nastanu).
- [x] Modul integrisan: `NotesModule` + `NoteSearchProvider` + dashboard contributor; poštuje vidljivost/RBAC.
- [x] 🔧 Commit: `feat: notes module with daily journal`

### 3.3 Dijeljene liste za kupovinu
- [x] Entiteti `ShoppingList`, `ShoppingListItem`.
- [x] CRUD + čekiranje stavki (AJAX `ToggleItem`, bez reload-a — isti princip kao Kanban). **Verifikovano** (POST 200, precrtano bez reload-a, `IsChecked` u bazi).
- [x] Modul integrisan: `ShoppingListsModule` + `ShoppingListSearchProvider` (pretraga po nazivu liste i stavke); poštuje vidljivost/RBAC.
- [x] 🔧 Commit: `feat: shared shopping lists`

### 3.4 Dijeljenje i članovi domaćinstva (dovršavanje)
- [ ] **Model članova — poziv po e-mailu + povezivanje pri registraciji** (odlučeno): prvi registrovani korisnik kreira domaćinstvo i postaje **owner**. Owner na stranici "Članovi" dodaje člana upisom e-maila → kreira se `Member` "na čekanju" (`Email` popunjen, `IdentityUserId` prazan, `IsOwner=false`). Kad se osoba registruje tim e-mailom, `CurrentHouseholdService` je poveže s tim pending članom (postavi `IdentityUserId`) umjesto da kreira novo domaćinstvo. Ko se registruje **bez** postojeće pozivnice dobija **svoje** novo domaćinstvo (ne upada u tuđe — ispravlja trenutno ponašanje gdje svi upadaju u prvo domaćinstvo). Slanje e-maila pozivnice je V2 (za sada owner javi osobi da se registruje tim e-mailom).
- [ ] Model izmjene: `Member` dobija `IsOwner` (bool) i `Email` (string, za pending pozivnice i notifikacije). Migracija `AddMemberInvites`.
- [ ] Stranica "Članovi" (vidi/upravlja **samo owner**): lista članova (aktivni + na čekanju), dodaj po e-mailu, ukloni.
- [ ] **E-mail pri pozivu:** kad owner doda člana na čekanju, pozvana adresa dobija e-mail (preko postojećeg `IEmailSender`/Resend) s uputom da se registruje tim e-mailom. Sadržaj lokalizovan (kultura ownera).
- [ ] **E-mail vlasniku pri registraciji:** kad se pozvani član registruje i poveže s domaćinstvom, **owner dobija e-mail** obavijest da se osoba pridružila. Slanje ide iz `CurrentHouseholdService` u trenutku povezivanja.
- [ ] *Napomena:* oba e-maila koriste postojeći `IEmailSender` — ponovna upotreba zajedničkog resursa platforme (spec: "Zajedničke resurse ... e-mail ... pruža sama platforma"). Vrijedi Resend sandbox ograničenje dok domen nije verifikovan (šalje samo na vlasnikov Resend e-mail) — kod pokušava slanje i tiho loguje neuspjeh, ne ruši tok registracije.
- [x] Dodjela zadataka/podsjetnika članu + prikaz "ko je zadužen" na listama. **E-mail pri dodjeli zadatka** (`TaskAssignedEvent` → `TaskAssignedEmailHandler`) ide preko event bus-a, poštuje lične postavke obavještenja. **Verifikovano** (handler šalje, Resend sandbox 403 tiho hendlan).
- [x] Vidljivost (`Private`/`Household`/`SpecificMembers`) primijenjena dosljedno kroz Tasks, Reminders, Calendar, Notes, Shopping — član vidi *dijeljene stavke domaćinstva + svoje privatne + stavke dijeljene lično s njim*. Zajednički `VisibleTo(...)` helper (uz `ItemShare` EXISTS podupit) primijenjen u listama, pretrazi i dashboard doprinosima; selektor vidljivosti + biranje osoba dodati u Create/Edit forme. **Verifikovano** (vlasnik i dijeljeni član vide, nedijeljeni ne).
- [ ] **RBAC po članu — pristup modulima po članu** (dogovoreno na prelazu Dan 2/3): owner dodjeljuje svakom članu koje module smije otvarati/vidjeti. Nova tabela `MemberModuleAccess(HouseholdId, MemberId, ModuleKey, CanAccess)`; default = svi članovi vide sve, owner može ograničiti. `IModuleRegistry.GetEnabledAsync` proširuje se da filtrira i po pristupu trenutnog člana (uključeno za domaćinstvo **I** dozvoljeno članu). Upravljanje na stranici članova/`/Modules`. **Razlika od postojećih dozvola:** postojeći `ModulePermissionState` je pristup *modula tuđim podacima* (nivo domaćinstva); ovo je pristup *člana modulu* (nivo osobe) — dva odvojena sloja, vidi `02_Pravila_Programiranja.md` 1.9.
- [ ] 🔧 Commit: `feat: household sharing, member assignment, and per-member module access`

**Definition of Done za Dan 3:** Kanban radi glatko sa drag-and-drop, bilješke i liste za kupovinu su funkcionalne, osnovno dijeljenje između članova radi. — **ISPUNJENO.** Svi Dan 3 moduli (Kanban, Bilješke, Liste za kupovinu) + članovi domaćinstva + sva 3 sloja prava (RBAC po članu, vidljivost stavki, dodjela) gotovi i verifikovani; svaki novi modul donosi svoj descriptor/pretragu/dashboard i poštuje vidljivost/RBAC.

---

## Revizija — usklađivanje sa izvorom (nakon Dana 3, prije Finansija/Kućne administracije)

Nakon Dana 3 urađena je revizija svega odrađenog naspram `00_Specifikacija_Izvor.md` i ispravljene su uočene neusklađenosti. Sve stavke ispod su implementirane, build-ane, testirane u browseru i commit-ovane.

- [x] **Kanban — jedna auto-tabla od zadataka** (vidi 3.1): uklonjeni `Board`/`Column`/`Card`, migracija `RemoveKanbanBoards`.
- [x] **Ponavljajući zadaci** — prebacivanje u *Done* (i iz `Tasks/Edit` i iz Kanban drag-a) spawn-uje sljedeću instancu preko `ITaskWorkflowService` + `IRecurrenceService`. Izvor: "ponavljajući zadaci".
- [x] **Ponavljajući podsjetnici** — rješavanje (`Resolve`) ponavljajućeg podsjetnika kreira sljedeću instancu (isti obrazac kao zadaci). Izvor: "ponavljajući podsjetnici".
- [x] **Quick Capture — bilješka** dodana kao treći tip (uz zadatak i podsjetnik). Izvor: "brzo dodavanje zadatka, bilješke ili podsjetnika".
- [x] **E-mail pri dodijeljenom zadatku** preko event bus-a (`TaskAssignedEvent`). Izvor: "dodijeljen zadatak" kao okidač obavještenja.
- [x] **Individualna podešavanja obavještenja** — `NotificationCategory` (+`MemberNotificationPreference`, opt-out model), lična stranica `/NotificationSettings`, provjera prije slanja u svim kanalima. Izvor: "Individualna podešavanja obavještenja", "uključivanje/isključivanje kategorija obavještenja". Migracija `AddNotificationPreferences`.
- [x] **Dijeljenje sa specifičnim osobama** — `Visibility.SpecificMembers` + polimorfna `ItemShare` tabela + `IItemSharingService`; `VisibleTo` proširen EXISTS podupitom, primijenjen na sve read putanje; picker osoba u Tasks formama. Izvor: "privatno, dijeljeno sa cijelim domom ili sa specifičnim osobama". Migracija `AddItemShares`.
- [x] **Sinhronizacija u realnom vremenu** — SignalR (`HouseholdHub` + globalni broadcast filter + `realtime.js`); promjena jednog člana osvježava ekrane drugih bez reload-a. Izvor linija 69. Vidi sekciju ispod.
- [x] **E-mail prebačen na Gmail SMTP** (MailKit) umjesto Resend-a — sada realno šalje bilo kom primaocu (App Password, bez sandbox ograničenja). Nepromijenjen `IEmailSender` ugovor, pa nijedan pozivalac nije diran.
- [x] **Link do stavke u e-mailu** — mail za dodijeljeni zadatak i za dospjeli podsjetnik sadrže apsolutni link (`IAppUrlBuilder`) koji otvara zadatak/podsjetnik u jednom kliku.

### Sinhronizacija u realnom vremenu (implementirano)

Izvor (linija 69) traži: "izmjene jednog člana **odmah su vidljive svima**". Iako je arhitektura server-rendered MVC/Razor, real-time je implementiran preko **SignalR-a** tako da promjena jednog člana osvježava otvorene ekrane drugih članova bez ručnog reload-a:

- `HouseholdHub` (`/hubs/household`) — svaka konekcija se pri spajanju pridruži grupi `household-{HouseholdId}`, pa broadcast dolazi samo članovima tog domaćinstva.
- `HouseholdBroadcastFilter` (globalni MVC filter) — nakon svakog uspješnog autentifikovanog POST-a emituje `{ module }` grupi domaćinstva. **Jedna tačka** pokriva sve module, umjesto da svaki kontroler zasebno objavljuje događaj.
- `wwwroot/js/realtime.js` — klijent se spaja na hub i osvježava stranicu samo ako je promijenjeni modul relevantan za tekući ekran (dashboard na svaku promjenu; Kanban↔Tasks i Calendar←Tasks unakrsno). Ignoriše odjek vlastite upravo poslane akcije.

**Verifikovano:** sa otvorenom Kanban tablom, promjena statusa zadatka (iz drugog konteksta) izazvala je automatsko osvježavanje i kartica se premjestila u drugu kolonu bez ručnog reload-a.

*Napomena o obimu:* radi jednostavnosti i pouzdanosti, klijent radi ciljani `reload()` relevantne stranice (ne granularni DOM patch). To u potpunosti ispunjava zahtjev "odmah vidljivo svima"; granularno ažuriranje pojedinačnih elemenata je moguća V2 optimizacija. SignalR klijent se učitava sa CDN-a (isti obrazac kao SortableJS/FullCalendar).

---

## Dan 4 (nedjelja) — Finansije, Kućna administracija, Pretraga, Polish, README, Demo

### 4.1 Finansije
- [x] Entiteti `Category`, `Transaction`, `ExpenseShare`, `Budget`, `Bill`.
- [x] CRUD za transakcije po kategoriji, definisanje mjesečnog budžeta (limit po kategoriji, progress bar u pregledu).
- [x] Upravljanje računima sa datumom dospijeća → **koristi postojeći Reminder modul** preko event bus-a (`BillDueDateCreatedEvent` → `BillDueReminderHandler`, upozorenje 3 dana prije). Ne gradi se nova logika. **Verifikovano** (auto-reminder `SourceType=Bill`).
- [x] Mjesečni sažetak (prihodi/troškovi/neto + ukupno po kategoriji naspram budžeta).
- [x] Split expense — proizvoljan iznos po članu + "podijeli jednako" dugme (`ExpenseShare` po članu). **Verifikovano** (split 60/60).
- [x] 🔧 Commit: `feat: finance module (transactions, budgets, bills, split expense)`

### 4.2 Kućna administracija (Life admin)
- [x] Entiteti `Document` (naziv, kategorija, datum isteka, bilješke; `FilePath` ostavljen kao V2 hook — upload fajla van obima) i `Contact` (naziv, uloga, telefon, e-mail).
- [x] Datumi isteka → automatski generišu `Reminder` preko event bus-a (`DocumentExpiryCreatedEvent` → `DocumentExpiryReminderHandler`, 7 dana prije). **Verifikovano** (auto-reminder `SourceType=Document`).
- [x] 🔧 Commit: `feat: life admin module (documents, contacts, renewal reminders)`

### 4.3 Univerzalna pretraga
- [x] Pretraga kroz module po naslovu/sadržaju (`LIKE` upit) — **ispunjeno ranije** (vidi Međukorak). Mehanizam `ISearchable`/`SearchService`/`/Search` je gotov; svaki modul se sam prijavljuje.
- [x] `ISearchable` provider za Finance (`FinanceSearchProvider`) i Life Admin (`LifeAdminSearchProvider`) dodati uz same module.

### 4.4 Dashboard — finalno dovršavanje
- [x] Dashboard se generiše iz `IDashboardContributor` doprinosa (vidi Međukorak) — **arhitektura gotova**.
- [x] Finance (`FinanceDashboardContributor` — predstojeći računi) i Life Admin (`LifeAdminDashboardContributor` — dokumenti koji ističu) dodali svoj doprinos. **Verifikovano** na Today ekranu.
- [x] 🔧 Commit: `feat: complete dashboard integration across all modules`

### 4.5 Polish i responzivnost (provjera na sve module, ne samo posljednji)
- [ ] Prazna stanja (empty states) i loading indikatori na svim listama.
- [ ] Provjera responzivnosti na 3 širine ekrana za svaki modul.
- [ ] Konzistentnost boja/tipografije kroz cijelu aplikaciju.
- [ ] 🔧 Commit: `style: responsive polish and empty states`

### 4.6 README.md (obavezan dio predaje)
- [ ] Kratak opis aplikacije (2-3 rečenice).
- [ ] Uputstvo za pokretanje (connection string, migracije, **Gmail SMTP: `Smtp:FromEmail` + `Smtp:AppPassword` kroz user-secrets**, `dotnet ef database update`, `dotnet run`).
- [ ] Šta je implementirano po modulu + svjesna pojednostavljenja i zašto (npr. invite flow, split expense, file upload).
- [ ] Kako bi se sistem proširio novim modulom (kratko, konceptualno — referenca na princip "gradi na postojećem" iz specifikacije).
- [ ] Šta bi se drugačije uradilo sa više vremena.
- [ ] 🔧 Commit: `docs: README`

### 4.7 Finalna provjera i priprema demoa
- [ ] Kompletan test toka kroz sve module (redom kao u demo scenariju).
- [ ] Čišćenje koda, uklanjanje mrtvog koda/komentara, provjera commit istorije.
- [ ] Priprema demo redoslijeda (5-7 min): Dashboard → Zadaci → Podsjetnik+e-mail → Kanban drag-and-drop → Kalendar → Finansije/split expense → kratko kroz arhitekturu koda → README "sljedeći koraci".
- [ ] Proba demoa naglas barem jednom.
- [ ] 🔧 Finalni commit + push: `chore: final polish before submission`
- [ ] 🔧 **Prebaciti GitHub repozitorij sa Private na Public** (radi dijeljenja sa komisijom) — **prije toga eksplicitno provjeriti cijelu commit istoriju** da nigdje nije procurio connection string, **Gmail App Password**, ili bilo koja druga tajna (GitHub → Settings → General → Change visibility → Public, tek nakon te provjere).

**Definition of Done za Dan 4 (i cijeli projekat):** Svih 8 modula funkcionalno radi, README jasno dokumentuje obim i odluke, demo je uvježban, sve je na GitHub-u sa urednom, čitljivom istorijom commit-ova od prvog dana.

---

## Kontinuirano, kroz sva 4 dana

- [ ] Commit nakon svake značajnije završene stavke — mala, česta commit-ovanja, ne jedan veliki na kraju dana.
- [ ] Ako neki modul počne trošiti mnogo više vremena od planiranog — pojednostaviti taj modul (dublje pojednostavljenje, ne brisanje) i zabilježiti to u README-u, umjesto da se ugrožava sljedeći modul u redu.
- [ ] Specifikacija (`00_Specifikacija_Izvor.md`) je izvor istine za *šta*, ovaj roadmap za *kada/kojim redom*, a `02_Pravila_Programiranja.md` za *kako*.
