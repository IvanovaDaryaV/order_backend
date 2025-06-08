import pandas as pd
import joblib
from datetime import datetime


def predict_overdue_probability(new_task_data, model_path="overdue_predictor.joblib"):
    """Прогнозирование вероятности просрочки для новой задачи"""
    model = joblib.load(model_path)

    # Преобразование
    features = pd.DataFrame([{
        'days_until_deadline': (datetime.strptime(new_task_data['task_deadline'], '%Y-%m-%d') -
                                datetime.strptime(new_task_data['task_created'], '%Y-%m-%d')).days,
        'priority': new_task_data['priority'],
        'past_overdue_rate': new_task_data['user_past_overdue_rate'],
        'time_of_year': datetime.strptime(new_task_data['task_created'], '%Y-%m-%d').month
    }])

    proba = model.predict_proba(features)[0][1]  # Вероятность класса 1 (просрочка)
    return float(proba)


# Пример вызова:
# new_task = {
#     'user_id': 123,
#     'task_created': '2023-12-01',
#     'task_deadline': '2023-12-15',
#     'priority': 3,
#     'user_past_overdue_rate': 0.6
# }
#
# probability = predict_overdue_probability(new_task)
# print(f"Вероятность просрочки: {probability:.1%}")