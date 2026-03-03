# Server — ASP.NET Core

Бэкенд часть Scheme App. REST API для управления пользователями, палитрой элементов и схемами.

## Стек

- ASP.NET Core 8
- Entity Framework Core + SQLite
- BCrypt.Net — хэширование паролей
- JWT Bearer — аутентификация
- Swagger — документация API

## Структура

```
server/
├── Controllers/
│   ├── AuthController.cs      # Регистрация и логин
│   └── PaletteController.cs   # Управление палитрой
├── Data/
│   └── AppDbContext.cs        # Контекст БД
├── Models/
│   ├── User.cs
│   └── PaletteItem.cs
├── wwwroot/uploads/           # Загруженные изображения
└── scheme.db                  # SQLite база данных
```

## Запуск

```bash
dotnet run
```

Сервер запустится на `http://localhost:5204`. Swagger UI доступен по корневому адресу.

## Миграции

Применить миграции к БД:
```bash
dotnet ef database update
```

Создать новую миграцию после изменений модели:
```bash
dotnet ef migrations add НазваниеМиграции
```

## Эндпоинты

| Метод | Путь | Описание |
|-------|------|----------|
| POST | `/api/auth/register` | Регистрация |
| POST | `/api/auth/login` | Вход, возвращает JWT |
| GET | `/api/palette` | Список элементов пользователя |
| POST | `/api/palette/upload` | Загрузка нового изображения |

Все эндпоинты кроме auth требуют заголовок `Authorization: Bearer <token>`.
