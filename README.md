# Структура проекта

Order
- **Controllers**
  - **EntitiesControllers** - Контроллеры с CRUD для основных сущностей (Task, Event, Project)
  - Контроллер с CRUD для **пользователя** и получение задач и ивентов для календаря для пользователя
  - **ParserController** - добавление событий из вкладки "ТюмГУ полезное"
  - **ScheduleController** - загрузка файла .ics
  - **ShareController** - создание ссылки на расписание, проверка запроса
- **Mappings** - настройка маппинга полей для Task, Event, Project
- **Models**
  - **AuthModel** - авторизация и регистрация
  - Классы для всех сущностей из БД
  - DTO - отдельно вынесена структура Task, Event, Project для маппинга
  - **ScheduleSharingModel** - структура отдельной уникальной записи с информацией о расписании при создании ссылки (токен, период, userId)
- **Services**
  - **MainService** - методы для привязки/отвязки задач, для зануления полей и валидации
  - ScheduleFetcherService - методы для получения ивентов из модеуса и внесение в БД, не используется
  - **ScheduleSharingService** - методы для деления расписанием по ссылке
- **ApplicationDbContext** - структура БД, связи между таблицами (внешние ключи)
- **appsettings.json** - переменные для JWT, строка подключения к БД
- **Program.cs** - архитектура приложения (подключение авторизации, swagger'а, маршруты и тд)

__________________________________

# Создание публичной ссылки на свое расписание

1. Создать ссылку: api/Schedule/create-public-link (нужно передать списком id ивентов, которые будут скрыты; вернется полная ссылка, ее последняя часть - уникальный токен)
2. Эндпоинт для проверки на то, что такой запрос был: api/Schedule/{token} (ошибка/код 200)
3. Получить информацию по токену: api/Schedule/public/{token}

Возврат полных объектов в списках:

"tasks": [ ... ],

"events": [ ... ],

"privateEventsId": [1, 2, 3]

__________________________________

# Работа с модеусом (не используется)

Вход: 
- токен из модеуса
- две даты в формате "2024-12-01T00:00:00Z"
- userId, к которому будет привязываться событие

Пример URL с моим токеном: http://localhost:5141/api/Schedule/fetch?token=eyJ4NXQiOiJNalpoTjJVNVkyWTNNVGxpWWpVd01qbGtaR1U0TVdNek1ESXlaamM1Tm1RME0yUTJZVGxpTVEiLCJraWQiOiJkMGVjNTE0YTMyYjZmODhjMGFiZDEyYTI4NDA2OTliZGQzZGViYTlkIiwiYWxnIjoiUlMyNTYifQ.eyJhdF9oYXNoIjoiQUdQSHZzODcyRHhOUWNSS0NsUFN6USIsInN1YiI6ImQ2M2FhN2RkLTc2OTYtNDk2YS1iYWU1LWViM2M3MTk0NWYzMiIsImF1ZCI6WyJzS2lyN1lRbk9VdTRHMGVDZm4zdFR4bkJmemNhIl0sImF6cCI6InNLaXI3WVFuT1V1NEcwZUNmbjN0VHhuQmZ6Y2EiLCJFeHRlcm5hbFBlcnNvbklkIjoiNGE3ZDAxN2MtNDYyZi00YjJkLWFhMzYtYTAzODVlMWYwNGQyIiwiaXNzIjoiaHR0cHM6XC9cL2F1dGgubW9kZXVzLm9yZzo0NDNcL29hdXRoMlwvdG9rZW4iLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiLQlNCw0YDRjNGPINCS0LsuINCY0LLQsNC90L7QstCwIiwiZXhwIjoxNzMzNTAzMTIxLCJub25jZSI6IlJtWnZmbFowYjBrdWRtZENlV05TVlVaUGZuVTFXVkE0TlZGcVNFczBkbmQ1YW41bk4waGFaRXByVWxKeCIsImlhdCI6MTczMzQxNjcyMSwicGVyc29uX2lkIjoiZDE3MDZhNjktNTYxNS00ZTBlLWJiYWEtNTljMWUzYjQ1MDU2In0.MnVXtGzCmsoO2CfF0pE7rY-ztQQG3p25XYXVqqj8aczsAJLlFUbGbQGVEnCXN1g2gAT_rRnfzCh4qPTX3O4ZJJSfIkeHvyi1ar88_ovH2LTZTSsswtJZ6Q7ngkHAkxZlMoswom-X0f6d9jgFLjfcS9b7nJRUXQOJtL8SW2XtRYNUvuXqmuaT2fDc7bQqQ93Yxkwz8X89r3FG_yzPNsfreppPirUwMKZTB38Yf86vTK9jXE7FFbFbT1Clg48v4MBlxkooV-e1MxKRZG0I3p2vCG22FiapS9TaPC3e9fhFeQnBqaIWIPFFktqkMhuePOQgmcmHHysiy6tNVtiWLWy5Uw&userId=d74c5df6-a4b2-4d6d-9a4a-6f1cbe7231fd&startDate=2024-12-01T00%3A00%3A00Z&endDate=2024-12-12T00%3A00%3A00Z

Все полученные записи вносятся в БД как ивенты (заполняются поля Name, UserId, PeriodEnd, PeriodStart)

__________________________________

# Загрузка из фала .ics

Эндпоинт api/Schedule/upload-ics

- userId
- загрузить файл

Файл автоматически парсится, добавляются события для пользователя.

- Название предмета + тема занятия
- Время начала, конца
- Тип modeus
- Статус true, если дата конца события уже прошла (точка отсчета - момент загрузки файла, берется текущее время); false иначе

__________________________________

# Получение событий с сайта тюмгу https://www.utmn.ru/news/events/

/api/events - просто получить список событий, например:

[ 

"14 мар 2025: 14−15 марта Всероссийская конференция «Право на город при (пост)социализме»",

"12 дек 2024: 12−13 декабря Конференция «Развитие аудита как инструмента эффективности и безопасности бизнеса в эпоху цифровой экономики»",
    
"9 дек 2024: Всероссийский диктант по английскому языку" 

]

/api/events/add-events-to-calender 

- userId к которому будем привязывать событие
- строка в формате как выше. Парсится и вносится в список событий пользователя. Type='uni event'

__________________________________

**CalendarController**

Чтобы получить данные по id юзера:

Формат URL: /api/Calendar/Weekly/UserId?data=["YYYY-MM-DD, YYYY-MM-DD"]

Пример URL: http://localhost:5141/api/Calendar/Weekly/123e4567-e89b-12d3-a456-426614174000?data=%5B%222024-11-24%22%2C%20%222025-07-20%22%5D

Возвращаются данные:
- id юзера (строка)
- список задач (все поля)
- список ивентов (все поля)
- список уникальныз названий контекстов для полученных задач и ивентов (список строк)

**UserController**
- /api/user/create (POST)
- /api/user/id/userid (PATCH) 
- /api/user/id/userid (DELETE) 
- /api/user?email='string' (GET) 

**CRUD для основных сущностей: Create, Update, Delete, GetById**
- TaskController
- EventController
- ProjectController

**TaskController**
- /api/Task/inbox/{userId} - получение всех инбоксовых задач (которые не имеют даты, контекста и не привязаны к проектам и ивентам)

_________________________________
В order.sql лежит бэкап тестовой бдшки.


# Доп. комментарии по логике

**Задача может быть привязана только к одному событию и одному проекту, а один проект или событие может иметь несколько задач (список)**. Этот список можно занулить. Каждый раз, когда передается поле taskIds, проверяется каждая задача: если ее поле Event/Project соответственно УЖЕ занято, обновление не происходит, нужно сначала отвязать задачу от события/проекта, чтобы привязать ее к другому. Если подобного конфликта не возникает, список обновляется полностью, для всех задач из списка обновляется EventId/ProjectId

У событий и у проектов есть поле taskIds + связь по внешнему ключу с задачами. Это поле нужно для того, чтобы быстро привязывать/отвязывать задачи от проектов и событий, указывая только id задачи, без передачи объекта целиком.

Реализация middleware: встроенные в ASP.Net методы.

Когда создается ссылка на расписание, создается отдельная запись о делении ссылкой в БД. Такая запись имеет уникальный токен, по которому можно проверить, был ли запрос на деление расписанием. В этой записи хранится userId, который хочет поделиться своим расписанием (у которого нужно получить его события), период и сам токен.
