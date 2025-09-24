# YDB Document API Integration

Проект переведен на использование официального [YDB .NET SDK](https://github.com/ydb-platform/ydb-dotnet-sdk) для работы с Document API.

## Установка и настройка

### 1. Пакеты
```xml
<PackageReference Include="Ydb.Sdk" Version="0.23.0" />
```

### 2. Конфигурация
В `appsettings.json`:
```json
{
  "Ydb": {
    "Endpoint": "grpcs://ydb.serverless.yandexcloud.net:2135",
    "Database": "/ru-central1/b1g7mevkrfi0i4i0dd8f/etna8imno352p5mm0k7u",
    "AuthToken": "${YDB_AUTH_TOKEN}"
  }
}
```

### 3. Переменные окружения
```bash
set YDB_AUTH_TOKEN=your_ydb_token_here
```

## Схема базы данных

Выполните SQL-скрипт `Database/init_table_api.sql` в YDB консоли для создания таблиц:

- **users** - пользователи с отдельными полями
- **recipes** - рецепты с отдельными полями  
- **recipe_ingredients** - ингредиенты рецептов
- **recipe_steps** - шаги приготовления
- **user_favorite_recipes** - избранные рецепты пользователей
- **selections** - подборки рецептов

Используется Table API с типизированными колонками для лучшей производительности.

## Архитектура

### Модели
- **User** - пользователи с вложенными коллекциями избранных и добавленных рецептов
- **Recipe** - рецепты с ингредиентами и шагами
- **Selection** - подборки рецептов
- **Ingredient** - ингредиенты (вложенные в рецепты)

### Сервисы
- **YdbService** - базовый сервис для работы с Document API
- **UserService** - управление пользователями
- **RecipeService** - управление рецептами
- **FavoriteRecipeService** - избранные рецепты
- **SelectionService** - подборки

## Особенности Table API

1. **Типизированные столбцы** - каждое поле имеет строгий тип (Utf8, Int32, Timestamp, etc.)
2. **Нормализованная структура** - ингредиенты и шаги в отдельных таблицах
3. **Производительность** - быстрые запросы благодаря индексам и типизации
4. **Гибкие запросы** - полная поддержка YQL для сложных операций

## Запуск

```bash
cd Backend/Backend
dotnet restore
dotnet build
dotnet run
```

## API Endpoints

- `GET /api/recipes` - все рецепты
- `GET /api/recipes/{id}` - рецепт по ID
- `GET /api/recipes/search` - поиск рецептов
- `POST /api/recipes` - создание рецепта
- `PUT /api/recipes/{id}` - обновление рецепта
- `DELETE /api/recipes/{id}` - удаление рецепта

- `POST /api/user/login` - вход
- `POST /api/user/signup` - регистрация
- `GET /api/user/favoriteRecipes` - избранные рецепты
- `GET /api/user/addedRecipes` - добавленные рецепты



