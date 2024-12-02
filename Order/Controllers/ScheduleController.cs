using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
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
    
    [HttpGet("fetch")]
    public async Task<IActionResult> FetchSchedule(string token, Guid userId, DateOnly startDate, DateOnly endDate)
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
