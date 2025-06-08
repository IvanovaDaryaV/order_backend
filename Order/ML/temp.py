# import requests
#
# url = "https://jsonplaceholder.typicode.com/posts"
# data = {
#     'user_id' = "14cb4d95-3196-42fe-b456-587d94dd499a",
#     'task_created' = "07.06.2025 13:08:04",
#     'task_deadline' = "",
#     'task_completed' = False,
#     'priority' = 3
# }
#
# new_task = {
#     'user_id': 123,
#     'task_created': '2023-12-01',
#     'task_deadline': '2023-12-15',
#     'priority': 3,
#     'user_past_overdue_rate': 0.6
# }
#
# response = requests.post(url, json=data)  # автоматически добавляет Content-Type: application/json
# print("Status Code:", response.status_code)
# print("Response JSON:", response.json())