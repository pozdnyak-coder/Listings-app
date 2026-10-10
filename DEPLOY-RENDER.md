# Запуск, пуш на GitHub и бесплатный деплой на Render

Результат: сайт по ссылке вида `https://listings-app-xxxx.onrender.com`, бесплатно, без своего сервера.

---

## 0. Что нужно знать про бесплатный Render (прочитайте до деплоя)

| Особенность | Что это значит для проекта |
|---|---|
| Сервис «засыпает» после 15 минут без запросов | Первое открытие после паузы занимает до минуты. Потом всё быстро. |
| Файловая система временная | SQLite-файл `App_Data/listings.db` **сбрасывается** при каждом деплое, перезапуске и засыпании. После этого в базе снова 8 стартовых объявлений. Созданные через API объявления пропадают. |
| 750 бесплатных часов в месяц | Хватает на один сервис, работающий круглосуточно. |
| 512 МБ RAM, 0.1 CPU | Для этого проекта достаточно, сборка Docker идёт на серверах Render. |

Для учебного проекта это нормально: сайт всегда стартует с готовыми данными. Если нужно, чтобы данные
сохранялись, потребуется внешняя база (например, PostgreSQL) и изменение кода. Для сдачи это обычно не нужно.

Перед показом преподавателю откройте сайт за 1–2 минуты, чтобы он «проснулся».

---

## 1. Проверка проекта на своём компьютере (желательно до пуша)

Нужны: **.NET 10 SDK**, **Node.js 22+**, **Git**.

```bash
cd listings-app/api

# 1) Проверка миграций. Ожидаемый результат: "No changes have been made to the model since the last migration."
dotnet tool install --global dotnet-ef --version 10.*     # один раз
dotnet ef migrations has-pending-model-changes

# 2) Запуск API
dotnet run --urls http://localhost:5000
```

Откройте в браузере:

- http://localhost:5000/api/districts — JSON с тремя районами
- http://localhost:5000/api/listings — 8 объявлений
- http://localhost:5000/swagger — Swagger (только в режиме разработки)

**Если шаг 1 сообщил о различиях** (`Changes have been made to the model since the last migration`), пересоздайте миграцию:

```bash
cd listings-app/api
rm -rf Data/Migrations          # Windows PowerShell: Remove-Item -Recurse -Force Data\Migrations
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
```

Приложение при этом продолжит работать, различия выводятся только как предупреждение в лог.

Полная сборка фронтенда вместе с API (по желанию):

```bash
cd listings-app
node build.mjs
```

---

## 2. Пуш проекта на GitHub

### 2.1. Один раз: установка и вход

1. Установите Git: https://git-scm.com/downloads
2. Создайте аккаунт на https://github.com
3. Задайте имя и почту (те же, что на GitHub):

```bash
git config --global user.name "Ваше Имя"
git config --global user.email "you@example.com"
```

### 2.2. Создайте пустой репозиторий

1. На GitHub нажмите **+ → New repository**.
2. Имя: например `listings-app`.
3. **Не** отмечайте «Add a README», «.gitignore» и «license» — репозиторий должен быть пустым.
4. Нажмите **Create repository** и скопируйте адрес вида `https://github.com/ВАШ_ЛОГИН/listings-app.git`.

### 2.3. Отправьте код

В папке проекта (там, где лежит `Dockerfile`):

```bash
cd listings-app
git init
git add .
git status          # убедитесь, что в списке НЕТ папок bin/, obj/, web/, node_modules/, api/wwwroot/*, App_Data/
git commit -m "Первая версия проекта"
git branch -M main
git remote add origin https://github.com/ВАШ_ЛОГИН/listings-app.git
git push -u origin main
```

При пуше GitHub попросит авторизацию:

- Git for Windows откроет окно входа в браузере (Git Credential Manager) — войдите и подтвердите.
- Если просит пароль в терминале: обычный пароль не подойдёт, создайте токен: GitHub → Settings → Developer settings →
  Personal access tokens → Tokens (classic) → Generate, права `repo`. Токен вставьте вместо пароля.

Обязательно проверьте на странице репозитория, что в корне лежат `Dockerfile`, `render.yaml`, папки `api/` и `web-src/`.

### 2.4. Последующие изменения

```bash
git add .
git commit -m "Что изменили"
git push
```

Render сам пересоберёт сайт после каждого пуша (автодеплой включён по умолчанию).

---

## 3. Деплой на Render

### Вариант А. Через панель (проще всего)

