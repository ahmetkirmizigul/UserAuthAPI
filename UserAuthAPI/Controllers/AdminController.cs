using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAuthAPI.DataAccess;

namespace UserAuthAPI.Controllers;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    //Admin paneli - Kullanıcı istatislikleri
    [HttpGet("user-stats")]
    public async Task<IActionResult> GetUserStatistics()
    {
        var customerCount = await _context.Users.CountAsync(u => u.Role.Name == "Customer");
        var managerCount = await _context.Users.CountAsync(u => u.Role.Name == "Manager");
        var adminCount = await _context.Users.CountAsync(u => u.Role.Name == "Admin");

        return Ok(new
        {
            TotalUsers = customerCount + managerCount + adminCount,
            Customers = customerCount,
            Managers = managerCount,
            Admins = adminCount
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.Include(u => u.Role)
            .Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            Role= u.Role.Name,
            u.CreatedAt,
            u.IsBlocked
        }).ToListAsync();

        return Ok(users);
    }

    [HttpPost("ban/{userId}")]
    public async Task<IActionResult> BanUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = " Kullanıcı bulunamadı." });

        user.IsBlocked = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kullanıcı başarıyla banlandı." });
        
    }

    [HttpPost("unban/{userId}")]
    public async Task<IActionResult> UnbanUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı!" });

        user.IsBlocked = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kullanıcı başarıyla unban edildi." });
    }

}


/*
{
  "email": "admin@gmail.com",
  "password": "Admin123!"
}
*/
