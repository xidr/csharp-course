# Events API Service

Веб-сервис на ASP.NET Core (.NET 10) для управления событиями (мероприятиями) и их бронированием.
Бронирование работает по схеме "быстрый ответ + отложенная обработка": API сразу отвечает `202 Accepted`,
а бронь обрабатывает фоновый сервис.
События и бронирования хранятся в памяти приложения и теряются при его перезапуске.

Деплой для теста доступен по ссылке: https://api.csharp-course.xidriel.me/swagger/index.html

---

## 🚀 Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) или новее

---

## 🛠️ Запуск проекта

### Сборка проекта

Выполните команду сборки в корневом каталоге репозитория:

```bash
dotnet build
```

### Запуск приложения

```bash
dotnet run --project csharp-course
```

По умолчанию приложение доступно по следующим адресам:
- HTTP: `http://localhost:5132`
- HTTPS: `https://localhost:7291`

### Swagger UI

После запуска приложения интерактивная документация Swagger UI доступна по адресу:
- `http://localhost:5132/swagger` (или `https://localhost:7291/swagger`)

### Тесты

Юнит-тесты лежат в проекте `csharp-course.Tests` (xUnit). Они покрывают сервис событий (`EventService`),
сервис бронирований (`BookingService`), хранилище бронирований (`BookingRepositoryInMemoryDict`)
и обработчик ошибок (`GlobalExceptionHandlingMiddleware`).
Запуск всех тестов из корневого каталога репозитория:

```bash
dotnet test
```

Тесты размечены трейтами (`[Trait]`), поэтому их можно запускать выборочно:

| Трейт | Значения |
|---|---|
| `Category` | `Success`, `Failure` |
| `Feature` | `Create`, `Read`, `Get`, `Add`, `Update`, `Delete`, `Process`, `Pending`, `Filtering`, `Pagination`, `Combined`, `Cancellation`, `Concurrency` |
| `Service` | `EventService`, `BookingService` |
| `Repository` | `BookingRepositoryInMemoryDict` |

```bash
dotnet test --filter "Category=Failure"
dotnet test --filter "Feature=Pagination"
dotnet test --filter "Service=BookingService"
```

### Deployment

В корне репозитория лежит скрипт deploy.sh для деплоя приложения на сервер.
Требуется выставить переменные окружения. Для этого предлагается установить пакет `direnv`, 
а также создать файл `.envrc` в корне репозитория с переменными окружения. Пример находится в файле `.envrc.example`



---

## 🧱 Структура проекта

```
csharp-course/             # веб-приложение
├── Controllers/           # EventsController, BookingsController — HTTP-эндпоинты
├── Contracts/
│   ├── Requests/          # EventRequest, EventsFilterRequest
│   └── Responses/         # EventResponse, BookingInfo, PaginatedResult<T>
├── Domain/                # Event, Booking (и enum BookingStatus)
├── Services/
│   ├── Interfaces/        # IEventService, IBookingService
│   ├── Implementations/   # EventService, BookingService — бизнес-логика
│   └── Background/        # BookingBackgroundService — фоновая обработка броней
├── Repositories/          # IEventRepository, IBookingRepository и их реализации в памяти
├── Middleware/            # GlobalExceptionHandlingMiddleware
└── Utils/                 # ApiResult, NotFoundException
csharp-course.Tests/       # юнит-тесты (xUnit)
```

Зависимости подключаются через DI (см. `Program.cs`):

- `IEventService` → `EventService`
- `IEventRepository` → `EventRepositoryInMemoryDict`
- `IBookingService` → `BookingService`
- `IBookingRepository` → `BookingRepositoryInMemoryDict`
- `BookingBackgroundService` — фоновый сервис (`AddHostedService`)

Сервисы и хранилища зарегистрированы как singleton, поскольку хранилища держат данные в памяти приложения
(`ConcurrentDictionary`). Чтобы сменить способ хранения, достаточно реализовать интерфейс хранилища
и поменять регистрацию.

