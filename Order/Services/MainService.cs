using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Order;
using Order.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public class MainService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public MainService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    public string GenerateShortLivedToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Алгоритм подписи

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(3),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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
