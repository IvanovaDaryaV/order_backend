using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
using System.Reflection;
using System.Text.Json;

public class MainService
{
    private readonly ApplicationDbContext _context;

    public MainService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Метод для принудительной установки null значений
    // универсальный для любых классов
    public void SetNullFields<T>(T obj, Dictionary<string, object> jsonDict) where T : class
    {
        foreach (var key in jsonDict.Keys)
        {
            if (jsonDict[key] == null)
            {
                var propertyName = char.ToUpper(key[0]) + key.Substring(1);
                var property = obj.GetType().GetProperty(propertyName);

                if (property != null && property.CanWrite)
                {
                    property.SetValue(obj, null);
                }
            }
        }
    }

    // Валидация для полей, которые не должны зануляться
    public void ValidateEntityForNotNullConstraints(object entity)
    {
        var type = entity.GetType();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Проверяем, допускает ли свойство значение null
            var propertyType = property.PropertyType;
            bool isNullable = !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null;

            // Получаем значение свойства
            var value = property.GetValue(entity);

            // Если свойство не nullable и значение null, генерируем исключение
            if (!isNullable && value == null)
            {
                throw new InvalidOperationException($"Поле '{property.Name}' не может быть null.");
            }
        }
    }


}
