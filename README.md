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

В backup_order.sql лежит бэкап моей тестовой бдшки, там буквально одна запись, я тестила на ней.
