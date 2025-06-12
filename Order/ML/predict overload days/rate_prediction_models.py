import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score, precision_score, recall_score, f1_score, roc_auc_score
from sklearn.linear_model import LogisticRegression
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier, GradientBoostingClassifier

# Загрузи свой датасет
df = pd.read_csv("zaval_dataset_balanced.csv")

# Подготовка данных
X = df.drop(columns=["label_zaval", "date"])
y = df["label_zaval"]
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# Список моделей для сравнения
models = {
    "Logistic Regression": LogisticRegression(max_iter=1000),
    "Decision Tree": DecisionTreeClassifier(),
    "Random Forest": RandomForestClassifier(),
    "Gradient Boosting": GradientBoostingClassifier()
}

# Функция для оценки модели
def evaluate_model(name, model):
    model.fit(X_train, y_train)
    y_pred = model.predict(X_test)
    y_prob = model.predict_proba(X_test)[:, 1]

    return {
        "Model": name,
        "Accuracy": round(accuracy_score(y_test, y_pred), 3),
        "Precision": round(precision_score(y_test, y_pred), 3),
        "Recall": round(recall_score(y_test, y_pred), 3),
        "F1 Score": round(f1_score(y_test, y_pred), 3),
        "ROC AUC": round(roc_auc_score(y_test, y_prob), 3)
    }

# Оцени все модели
results = [evaluate_model(name, model) for name, model in models.items()]
report_df = pd.DataFrame(results)

# Сохрани отчет в файл
report_text = report_df.to_string(index=False)
with open("model_report.txt", "w", encoding="utf-8") as f:
    f.write("📊 Сравнительный отчёт по моделям для прогноза завалов\n\n")
    f.write(report_text)

print("Отчет сохранён в model_report.txt")
