# from fastapi import FastAPI
# from pydantic import BaseModel
# from joblib import load
# from datetime import datetime
# import traceback
#
# app = FastAPI()
#
# model = load("deadline_model.pkl")
#
# class TaskData(BaseModel):
#     user_id: str
#     task_created: str
#     task_deadline: str
#     task_completed: bool
#     priority: int
#
# @app.post("/predict")
# async def predict(task: TaskData):
#     print("Запрос получен")
#     try:
#         print("Полученные данные:", task)
#         # Преобразование даты, как пример:
#         created = datetime.strptime(task.task_created, "%d.%m.%Y %H:%M:%S")
#         # Простая фича: разница с текущим временем
#         now = datetime.utcnow()
#         delta = (now - created).total_seconds()
#
#         features = [[delta, task.priority, int(task.task_completed)]]
#         prob = model.predict_proba(features)[0][1]
#         print("Вероятность:", prob)
#         return {"probability": float(round(prob, 3))}
#     except Exception as e:
#         print("Ошибка при обработке запроса:")
#         traceback.print_exc()
#         return {"probability": 0.0}
