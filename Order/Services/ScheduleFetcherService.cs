﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Order.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Order;
using Microsoft.AspNetCore.Http;

public class ScheduleFetcherService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;

    public ScheduleFetcherService(HttpClient httpClient, ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }
    public string GetPersonIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Claims — это список данных в токене
        var personIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "person_id");

        return personIdClaim?.Value;
    }

    public async Task<string> FetchScheduleAsync(string token, string modeusPersonId, Guid userId, DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var requestBody = new
            {
                size = 500,
                timeMin = startDate.ToString("o"),  // ISO 8601
                timeMax = endDate.ToString("o"),
                //timeMin = "2024-12-01T00:00:00Z",
                //timeMax = "2024-12-07T23:59:59Z",
                //timeMin = startDate,
                //timeMax = endDate,
                attendeePersonId = new[] { modeusPersonId }
            };
            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Заголовки
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Выполняем POST-запрос
            var response = await _httpClient.PostAsync("https://utmn.modeus.org/schedule-calendar-v2/api/calendar/events/search?tz=Asia/Tyumen", content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            await ConvertEventsAsync(responseData, userId);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching schedule data: {ex}");
            return string.Empty; 
        }
    }
    public class HoldingStatus
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Link self { get; set; }
        public Link type { get; set; }
        public Link format { get; set; }
    }

    public class Event
    {
        public string name { get; set; }
        public string start { get; set; } 
    }

    public class Embedded
    {
        public List<Event> events { get; set; }
    }

    public class Root
    {
        public Embedded _embedded { get; set; }
    }
    public async System.Threading.Tasks.Task ConvertEventsAsync(string jsonString, Guid userId)
        {
        var schedule = JsonConvert.DeserializeObject<Root>(jsonString);

        if (schedule?._embedded?.events != null)
        {
            foreach (var ev in schedule._embedded.events)
            {
                DateTime date = DateTime.Parse(ev.start);
                string dateStr = date.ToString("yyyy-MM-dd");

                Console.WriteLine($"Занятие: {ev.name}, Дата: {dateStr}");

                Order.Models.Event evt = new Order.Models.Event();
                evt.Name = ev.name;
                evt.CalendarDate = DateOnly.Parse(dateStr);
                evt.UserId = userId;
                evt.Status = false;

                _context.Events.Add(evt);
                
            }
            await _context.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("Нет данных для отображения.");
        }
    }
}
