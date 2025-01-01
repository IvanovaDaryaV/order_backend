using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Models.DTO;
using Ical.Net;
using Order;


[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ScheduleFetcherService _modeusService;
    private readonly ApplicationDbContext _context;

    public ScheduleController(ScheduleFetcherService modeusService, ApplicationDbContext context)
    {
        _modeusService = modeusService;
        _context = context;
    }

    [HttpPost("upload-ics")]
    public async Task<IActionResult> UploadICS(IFormFile file, Guid userId, string type)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        string fileContent;

        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            fileContent = reader.ReadToEnd();
        }

        var calendar = Calendar.Load(fileContent);
        var events = new List<Event>();

        foreach (var component in calendar.Events)
        {
            string inputName = component.Summary;
            string[] parts = inputName.Split(" / ");
            string resultName = "";
            if (parts.Length >= 2)
            {
                resultName = $"{parts[0]} ({parts[1]})";
            }

            Event evt = new Event();
            evt.Name = resultName;
            evt.PeriodStart = component.DtStart.AsDateTimeOffset.DateTime;
            evt.PeriodEnd = component.DtEnd.AsDateTimeOffset.DateTime;
            evt.UserId = userId;
            evt.Type = type;

            if (evt.PeriodEnd > DateTime.Today) evt.Status = false;
            else evt.Status = true;

            _context.Events.Add(evt);
        }
        await _context.SaveChangesAsync();

        return Ok();
    }


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

    //[HttpGet("auth-modeus")]
    //public async Task<IActionResult> AuthModeus(string email, string password)
    //{
    //    var token = await _modeusService.AuthenticateAsync(email, password);
    //    return Ok(token); 
    //}
}
