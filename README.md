## Pytania

### Dlaczego kolejność middleware w Program.cs ma znaczenie?

Middleware wykonywane są w kolejności rejestracji - każde wywołuje `next()`, co przekazuje żądanie do kolejnego middleware w łańcuchu. Jeśli `ExceptionHandlingMiddleware` zarejestrujemy jako pierwsze, "opakowuje" wszystkie pozostałe i może przechwycić wyjątki z dowolnego miejsca potoku. Gdyby było zarejestrowane jako ostatnie, wyjątki z wcześniejszych middleware nie zostałyby przez nie obsłużone. Analogicznie - `RequestTimingMiddleware` musi być przed routingiem, żeby mierzyć czas całego żądania, a nie tylko jego części.

### Czym różni się `app.Use` od `app.Run`?

`app.Use` dodaje middleware, które wywołuje `next()` i przekazuje żądanie dalej w potoku. `app.Run` dodaje middleware terminalne, które **nie** wywołuje `next()` - żądanie zatrzymuje się tutaj. Użycie `app.Run` w środku potoku sprawi, że wszystko zarejestrowane po nim nigdy nie zostanie wykonane.

### Dlaczego kontroler nie powinien zawierać całej logiki aplikacji?

Kontroler odpowiada wyłącznie za przyjęcie żądania HTTP i zwrócenie odpowiedzi (ViewResult, redirect, itp.). Logika biznesowa w kontrolerze jest trudna do przetestowania (wymaga symulowania HTTP), ciężka do ponownego użycia (nie da się wywołać z innego miejsca niż HTTP) i łamie zasadę pojedynczej odpowiedzialności. Wyodrębnienie jej do serwisu pozwala testować reguły biznesowe w izolacji, bez uruchamiania serwera webowego.

### Co daje test jednostkowy warstwy Service?

Pozwala zweryfikować reguły biznesowe (walidację, logikę zmiany stanu) w izolacji od infrastruktury (bazy, HTTP). Dzięki fałszywemu repozytorium testy są szybkie, deterministyczne i niezależne od środowiska. Błędy w logice są wykrywane natychmiast, bez konieczności uruchamiania całej aplikacji.

### Co powinno się stać, jeśli zapis zgłoszenia się uda, ale zapis komentarza zakończy się błędem?

Transakcja powinna zostać wycofana (`RollbackAsync`) - w bazie nie powinno zostać ani zgłoszenie, ani komentarz. Dzięki temu dane są zawsze spójne: nie istnieje zgłoszenie bez pierwszego komentarza. W tej aplikacji obsługuje to blok `try/catch` w `TicketRepository.CreateTicketWithCommentAsync`.