---

## 📖 Документация API

Пути контроллеров: `/events` (события) и `/bookings` (бронирования).
Создание брони вызывается по пути события: `POST /events/{id}/book`.

### Модели данных

#### `EventRequest` (тело запроса при создании и обновлении)
- `title` (строка, обязательно): название мероприятия (от 2 до 100 символов).
- `description` (строка, опционально): описание мероприятия (до 1000 символов). Если поле не передано или равно `null`, сохраняется пустая строка.
- `startAt` (дата/время, обязательно): дата и время начала.
- `endAt` (дата/время, обязательно): дата и время окончания (должна быть позже даты начала).

#### `EventResponse` (данные мероприятия в ответе)
- `id` (GUID): уникальный идентификатор мероприятия.
- `title` (строка): название мероприятия.
- `description` (строка, может быть `null`): описание мероприятия.
- `startAt` (дата/время): дата и время начала.
- `endAt` (дата/время): дата и время окончания.

#### `EventsFilterRequest` (query-параметры `GET /events`)
Все параметры необязательны. Имена параметров не чувствительны к регистру.
- `title` (строка): поиск по названию (регистронезависимый, частичное совпадение).
- `from` (дата/время): события, которые начинаются не раньше указанной даты.
- `to` (дата/время): события, которые заканчиваются не позже указанной даты.
- `page` (integer, по умолчанию `1`): номер страницы, не меньше 1.
- `pageSize` (integer, по умолчанию `10`): количество элементов на странице, от 1 до 100.

Фильтры применяются только если переданы и комбинируются по правилу логического И.
Если `from` позже `to`, возвращается `400 Bad Request`.

Дата без времени в параметрах `from` и `to` означает полночь в начале указанного дня.
Например, `to=2026-11-05` эквивалентно `to=2026-11-05T00:00:00`:
события, заканчивающиеся днём или вечером 5 ноября, в выборку не попадут.
Чтобы включить события, заканчивающиеся в течение всего 5 ноября, укажите
`to=2026-11-05T23:59:59.9999999`. Граница `to` включительна.

#### `Booking` (бронь) и `BookingStatus`
Бронь связана с событием по `eventId` и хранится в памяти приложения.

- `id` (GUID): уникальный идентификатор брони.
- `eventId` (GUID): идентификатор события, к которому относится бронь.
- `status` (`BookingStatus`): текущий статус брони.
- `createdAt` (дата/время, UTC): момент создания брони.
- `processedAt` (дата/время, UTC, может быть `null`): момент обработки брони; пока бронь не обработана, пусто.

Статусы (`BookingStatus`):

| Статус | Значение |
|---|---|
| `Pending` | бронь создана и ожидает обработки (начальный статус) |
| `Confirmed` | бронь подтверждена |
| `Rejected` | бронь отклонена |

Сейчас фоновый сервис всегда переводит бронь в `Confirmed`. Статус `Rejected` предусмотрен моделью
для будущей логики выбора статуса.

#### `BookingInfo` (данные брони в ответе)
- `id` (GUID): идентификатор брони.
- `eventId` (GUID): идентификатор события.
- `status` (строка): `Pending`, `Confirmed` или `Rejected`.

#### `PaginatedResult<T>` (результат постраничной выборки)
- `items` (массив): элементы запрошенной страницы.
- `totalCount` (integer): общее количество событий, подходящих под фильтры (без учёта пагинации).
- `page` (integer): номер текущей страницы.
- `pageSize` (integer): запрошенное количество элементов на странице.

События упорядочены по дате начала (`startAt`). Если страница выходит за пределы выборки, `items` пустой, а `totalCount` сохраняется.

#### `ApiResult<T>` / `ApiResult` (формат успешного ответа)
- `dateTime` (datetime): дата и время формирования ответа (UTC).
- `data` (T, только для `ApiResult<T>`): возвращаемые полезные данные.

