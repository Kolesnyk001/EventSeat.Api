# EventSeat API

Стартовий бекенд для розсадки гостей на весіллях та конференціях. Можна створити подію, додати столи й гостей та призначити гостю стіл у межах цієї події.

## Запуск

Потрібен .NET 10 SDK. У папці проєкту виконай:

```bash
dotnet run -- --urls http://localhost:5080
```

При першому запуску створяться файл SQLite `eventseat.db` і три таблиці: `Events` (події), `SeatTables` (столи), `Guests` (гості). Початкова схема створюється через `EnsureCreatedAsync`.

## Запити

| Метод | Шлях | Приклад JSON |
|---|---|---|
| GET | /api/events | |
| GET | /api/events/1 | |
| POST | /api/events | `{"name":"Весілля","date":"2027-05-22"}` |
| GET | /api/events/1/tables | |
| POST | /api/events/1/tables | `{"name":"Стіл 1"}` |
| GET | /api/events/1/guests | |
| POST | /api/events/1/guests | `{"name":"Олена"}` |
| PUT | /api/events/1/guests/1/table | `{"tableId":1}` |

У Postman спочатку створи подію, потім стіл і гостя. Підстав ID з відповідей API в наступні запити. Для JSON використовуй `Content-Type: application/json`. Щоб звільнити місце гостя, передай `{"tableId":null}`.
