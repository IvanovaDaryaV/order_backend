using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    //возврат данных о пользователе по его идинтификатору
    [HttpGet("id/{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    //создание пользователя
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(string name, string email, string hash)
    {
        try
        {
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = hash
            };

            // Добавляем пользователя в контекст
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Возвращаем результат создания
            return CreatedAtAction(nameof(GetUserById), new { userId = newUser.Id }, newUser);
        }
        catch (Exception ex)
        {
            // Запись ошибки в логи (можно использовать ILogger)
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /*[HttpPatch("id/{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User updatedUser)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        await _context.SaveChangesAsync();

        return NoContent();
    }*/

    [HttpDelete("id/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _context.Users
            //.Include(u => u.Events)   
            /*.Include(u => u.Projects)
            .Include(u => u.Tasks)     */
            .FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();
        return Ok(user);
    }
    

}
