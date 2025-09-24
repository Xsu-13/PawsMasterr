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

Выполните SQL-скрипт `Database/init.sql` в YDB консоли:

```sql
CREATE TABLE users (
    id Utf8,
    data Json,
    created_at Timestamp,
    updated_at Timestamp,
    PRIMARY KEY (id)
);

CREATE TABLE recipes (
    id Utf8,
    data Json,
    created_at Timestamp,
    updated_at Timestamp,
    PRIMARY KEY (id)
);

CREATE TABLE selections (
    id Utf8,
    data Json,
    created_at Timestamp,
    updated_at Timestamp,
    PRIMARY KEY (id)
);
```

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

## Особенности Document API

1. **JSON-документы** - все данные хранятся как JSON в поле `data`
2. **Вложенные коллекции** - ингредиенты, шаги, избранное хранятся внутри документов
3. **Гибкая схема** - легко добавлять новые поля без миграций
4. **Простой поиск** - базовая фильтрация, для сложных запросов нужны YQL-функции

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



