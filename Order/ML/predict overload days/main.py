from fastapi import FastAPI
from pydantic import BaseModel
import joblib
import numpy as np
import traceback
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
import pandas as pd
import joblib

import pandas as pd
import numpy as np
from datetime import datetime, timedelta

np.random.seed(42)

app = FastAPI()


class ZavalInput(BaseModel):
    overdue_last_3d: int
    high_priority_due: int
    total_tasks_due: int
    avg_total: float
    active_tasks: int

@app.post("/predict")
async def predict_zaval(data: ZavalInput):
    print("Запрос получен")
    try:
        model = joblib.load("zaval_model.pkl")
        print("Полученные данные:", data)
        features = np.array([[
            data.overdue_last_3d,
            data.high_priority_due,
            data.total_tasks_due,
            data.avg_total,
            data.active_tasks
        ]])
        prob = model.predict_proba(features)[0][1]
        return {"probability_zaval": round(float(prob), 3)}
    except Exception as e:
        print("Ошибка при обработке запроса:")
        traceback.print_exc()
        return {"probability": 0.0}

@app.post("/train-model")
async def predict_zaval():
    print("Запрос получен")
    try:
        generate_data()
        train_model()
        return {}
    except Exception as e:
        print("Ошибка при обработке запроса:")
        traceback.print_exc()

def train_model():
    df = pd.read_csv("zaval_dataset_balanced.csv")

    X = df.drop(columns=["date", "label_zaval"])
    y = df["label_zaval"]

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2)

    model = RandomForestClassifier()
    model.fit(X_train, y_train)

    print(classification_report(y_test, model.predict(X_test)))

    joblib.dump(model, "zaval_model.pkl")

def generate_data():
    # Начальная дата
    start_date = datetime.strptime("2025-05-01", "%Y-%m-%d")
    data = []
    threshold = 0.7

    for i in range(10000):
        day = start_date + timedelta(days=i)

        # искусственное повышение случаев с завалом (30% примерно)
        is_zaval = np.random.rand() < 0.3

        if is_zaval:
            overdue_last_3d = np.random.randint(3, 6)
            high_priority = np.random.randint(3, 6)
            active_tasks = np.random.randint(8, 12)
        else:
            overdue_last_3d = np.random.randint(0, 3)
            high_priority = np.random.randint(0, 3)
            active_tasks = np.random.randint(2, 7)

        tasks_created = np.random.poisson(4)
        avg_duration = np.round(np.random.uniform(1.5, 5.0), 2)

        label = int((overdue_last_3d >= 3 and active_tasks >= 8) or np.random.rand() < 0.1)

        data.append([
            day.strftime("%Y-%m-%d"),
            tasks_created,
            overdue_last_3d,
            high_priority,
            avg_duration,
            active_tasks,
            label
        ])

    df = pd.DataFrame(data, columns=[
        "date", "tasks_created", "overdue_last_3d", "high_priority",
        "avg_duration", "active_today", "label_zaval"
    ])

    # Сохраняем в CSV
    csv_path = "zaval_dataset_balanced.csv"
    df.to_csv(csv_path, index=False)