HTTP-код передаётся только в статусе ответа, в теле его нет.

#### Формат ошибок (Problem Details, RFC 7807)
Все ошибки возвращаются с `Content-Type: application/problem+json`.

`ProblemDetails` (ошибки `404 Not Found` и `500 Internal Server Error`):
- `status` (integer): HTTP-код ответа.
- `detail` (строка): описание ошибки. Для `500` всегда возвращается общее сообщение без внутренних подробностей.

`ValidationProblemDetails` (ошибка `400 Bad Request`):
- `title` (строка): общее описание ошибки.
- `status` (integer): HTTP-код ответа (`400`).
- `errors` (объект `поле -> массив сообщений`): ошибки валидации, сгруппированные по имени поля.

Пример ответа `400 Bad Request`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["Название мероприятия должно быть от 2 до 100 символов"]
  }
}
```

Пример ответа `404 Not Found`:

```json
{
  "status": 404,
  "detail": "Событие 3fa85f64-5717-4562-b3fc-2c963f66afa6 не найдено"
}
```

### Обработка ошибок

Ошибки валидации тела запроса (`EventRequest`) и query-параметров (`EventsFilterRequest`) обрабатываются при привязке модели и возвращаются как `ValidationProblemDetails`.
Остальные исключения перехватывает `GlobalExceptionHandlingMiddleware`:

| Исключение | HTTP-статус | Уровень логирования |
|---|---|---|
| `ValidationException` (например, нарушено правило «дата окончания позже даты начала» в доменной модели) | `400 Bad Request` | `Information` |
| `NotFoundException` | `404 Not Found` | `Information` |
| любое другое | `500 Internal Server Error` | `Error` (со стеком вызовов) |


---

### Эндпоинты

#### 1. Получить список событий (с фильтрацией и пагинацией)
- **Метод**: `GET`
- **Путь**: `/events`
- **Query-параметры**: `title`, `from`, `to`, `page`, `pageSize` (см. `EventsFilterRequest`).
- **Ответ**:
  - `200 OK`: возвращает страницу событий (`ApiResult<PaginatedResult<EventResponse>>`).
  - `400 Bad Request`: некорректные параметры фильтрации или пагинации (`ValidationProblemDetails`).

Пример запроса: `GET /events?title=джаз&from=2026-03-01T00:00:00&page=1&pageSize=5`

Пример ответа:

```json
{
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Концерт джаза",
        "description": "",
        "startAt": "2026-03-01T18:00:00",
        "endAt": "2026-03-01T21:00:00"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 5
  },
  "dateTime": "2026-10-13T10:00:00Z"
}
```

#### 2. Получить событие по ID
- **Метод**: `GET`
- **Путь**: `/events/{id}`
- **Параметры**: `id` (GUID) — идентификатор события.
- **Ответ**:
  - `200 OK`: событие найдено (`ApiResult<EventResponse>`).
  - `404 Not Found`: событие с указанным идентификатором не найдено (`ProblemDetails`).

#### 3. Создать событие
- **Метод**: `POST`
- **Путь**: `/events`
- **Тело запроса**: `EventRequest` (JSON)
- **Ответ**:
  - `201 Created`: событие успешно создано, возвращает созданное событие (`ApiResult<EventResponse>`).
  - `400 Bad Request`: ошибка валидации входных данных (`ValidationProblemDetails`).

#### 4. Обновить событие
- **Метод**: `PUT`
- **Путь**: `/events/{id}`
- **Параметры**: `id` (GUID) — идентификатор обновляемого события.
- **Тело запроса**: `EventRequest` (JSON)
- **Ответ**:
  - `204 No Content`: событие успешно обновлено, тело ответа пустое.
  - `400 Bad Request`: ошибка валидации входных данных (`ValidationProblemDetails`).
  - `404 Not Found`: событие не найдено (`ProblemDetails`).

#### 5. Удалить событие
- **Метод**: `DELETE`
- **Путь**: `/events/{id}`
- **Параметры**: `id` (GUID) — идентификатор удаляемого события.
- **Ответ**:
  - `200 OK`: событие успешно удалено (`ApiResult`).
  - `404 Not Found`: событие не найдено (`ProblemDetails`).

#### 6. Забронировать событие
- **Метод**: `POST`
- **Путь**: `/events/{id}/book`
- **Параметры**: `id` (GUID) — идентификатор события.
- **Тело запроса**: не требуется.
- **Ответ**:
  - `202 Accepted`: бронь принята и ожидает обработки (`ApiResult<BookingInfo>`, `status` = `Pending`).
    В заголовке `Location` возвращается ссылка на ресурс брони, например `http://localhost:5132/bookings/{bookingId}`.
  - `404 Not Found`: событие не найдено или удалено (`ProblemDetails`).

