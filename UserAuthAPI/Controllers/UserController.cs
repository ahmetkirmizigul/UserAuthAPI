using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserAuthAPI.DataAccess;

namespace UserAuthAPI.Controllers;

[Route("api/user")]
[ApiController]
[Authorize] 
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı!" });

        // Kullanıcının temel bilgileri
        var profileData = new
        {
            user.Id,
            user.FullName,
            user.Email,
            Role = user.Role.Name,
            CreatedAt = user.CreatedAt
        };

        // Kullanıcı rolüne göre ek bilgiler getiriliyor.
        if (user.Role.Name == "Manager")
        {
            return Ok(new
            {
                profileData,
                ReferralCode = user.ReferralCode
            });
        }
        else if (user.Role.Name == "Admin")
        {
            var customerCount = await _context.Users.CountAsync(u => u.Role.Name == "Customer");
            var managerCount = await _context.Users.CountAsync(u => u.Role.Name == "Manager");
            var adminCount = await _context.Users.CountAsync(u => u.Role.Name == "Admin");

            return Ok(new
            {
                profileData,
                UserStatistics = new
                {
                    TotalUsers = customerCount + managerCount + adminCount,
                    Customers = customerCount,
                    Managers = managerCount,
                    Admins = adminCount
                }
            });
        }

        return Ok(profileData);
    }
}
