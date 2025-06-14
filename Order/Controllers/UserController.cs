using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Order.Models;
using Order;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Order.Models.DTO;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public UserController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet("users")]
    /// получить всех пользователей, кроме текущего (для проектов надо)
    public async Task<IActionResult> GetAllUsersExceptCurrent()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized("User ID not found in token.");

        var users = await _context.Users
            .Where(u => u.UserId != currentUserId)
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("id/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] AuthModel model)
    {
        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Name = model.Name,
            Email = model.Email
        };

        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { userId = newUser.UserId }, newUser);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] AuthModel model, MainService service)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
            return Unauthorized("Invalid email or password");

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (passwordVerificationResult != PasswordVerificationResult.Success)
            return Unauthorized("Invalid email or password");

        var token = service.GenerateJwtToken(user, false);

        return Ok(new { 
            user,
            token
        });
    }

    // DELETE: api/User/{id}
    [HttpDelete("{id:Guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();
        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

   // GET: api/User/qrCode/{id}
    [HttpGet("qrCode")]
    [Authorize]
    public async Task<IActionResult> GetQrToken(MainService service)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return Unauthorized();

        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        if (user == null) return Unauthorized("Invalid user");

        var shortToken = service.GenerateJwtToken(user, true);

        return Ok(new { shortToken });
    }

    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> validateShortToken(MainService service) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        var token = service.GenerateJwtToken(user, true);
        return Ok(new { token });
    }


    [HttpPatch("settings")]  // частичное обновление для настроек
    [Authorize]
    public async Task<IActionResult> UpdateSettings([FromBody] UserSettingsDto settings, MainService service)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await service.UpdateUserSettingsAsync(Guid.Parse(userId), settings.Theme, settings.Language);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
