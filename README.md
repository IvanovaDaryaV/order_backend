# Структура проекта

Order
- **Controllers**
  - **EntitiesControllers** - Контроллеры с CRUD для основных сущностей (Task, Event, Project)
  - Контроллер с CRUD для пользователя и получение задач и ивентов для календаря для пользователя
- **Mappings** - настройка маппинга полей для Task, Event, Project
- **Models**
  - **AuthModel** - авторизация и регистрация
  - Классы для всех сущностей из БД
  - DTO - отдельно вынесена структура Task, Event, Project для маппинга 
- **Services**
  - **TaskService** - методы для привязки/отвязки задач к/от проектам, ивентам
  - ScheduleFetcherService - позже будут методы для получения ивентов из модеуса и внесение в БД, еще не готов
- **ApplicationDbContext** - структура БД, связи между таблицами (внешние ключи)
- **appsettings.json** - переменные для JWT, строка подключения к БД
- **Program.cs** - архитектура приложения (подключение авторизации, swagger'а, маршруты и тд)

__________________________________

# Работа с модеусом

Вход: 
- токен из модеуса
- две даты в формате "2024-12-01T00:00:00Z"
- userId, к которому будет привязываться событие

Пример URL с моим токеном: http://localhost:5141/api/Schedule/fetch?token=eyJ4NXQiOiJNalpoTjJVNVkyWTNNVGxpWWpVd01qbGtaR1U0TVdNek1ESXlaamM1Tm1RME0yUTJZVGxpTVEiLCJraWQiOiJkMGVjNTE0YTMyYjZmODhjMGFiZDEyYTI4NDA2OTliZGQzZGViYTlkIiwiYWxnIjoiUlMyNTYifQ.eyJhdF9oYXNoIjoiQUdQSHZzODcyRHhOUWNSS0NsUFN6USIsInN1YiI6ImQ2M2FhN2RkLTc2OTYtNDk2YS1iYWU1LWViM2M3MTk0NWYzMiIsImF1ZCI6WyJzS2lyN1lRbk9VdTRHMGVDZm4zdFR4bkJmemNhIl0sImF6cCI6InNLaXI3WVFuT1V1NEcwZUNmbjN0VHhuQmZ6Y2EiLCJFeHRlcm5hbFBlcnNvbklkIjoiNGE3ZDAxN2MtNDYyZi00YjJkLWFhMzYtYTAzODVlMWYwNGQyIiwiaXNzIjoiaHR0cHM6XC9cL2F1dGgubW9kZXVzLm9yZzo0NDNcL29hdXRoMlwvdG9rZW4iLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiLQlNCw0YDRjNGPINCS0LsuINCY0LLQsNC90L7QstCwIiwiZXhwIjoxNzMzNTAzMTIxLCJub25jZSI6IlJtWnZmbFowYjBrdWRtZENlV05TVlVaUGZuVTFXVkE0TlZGcVNFczBkbmQ1YW41bk4waGFaRXByVWxKeCIsImlhdCI6MTczMzQxNjcyMSwicGVyc29uX2lkIjoiZDE3MDZhNjktNTYxNS00ZTBlLWJiYWEtNTljMWUzYjQ1MDU2In0.MnVXtGzCmsoO2CfF0pE7rY-ztQQG3p25XYXVqqj8aczsAJLlFUbGbQGVEnCXN1g2gAT_rRnfzCh4qPTX3O4ZJJSfIkeHvyi1ar88_ovH2LTZTSsswtJZ6Q7ngkHAkxZlMoswom-X0f6d9jgFLjfcS9b7nJRUXQOJtL8SW2XtRYNUvuXqmuaT2fDc7bQqQ93Yxkwz8X89r3FG_yzPNsfreppPirUwMKZTB38Yf86vTK9jXE7FFbFbT1Clg48v4MBlxkooV-e1MxKRZG0I3p2vCG22FiapS9TaPC3e9fhFeQnBqaIWIPFFktqkMhuePOQgmcmHHysiy6tNVtiWLWy5Uw&userId=d74c5df6-a4b2-4d6d-9a4a-6f1cbe7231fd&startDate=2024-12-01T00%3A00%3A00Z&endDate=2024-12-12T00%3A00%3A00Z

Все полученные записи вносятся в БД как ивенты (заполняются поля Name, UserId, PeriodEnd, PeriodStart)

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

_________________________________
В backup_order.sql лежит бэкап моей тестовой бдшки.


# Логика

Задача может быть привязана только к одному событию и одному проекту, а один проект или событие может иметь несколько задач. При удалении задачи обновляется список задач у проекта и у события, к которому она была привязана.

Реализация middleware: встроенные в ASP.Net методы.
