using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ScheduleFetcherService _modeusService;

    public ScheduleController(ScheduleFetcherService modeusService)
    {
        _modeusService = modeusService;
    }

    //[HttpGet("auth")]
    //public async Task<string> AuthenticateAsync(string email, string password)
    //{
    //    try
    //    {
    //        // Тело запроса
    //        var requestBody = new
    //        {
    //            username = email,
    //            password = password
    //        };

    //        var jsonRequest = JsonConvert.SerializeObject(requestBody);
    //        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

    //        // Заголовки
    //        _httpClient.DefaultRequestHeaders.Clear();
    //        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

    //        // POST-запрос для получения токена
    //        var response = await _httpClient.PostAsync("https://utmn.modeus.org/auth/login", content);

    //        if (!response.IsSuccessStatusCode)
    //        {
    //            Console.WriteLine($"Error during authentication: {response.StatusCode}");
    //            return string.Empty;
    //        }

    //        var responseData = await response.Content.ReadAsStringAsync();
    //        var responseJson = JsonConvert.DeserializeObject<dynamic>(responseData);

    //        // Предположим, что токен хранится в поле "access_token"
    //        return responseJson?.access_token ?? string.Empty;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error during authentication: {ex}");
    //        return string.Empty;
    //    }
    //}


    [HttpGet("fetch")]
    public async Task<IActionResult> FetchSchedule(string token, Guid userId, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token and PersonId are required.");
        }

        var modeusPersonId = _modeusService.GetPersonIdFromToken(token);

        if (string.IsNullOrEmpty(modeusPersonId))
        {
            return BadRequest("Invalid token: person_id not found.");
        }
        Console.WriteLine(modeusPersonId);
        var schedule = await _modeusService.FetchScheduleAsync(token, modeusPersonId, userId, startDate, endDate);

        return Ok(schedule); // Возвращаем JSON с расписанием
    }
}
