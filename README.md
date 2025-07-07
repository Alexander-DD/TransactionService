# TransactionService

**TransactionService** is a service for managing client financial transactions. It implements operations for crediting, debiting, and reverting transactions, with support for idempotency and race condition protection.

## Key Features

- Crediting and debiting client balances
- Reverting transactions while preserving history
- Retrieving client balance at the current time or for a specific date
- Command validation and error handling in [RFC 9457 (Problem Details for HTTP APIs)](https://datatracker.ietf.org/doc/html/rfc9457) format
- Race condition protection using transactions and optimistic locking

## Architecture

- **Clean Architecture**: separation into Application, Domain, Infrastructure, API
- **CQRS**: separate handlers for commands and queries (MediatR)
- **Entity Framework Core**: database access, optimistic concurrency control using the `RowVersion` field
- **Docker**: containerization support

## Quick Start

### 1. Clone the repository
```
git clone https://github.com/your-org/TransactionService.git
```

### 2. Run via Docker Compose
```
docker-compose up --build
```

### 3. Local run
- Configure the connection string in TransactionService.Api/appsettings.json

- Run migrations if required:
```
dotnet ef database update --project TransactionService.Infrastructure
```

- Start the project:
```
dotnet run --project TransactionService.Api
```

## API Examples

### Credit

Request credit **POST**
```
/api/credit
```

Content-Type: application/json
```
{
 "id":"8f0452b2-867b-4ef8-9a9d-3c9c03d9afdf",
 "clientId": "cfaa0d3f-7fea-4423-9f69-ebff826e2f89",
 "dateTime":"2019-04-02T13:10:20.0263632+03:00",
 "amount":23.05
}
```
Response 200 OK:
```
{
 "insertDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 23.05
}
```

### Debit

Request debit **POST** 
```
/api/debit
```

Content-Type: application/json
```
{
 "id":"05eb235c-4955-4c16-bcdd-34e8178228de",
 "clientId": "cfaa0d3f-7fea-4423-9f69-ebff826e2f89",
 "dateTime":"2019-04-02T13:10:25.0263632+03,
 "amount":23.05
}
```

Response 200 OK:
```
{
 "insertDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 0
}
```

### Revert Transaction

Request **POST** Revert
```
/api/revert?id=05eb235c-4955-4c16-bcdd-34e8178228de
```

Response 200 OK:
```
{
 "revertDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 23.05
}
```

### Get balance

Request **GET** Balance
```
/api/balance?id=cfaa0d3f-7fea-4423-9f69-ebff826e2f89
```

Response 200 OK:
```
{
 "balanceDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 23.05
}
```

## Error Handling

All errors are returned in the [Problem Details (RFC 9457)](https://datatracker.ietf.org/doc/html/rfc9457) format:
```
{ 
  "type": "https://datatracker.ietf.org/doc/html/rfc9457", 
  "title": "Domain error", 
  "status": 400, 
  "detail": "Transaction not found.", 
  "instance": "/api/revert" 
}
```

## Implementation Details
- Race condition protection uses the RowVersion field in the transaction entity and transactions at the repository level.

- All business operations are processed through MediatR and implemented in separate handlers.

- Command validation is implemented via FluentValidation.

---

# TransactionService

**TransactionService** — это сервис для управления финансовыми транзакциями клиентов, реализующий операции зачисления, списания и отмены (revert) транзакций с поддержкой идемпотентности и защиты от гонок данных.

## Основные возможности

- Кредитование и дебетование баланса клиента
- Отмена (revert) транзакций с сохранением истории
- Получение баланса клиента на текущий момент или на дату
- Валидация команд и обработка ошибок в формате [RFC 9457 (Problem Details for HTTP APIs)](https://datatracker.ietf.org/doc/html/rfc9457)
- Защита от гонок с помощью транзакций и оптимистической блокировки

## Архитектура

- **Чистая архитектура**: разделение на Application, Domain, Infrastructure, API
- **CQRS**: отдельные обработчики команд и запросов (MediatR)
- **Entity Framework Core**: работа с БД, оптимистическая блокировка через поле `RowVersion`
- **Docker**: поддержка контейнеризации

## Быстрый старт

### 1. Клонируйте репозиторий
```
git clone https://github.com/your-org/TransactionService.git
```

### 2. Запуск через Docker Compose
```
docker-compose up --build
```

### 3. Локальный запуск
- Настройте строку подключения к БД в `TransactionService.Api/appsettings.json`
- Выполните миграции (если требуется):
```
dotnet ef database update --project TransactionService.Infrastructure
```

- Запустите проект:
```
dotnet run --project TransactionService.Api
```

## Примеры API

### Кредит

Request credit **POST**
```
/api/credit
```

Content-Type: application/json
```
{
 "id":"8f0452b2-867b-4ef8-9a9d-3c9c03d9afdf",
 "clientId": "cfaa0d3f-7fea-4423-9f69-ebff826e2f89",
 "dateTime":"2019-04-02T13:10:20.0263632+03:00",
 "amount":23.05
}
```
Response 200 OK:
```
{
 "insertDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 23.05
}
```

### Дебит

Request debit **POST** 
```
/api/debit
```

Content-Type: application/json
```
{
 "id":"05eb235c-4955-4c16-bcdd-34e8178228de",
 "clientId": "cfaa0d3f-7fea-4423-9f69-ebff826e2f89",
 "dateTime":"2019-04-02T13:10:25.0263632+03,
 "amount":23.05
}
```

Response 200 OK:
```
{
 "insertDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 0
}
```

### Отмена транзакции

Request **POST** Revert
```
/api/revert?id=05eb235c-4955-4c16-bcdd-34e8178228de
```

Response 200 OK:
```
{
 "revertDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 23.05
}
```

### Получение баланса
Request **GET** Balance
```
/api/balance?id=cfaa0d3f-7fea-4423-9f69-ebff826e2f89
```

Response 200 OK:
```
{
 "balanceDateTime": "2024-10-25T12:03:34+05:00",
 "clientBalance": 23.05
}
```

## Обработка ошибок

Все ошибки возвращаются в формате [Problem Details (RFC 9457)](https://datatracker.ietf.org/doc/html/rfc9457):
```
{ 
  "type": "https://datatracker.ietf.org/doc/html/rfc9457", 
  "title": "Domain error", 
  "status": 400, 
  "detail": "Transaction not found.", 
  "instance": "/api/revert" 
}
```

## Важные детали реализации

- Для защиты от гонок используется поле `RowVersion` в сущности транзакции и транзакции на уровне репозитория.
- Все бизнес-операции обрабатываются через MediatR и реализованы в отдельных Handler-ах.
- Валидация команд реализована через FluentValidation.