1. Зайдите на https://render.com и войдите через **GitHub** (**Sign in with GitHub**).
2. Нажмите **New + → Web Service**.
3. Выберите **Build and deploy from a Git repository** и подключите GitHub-репозиторий `listings-app`
   (при первом разе Render попросит разрешить доступ к репозиториям).
4. Заполните форму:
   - **Name:** `listings-app` (от имени зависит адрес сайта)
   - **Language / Runtime:** **Docker** (Render найдёт `Dockerfile` в корне)
   - **Branch:** `main`
   - **Instance Type:** **Free**
5. Раскройте **Advanced** (или раздел Environment) и добавьте переменную окружения:
   - Key: `Admin__ApiKey` (два нижних подчёркивания)
   - Value: любая длинная случайная строка, например `k8Jd92hsLq0PzXv31mNa7TrWc5`
   - Эта строка защищает создание, изменение и удаление объявлений. Запомните её.
6. В **Health Check Path** впишите `/api/districts`.
7. Нажмите **Create Web Service**.

### Вариант Б. Через render.yaml (Blueprint)

В проекте уже есть `render.yaml`. **New + → Blueprint** → выберите репозиторий → **Apply**.
Render создаст сервис с бесплатным планом и сам сгенерирует `Admin__ApiKey`
(посмотреть: сервис → **Environment**).

### Что будет дальше

Первая сборка занимает **5–10 минут** (ставится Angular, собирается .NET). Прогресс виден на вкладке **Logs**.
Успех: в логах `Your service is live`, статус **Live**, сверху ссылка `https://....onrender.com`.

### Проверка

- `https://ВАШ-САЙТ.onrender.com/` — страница с карточками объявлений
- `https://ВАШ-САЙТ.onrender.com/api/districts` — JSON
- `https://ВАШ-САЙТ.onrender.com/api/listings/count` — `{"count":8}`

Проверка записи (замените адрес и ключ):

```bash
# без ключа — ожидается 401
curl -i -X POST https://ВАШ-САЙТ.onrender.com/api/listings \
  -H "Content-Type: application/json" \
  -d '{"title":"Тест","price":1000,"rooms":1,"districtId":1}'

# с ключом — ожидается 201
curl -i -X POST https://ВАШ-САЙТ.onrender.com/api/listings \
  -H "Content-Type: application/json" -H "X-Admin-Key: ВАШ_КЛЮЧ" \
  -d '{"title":"Тест","price":1000,"rooms":1,"districtId":1}'
```

(В Windows PowerShell вместо `curl` используйте расширение VS Code REST Client и файл `api/listings.http`.)

---

## 4. Swagger на сервере (если нужен для демонстрации)

На Render: сервис → **Environment** → добавьте `Swagger__Enabled` = `true` → сохраните (сервис перезапустится).
Swagger будет по адресу `/swagger`.

---

## 5. Чтобы сайт не засыпал (по желанию)

Бесплатный сервис https://uptimerobot.com можно настроить на HTTP-проверку
`https://ВАШ-САЙТ.onrender.com/api/districts` раз в 5 минут — сайт будет оставаться «живым».
Лимит в 750 часов в месяц при этом расходуется на один сервис без остатка, так что другие бесплатные сервисы
на этом же аккаунте тогда держать нельзя.

---

## 6. Если что-то пошло не так

| Симптом | Причина и решение |
|---|---|
| Сборка падает на шаге `ng new` или `npm run build` | Откройте Logs, найдите первую строку с `ERROR`. Чаще всего это временный сбой npm: нажмите **Manual Deploy → Deploy latest commit**. |
| Сборка падает на `dotnet publish` | Ошибка компиляции C#: проверьте, что код из репозитория собирается локально (`cd api && dotnet build`). |
| Сайт показывает «Сервер недоступен» | Сервис просыпается — подождите минуту и нажмите «Повторить». |
| Сервис перезапускается в цикле, в логах ошибка про базу | Посмотрите текст исключения из `DatabaseBootstrap` в логах — там написано, какой столбец или таблица не подходит. |
| `Deploy failed: Health check timeout` | Проверьте, что Health Check Path = `/api/districts`. Приложение должно слушать порт из переменной `PORT` — это уже сделано в Dockerfile. |
| Страница 404 на корневом адресе | В `api/wwwroot` не попал фронтенд: проверьте в логах сборки шаг `COPY --from=web`. |
| Объявления «пропали» | Это ограничение бесплатного плана (см. раздел 0): база пересоздаётся при каждом перезапуске. |

---

## 7. Шпаргалка

```bash
# проверить локально
cd api && dotnet run --urls http://localhost:5000

# отправить изменения на сайт
git add . && git commit -m "изменения" && git push
```
