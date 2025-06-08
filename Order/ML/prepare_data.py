import pandas as pd
from datetime import datetime
from sklearn.model_selection import train_test_split
import datetime

def load_and_preprocess(data_path):
    df = pd.read_csv(data_path)

    # Пример данных (столбцы должны быть в вашем CSV):
    # user_id, task_created, task_deadline, task_completed, priority

    # Расчет целевой переменной: была ли задача просрочена (1/0)
    df['is_overdue'] = (df['task_completed'] > df['task_deadline']).astype(int)

    # Фичи для прогноза:
    # deadline_date = [lambda x: datetime.datetime.strptime(str(x), '%Y-%B-%d'),  df['task_deadline'].values]
    # creation_date = [lambda x: datetime.datetime.strptime(str(x), '%Y-%B-%d'),  df['task_created'].values]
    # print(list(creation_date[1])[0])
    # print(df['task_deadline'])

    df['days_until_deadline'] = subtract_lists(pd.to_datetime(df['task_deadline']), pd.to_datetime(df['task_created']))
    # df['days_until_deadline'] = df['days_until_deadline'].str.extract('(\d+)').astype(int)

    df['past_overdue_rate'] = df.groupby('user_id')['is_overdue'].transform('mean')
    df['time_of_year'] = pd.to_datetime(df['task_created']).dt.month  # Сезонность

    return df[['user_id', 'days_until_deadline', 'priority', 'past_overdue_rate', 'time_of_year', 'is_overdue']]

def subtract_lists(a, b):
    return [(x - y).days for x, y in zip(a, b)]

# Пример использования:
data = load_and_preprocess("tasks_data.csv")
print(data.to_string())
X = data.drop(columns=['is_overdue', 'user_id'])
y = data['is_overdue']

# Разделение на train/test
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
