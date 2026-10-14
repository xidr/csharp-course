# Events API Service

Веб-сервис на ASP.NET Core (.NET 10) для управления событиями (мероприятиями).
События хранятся в памяти приложения и теряются при его перезапуске.

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

Юнит-тесты сервиса событий (`EventService`) лежат в проекте `csharp-course.Tests` (xUnit).
Запуск всех тестов из корневого каталога репозитория:

```bash
dotnet test
```

Тесты размечены трейтами (`[Trait]`), поэтому их можно запускать выборочно:

| Трейт | Значения |
|---|---|
| `Category` | `Success`, `Failure` |
| `Feature` | `Create`, `Read`, `Update`, `Delete`, `Filtering`, `Pagination`, `Combined`, `Concurrency` |
| `Service` | `EventService` |

```bash
dotnet test --filter "Category=Failure"
dotnet test --filter "Feature=Pagination"
```

### Deployment

В корне репозитория лежит скрипт deploy.sh для деплоя приложения на сервер.
Требуется выставить переменные окружения. Для этого предлагается установить пакет `direnv`, 
а также создать файл `.envrc` в корне репозитория с переменными окружения. Пример находится в файле `.envrc.example`



---

## 🧱 Структура проекта

```
csharp-course/             # веб-приложение
├── Controllers/           # EventsController — HTTP-эндпоинты
├── Contracts/
│   ├── Requests/          # EventRequest, EventsFilterRequest
│   └── Responses/         # EventResponse, PaginatedResult<T>
├── Domain/                # Event
├── Services/              # IEventService, EventService — бизнес-логика
├── Repositories/	   # EventRepository, EventRepositoryDict
├── Middleware/            # GlobalExceptionHandlingMiddleware
└── Utils/                 # ApiResult, NotFoundException
csharp-course.Tests/       # юнит-тесты (xUnit)
```

Зависимости подключаются через DI (см. `Program.cs`):

- `IEventService` → `EventService`
- `IEventRepository` → `EventRepositoryDict` (хранилище в памяти на `ConcurrentDictionary`)

Оба сервиса зарегистрированы как singleton, поскольку хранилище держит данные в памяти приложения.
Чтобы сменить способ хранения, достаточно реализовать `IEventRepository` и поменять регистрацию.

---

## 📖 Документация API

Базовый путь к контроллеру событий: `/events`

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
