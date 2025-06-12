import pandas as pd
from sklearn.ensemble import RandomForestClassifier
import joblib

# Обучение и сохранение модели

# Примерные данные
df = pd.DataFrame([
    {"time_to_deadline": 24, "completed": 0, "priority": 3, "overdue": 1},
    {"time_to_deadline": 48, "completed": 1, "priority": 2, "overdue": 0},
    {"time_to_deadline": -1, "completed": 0, "priority": 1, "overdue": 1},  # без дедлайна
    # и т.д.
])

X = df[["time_to_deadline", "completed", "priority"]]
y = df["overdue"]

model = RandomForestClassifier()
model.fit(X, y)

joblib.dump(model, "deadline_model.pkl")
