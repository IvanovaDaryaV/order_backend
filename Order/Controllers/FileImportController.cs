using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Models.DTO;
using Ical.Net;
using Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query;

// Контроллер для импорта разных файлов с расписанием

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class FileImportController : ControllerBase
{
    private readonly ScheduleFetcherService _modeusService;
    private readonly ApplicationDbContext _context;

    public FileImportController(ScheduleFetcherService modeusService, ApplicationDbContext context)
    {
        _modeusService = modeusService;
        _context = context;
    }

    // Метод для проверки ресурса файла .ics
    static string CheckCalendarSource(Ical.Net.Calendar calendar)
    {
        string originalModeusProductId = "-//Custis/Modeus//Schedule Calendar App//RU";
        string originalLMSProductId = "-//Moodle Pty Ltd//NONSGML Moodle Version 2024042200.02//EN"; //проверить, для всех ли это файлов
        string prodId = calendar.ProductId;

        if (prodId == originalModeusProductId)
        {
            return "modeus";
        }
        else if (prodId == originalLMSProductId)
        {
            return "lms";
        }
        return "personal";
    }

    // метод для считывания файла ics с модеуса
    [HttpPost("upload-ics")]
    public async Task<IActionResult> UploadICS(IFormFile file, Guid userId)
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

        // Перед обработкой определить источник, т.к. у модеуса и лмс немного разные структуры описания
        // События из modeus записываются как сущность Event, lms - Task

        string currentSource = CheckCalendarSource(calendar);

        foreach (var component in calendar.Events)
        {
            string inputName = component.Summary;
            string resultName = "";
            if (currentSource == "modeus")
            {
                string[] parts = inputName.Split(" / ");
                if (parts.Length >= 2)
                {
                    resultName = $"{parts[0]} ({parts[1]})";
                }

                Event evt = new Event();
                evt.Name = resultName;
                evt.PeriodStart = component.DtStart.AsDateTimeOffset.DateTime;
                evt.PeriodEnd = component.DtEnd.AsDateTimeOffset.DateTime;
                evt.UserId = userId;

                evt.Type = currentSource;

                if (evt.PeriodEnd > DateTime.Today) evt.Status = false;
                else evt.Status = true;

                _context.Events.Add(evt);
            }
            else if (currentSource == "lms")
            {
                // Получение названия дисциплины (название курса на лмс)
                var groupedList = component.Categories;
                string courceName = string.Join(", ", groupedList);

                Order.Models.Task task = new Order.Models.Task();
                task.Name = $"{courceName}: {inputName}";
                task.HardDeadline = DateOnly.FromDateTime(component.DtStart.AsDateTimeOffset.DateTime);
                task.UserId = userId;

                task.Status = false;

                _context.Tasks.Add(task);
            }
        }
        await _context.SaveChangesAsync();

        return Ok();
    }


    //[HttpGet("fetch")]
    //public async Task<IActionResult> FetchSchedule(string token, Guid userId, DateTime startDate, DateTime endDate)
    //{
    //    if (string.IsNullOrEmpty(token))
    //    {
    //        return BadRequest("Token and PersonId are required.");
    //    }

    //    var modeusPersonId = _modeusService.GetPersonIdFromToken(token);

    //    if (string.IsNullOrEmpty(modeusPersonId))
    //    {
    //        return BadRequest("Invalid token: person_id not found.");
    //    }
    //    Console.WriteLine(modeusPersonId);
    //    var schedule = await _modeusService.FetchScheduleAsync(token, modeusPersonId, userId, startDate, endDate);

    //    return Ok(schedule); // Возвращаем JSON с расписанием
    //}

    //[HttpGet("auth-modeus")]
    //public async Task<IActionResult> AuthModeus(string email, string password)
    //{
    //    var token = await _modeusService.AuthenticateAsync(email, password);
    //    return Ok(token); 
    //}
}
