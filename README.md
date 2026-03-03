# Scheme App

Веб-приложение для визуального проектирования схем. Пользователь загружает изображения деталей, перетаскивает их на рабочее полотно и сохраняет результат.

## Стек

- **Frontend**: Vue 3, Vite, vue-draggable-plus
- **Backend**: ASP.NET Core 8, Entity Framework Core, SQLite
- **Auth**: JWT

## Структура

```
scheme-app/
├── client/   # Vue 3 приложение
└── server/   # ASP.NET Core API
```

## Быстрый старт

### Вручную

Запустить бэкенд:
```bash
cd server
dotnet run
```

Запустить фронтенд (в отдельном терминале):
```bash
cd client
npm install
npm run dev
```

Открыть в браузере: `http://localhost:5173`

### Через Docker

```bash
docker-compose up --build
```

Открыть в браузере: `http://localhost:5173`

## API

Документация доступна через Swagger после запуска бэкенда: `http://localhost:5204`
