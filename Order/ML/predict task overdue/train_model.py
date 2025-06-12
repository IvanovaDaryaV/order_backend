# from sklearn.ensemble import RandomForestClassifier
# from sklearn.metrics import classification_report
# import joblib
#
# from prepare_data import X_train, X_test, y_train, y_test
#
# def train_and_evaluate(X_train, X_test, y_train, y_test):
#     """Обучение и оценка модели"""
#     model = RandomForestClassifier(
#         n_estimators=100,
#         max_depth=5,
#         random_state=42
#     )
#
#     model.fit(X_train, y_train)
#
#     # Оценка
#     predictions = model.predict(X_test)
#     print(classification_report(y_test, predictions))
#
#     # Важность фич
#     print("Feature importances:", dict(zip(X_train.columns, model.feature_importances_)))
#
#     # Сохранение модели
#     joblib.dump(model, "overdue_predictor.joblib")
#     return model
#
#
# # Использование:
# model = train_and_evaluate(X_train, X_test, y_train, y_test)

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
