# Mini Helpdesk — APBD CW7

Prosta aplikacja ASP.NET Core MVC do obsługi zgłoszeń (helpdesk). Zbudowana w oparciu o wzorzec Controller–Service–Repository z własnymi middleware, transakcjami i testami jednostkowymi.

---

## Jak uruchomić aplikację

Wymagania: .NET 10 SDK

```bash
cd MiniHelpdesk.Web
dotnet run
```

Aplikacja domyślnie startuje na `http://localhost:5000`.  
Baza SQLite (`helpdesk.db`) tworzona jest automatycznie przy pierwszym uruchomieniu.

---

## Jak uruchomić testy

```bash
dotnet test MiniHelpdesk.Tests
```

lub z poziomu katalogu rozwiązania:

```bash
dotnet test
```

---

## Baza danych

**SQLite** — plik `helpdesk.db` tworzony obok pliku wykonywalnego.  
Schemat generowany automatycznie przez EF Core (`Database.EnsureCreated()`).  
Connection string w `appsettings.json` → `ConnectionStrings:DefaultConnection`.

---

## Gdzie jest middleware

Pliki: `MiniHelpdesk.Web/Middleware/`

| Klasa | Opis |
|---|---|
| `RequestTimingMiddleware` | Loguje metodę HTTP, ścieżkę i czas wykonania każdego żądania |
| `ExceptionHandlingMiddleware` | Przechwytuje nieobsłużone wyjątki, loguje je i zwraca użytkownikowi czytelny komunikat |

Rejestracja w `Program.cs`:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
```

---

## Gdzie jest transakcja

Plik: `MiniHelpdesk.Web/Repositories/TicketRepository.cs`, metoda `CreateTicketWithCommentAsync`.

Używa `Database.BeginTransactionAsync()` z EF Core:

```csharp
await using var transaction = await _db.Database.BeginTransactionAsync();
try
{
    _db.Tickets.Add(ticket);
    await _db.SaveChangesAsync();       // zapisuje Ticket

    comment.TicketId = ticket.Id;
    _db.TicketComments.Add(comment);
    await _db.SaveChangesAsync();       // zapisuje TicketComment

    await transaction.CommitAsync();    // zatwierdza obie operacje
}
catch
{
    await transaction.RollbackAsync();  // wycofuje w razie błędu
    throw;
}
```

Jeśli zapis komentarza się nie uda, `RollbackAsync()` cofa również wcześniej zapisane zgłoszenie.

---

## Gdzie są testy

Plik: `MiniHelpdesk.Tests/TicketServiceTests.cs`

6 testów jednostkowych dla `TicketService`, z użyciem fałszywego repozytorium `FakeTicketRepository` (brak zależności od bazy danych):

| # | Test | Co sprawdza |
|---|---|---|
| 1 | `CreateTicketAsync_ValidModel_CreatesTicketAndComment` | Poprawne zgłoszenie zostaje utworzone razem z komentarzem |
| 2 | `CreateTicketAsync_EmptyTitle_ThrowsArgumentException` | Puste Title powoduje `ArgumentException`, nic nie trafia do repozytorium |
| 3 | `CreateTicketAsync_EmptyCommentContent_ThrowsArgumentException` | Pusta treść komentarza powoduje `ArgumentException` |
| 4 | `CloseTicketAsync_ExistingTicket_ChangesStatusToClosed` | Zamknięcie zgłoszenia zmienia status na `Closed` |
| 5 | `CloseTicketAsync_NonExistentTicket_ThrowsInvalidOperationException` | Zamknięcie nieistniejącego id rzuca wyjątek |
| 6 | `GetTicketDetailsAsync_NonExistentId_ReturnsNull` | Szczegóły nieistniejącego zgłoszenia zwracają `null` |

---

## Pytania

### Dlaczego kolejność middleware w Program.cs ma znaczenie?

Middleware wykonywane są w kolejności rejestracji — każde wywołuje `next()`, co przekazuje żądanie do kolejnego middleware w łańcuchu. Jeśli `ExceptionHandlingMiddleware` zarejestrujemy jako pierwsze, "opakowuje" wszystkie pozostałe i może przechwycić wyjątki z dowolnego miejsca potoku. Gdyby było zarejestrowane jako ostatnie, wyjątki z wcześniejszych middleware nie zostałyby przez nie obsłużone. Analogicznie — `RequestTimingMiddleware` musi być przed routingiem, żeby mierzyć czas całego żądania, a nie tylko jego części.

### Czym różni się `app.Use` od `app.Run`?

`app.Use` dodaje middleware, które wywołuje `next()` i przekazuje żądanie dalej w potoku. `app.Run` dodaje middleware terminalne, które **nie** wywołuje `next()` — żądanie zatrzymuje się tutaj. Użycie `app.Run` w środku potoku sprawi, że wszystko zarejestrowane po nim nigdy nie zostanie wykonane.

### Dlaczego kontroler nie powinien zawierać całej logiki aplikacji?

Kontroler odpowiada wyłącznie za przyjęcie żądania HTTP i zwrócenie odpowiedzi (ViewResult, redirect, itp.). Logika biznesowa w kontrolerze jest trudna do przetestowania (wymaga symulowania HTTP), ciężka do ponownego użycia (nie da się wywołać z innego miejsca niż HTTP) i łamie zasadę pojedynczej odpowiedzialności. Wyodrębnienie jej do serwisu pozwala testować reguły biznesowe w izolacji, bez uruchamiania serwera webowego.

### Co daje test jednostkowy warstwy Service?

Pozwala zweryfikować reguły biznesowe (walidację, logikę zmiany stanu) w izolacji od infrastruktury (bazy, HTTP). Dzięki fałszywemu repozytorium testy są szybkie, deterministyczne i niezależne od środowiska. Błędy w logice są wykrywane natychmiast, bez konieczności uruchamiania całej aplikacji.

### Co powinno się stać, jeśli zapis zgłoszenia się uda, ale zapis komentarza zakończy się błędem?

Transakcja powinna zostać wycofana (`RollbackAsync`) — w bazie nie powinno zostać ani zgłoszenie, ani komentarz. Dzięki temu dane są zawsze spójne: nie istnieje zgłoszenie bez pierwszego komentarza. W tej aplikacji obsługuje to blok `try/catch` w `TicketRepository.CreateTicketWithCommentAsync`.
