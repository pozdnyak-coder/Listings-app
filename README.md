# Объявления о квартирах — C# (ASP.NET Core) + Angular

Единый проект по урокам 1–4: API на C# с базой SQLite (EF Core) и страница на Angular
(карточки, фильтр, детали). Сервер отдаёт и API, и сайт — **одно приложение, один порт**.

```
api/        ASP.NET Core 10: endpoint'ы, EF Core, SQLite, DTO, listings.http
web-src/    исходники Angular (src/), накладываются на свежесозданный Angular-проект
deploy/     шаблоны systemd и nginx для сервера
build.mjs   сборка всего проекта одной командой
```

## Что умеет

- `GET /api/listings` (фильтры `districtId`, `maxPrice`), `GET /api/listings/{id}`, `GET /api/listings/count`
- `POST` / `PUT` / `DELETE /api/listings` — коды 201 / 204 / 404 / 400 как в уроке 2
- `GET /api/districts` — районы с числом объявлений
- Поле `address`, районы (связь 1-ко-многим), 8 стартовых объявлений
- Angular: карточка, список, фильтр, панель деталей, загрузка/ошибка, русская локаль, бейджи «выгодно»/«новое»
- Защита записи: если задан ключ `Admin:ApiKey`, POST/PUT/DELETE требуют заголовок `X-Admin-Key`

## Требования на вашем компьютере (для сборки)

.NET 10 SDK, Node.js 22+, Angular CLI (`npm i -g @angular/cli`) — как в уроке 1. Нужен интернет (NuGet и npm).

## Запуск локально (разработка)

```bash
# терминал 1 — API
cd api
dotnet run --urls http://localhost:5000      # Swagger: http://localhost:5000/swagger

# терминал 2 — Angular (первый раз создайте каркас: node build.mjs, или вручную — см. build.mjs)
cd web
ng serve --open                              # http://localhost:4200
```

## Сборка для сервера

```bash
node build.mjs
```

Скрипт сам: создаёт Angular-проект `web/` (если его нет) → накладывает `web-src/` → `ng build` →
кладёт результат в `api/wwwroot` → `dotnet publish` для Linux → делает `listings-publish.tar.gz`.

Параметры: `--rid=linux-arm64` (если сервер на ARM), `--fx` (не вшивать .NET; тогда на сервере нужен .NET 10 Runtime).

## Развёртывание на сервере с ispmanager

> **Важно.** Нужен **VPS / выделенный сервер с root-доступом по SSH**. На обычном виртуальном (shared)
> хостинге ASP.NET Core не запустить — там только PHP/статика.

1. **Сайт.** В ispmanager создайте WWW-домен (и SSL Let's Encrypt, если нужно).
2. **Загрузка.** Через «Менеджер файлов» загрузите `listings-publish.tar.gz` в
   `/var/www/ПОЛЬЗОВАТЕЛЬ/data/listings-app/` и распакуйте (или по SSH: `tar -xzf listings-publish.tar.gz`).
3. **Сервис (по SSH под root).**
   ```bash
   cd /var/www/ПОЛЬЗОВАТЕЛЬ/data/listings-app
   chmod +x ListingsApi
   nano /etc/systemd/system/listings.service     # вставьте deploy/listings.service, поправьте пути, пользователя и ключ
   systemctl daemon-reload
   systemctl enable --now listings
   systemctl status listings                      # должно быть active (running)
   curl http://127.0.0.1:5000/api/districts       # должен вернуть JSON
   ```
4. **Проксирование.** В ispmanager откройте настройки вашего сайта и добавьте содержимое
   `deploy/nginx-directives.conf` в дополнительные директивы nginx сайта (название пункта зависит от версии
   панели; запасной вариант — править конфиг сайта в `/etc/nginx/vhosts/...`). Сохраните — панель перечитает nginx.
   Если у сайта включён PHP/Apache-обработчик, переключите обработчик на «нет/статика», чтобы он не перехватывал `/`.
5. Откройте ваш домен — должна загрузиться страница с объявлениями.

### Обновление

Соберите заново (`node build.mjs`), остановите сервис (`systemctl stop listings`), распакуйте новый архив поверх
**кроме папки `App_Data`** (там база), `systemctl start listings`.

### База данных

Файл `App_Data/listings.db` рядом с приложением. Резервная копия — просто скопировать файл (сервис лучше остановить).

### Безопасность

- В `listings.service` обязательно замените `Admin__ApiKey` на длинную случайную строку — иначе любой посетитель сможет
  создавать и удалять объявления. С ключом запросы идут с заголовком `X-Admin-Key: ваш_ключ` (см. `api/listings.http`).
- Swagger на сервере выключен. Включить временно: `Environment=Swagger__Enabled=true` в unit-файле.

### Миграции EF Core

Схема базы управляется EF Core migrations. При старте приложение вызывает `DatabaseBootstrap.Apply()` и применяет неприменённые миграции.
Начальная миграция находится в `api/Data/Migrations/`.

Для локальной разработки (из каталога `api`):

```bash
dotnet tool install --global dotnet-ef --version 10.*  # если инструмент ещё не установлен
dotnet restore
dotnet ef database update
```

Проверить, что код модели и миграции согласованы: `dotnet ef migrations has-pending-model-changes` (из каталога `api`).
Если расхождение есть, приложение всё равно стартует и пишет предупреждение в лог.

При изменении сущностей создавайте следующую миграцию командой `dotnet ef migrations add <MigrationName>`, затем проверяйте её и запускайте `dotnet ef database update`. Перед обновлением существующей/production SQLite-базы обязательно сделайте резервную копию. Базы, ранее созданные через `EnsureCreated`, автоматически принимаются только если присутствуют ожидаемые таблицы и столбцы; несовместимая или частичная схема остановит запуск с ошибкой вместо попытки молча изменить данные.

## Вариант без своего сервера: GitHub + Render (Docker)

В корне проекта есть `Dockerfile` и `render.yaml`. Загрузите проект на GitHub и подключите репозиторий к
Render — сервис сам соберёт Angular и .NET и опубликует сайт по ссылке. Подробная пошаговая инструкция (проверка, пуш на
GitHub, деплой, решение проблем) — в файле [DEPLOY-RENDER.md](DEPLOY-RENDER.md).

> **Ограничение бесплатного Render:** файловая система временная, поэтому SQLite-база сбрасывается при каждом
> деплое, перезапуске и засыпании сервиса (после этого снова 8 стартовых объявлений). Для учебной демонстрации это подходит.