Эндпоинт не ждёт окончания обработки: он создаёт бронь в статусе `Pending` и сразу возвращает ответ.

Пример ответа:

```json
{
  "data": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Pending"
  },
  "dateTime": "2026-10-18T20:01:47.7359450Z"
}
```

#### 7. Получить бронь по ID
- **Метод**: `GET`
- **Путь**: `/bookings/{id}`
- **Параметры**: `id` (GUID) — идентификатор брони.
- **Ответ**:
  - `200 OK`: текущее состояние брони (`ApiResult<BookingInfo>`).
  - `404 Not Found`: бронь не найдена (`ProblemDetails`).

---

## ⚙️ Фоновая обработка бронирований

Обработкой броней занимается `BookingBackgroundService` (наследник `BackgroundService`), он запускается вместе с приложением.

1. Раз в 2 секунды сервис запрашивает у `BookingService` брони в статусе `Pending`, от самых старых к самым новым.
2. Для каждой брони вызывается `ProcessBookingAsync`. Он выдерживает искусственную задержку в 1 секунду,
   имитирующую обращение к внешней системе.
3. После задержки бронь переводится в статус `Confirmed`, заполняется `processedAt` (UTC), и бронь сохраняется.
   Сохранение выполняется, только если бронь всё ещё в статусе `Pending`, поэтому уже обработанная бронь
   повторно не обрабатывается и ничего не перезаписывается.
4. Ошибка при обработке одной брони логируется и не останавливает обработку остальных.
5. При остановке приложения сервис завершается корректно через `CancellationToken`.

Из-за паузы между проверками и задержки обработки бронь становится `Confirmed` примерно через 1–3 секунды после создания.

## 🧪 Пример сценария использования

1. Создайте событие:

   ```bash
   curl -X POST http://localhost:5132/events \
     -H "Content-Type: application/json" \
     -d '{"title":"Концерт джаза","startAt":"2026-03-01T18:00:00","endAt":"2026-03-01T21:00:00"}'
   ```

   В ответе в поле `data.id` придёт идентификатор события.

2. Забронируйте событие. Ответ приходит сразу: `202 Accepted`, `status` = `Pending`, заголовок `Location` со ссылкой на бронь:

   ```bash
   curl -i -X POST http://localhost:5132/events/{eventId}/book
   ```

3. Сразу запросите бронь по ссылке из `Location`. Статус всё ещё `Pending`:

   ```bash
   curl http://localhost:5132/bookings/{bookingId}
   ```

4. Подождите несколько секунд и повторите запрос. Статус изменился на `Confirmed`:

   ```json
   { "data": { "id": "...", "eventId": "...", "status": "Confirmed" }, "dateTime": "..." }
   ```

5. Если событие не существует или уже удалено, `POST /events/{id}/book` вернёт `404 Not Found`,
   так же как `GET /bookings/{id}` для неизвестной брони.

Тот же сценарий можно выполнить в Swagger UI (`/swagger`).
