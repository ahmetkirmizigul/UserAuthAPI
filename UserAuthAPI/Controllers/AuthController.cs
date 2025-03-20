using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAuthAPI.DataAccess;
using UserAuthAPI.Helpers;
using UserAuthAPI.Models.DTOs;
using UserAuthAPI.Services;

namespace UserAuthAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
  
    public AuthController(AuthService authService, AppDbContext context, JwtService jwtService)
    {
        _authService = authService;
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterUser(dto);

        return Ok(new { message = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
 
        var (success, message) = await _authService.LoginUser(dto, ipAddress);
        if (!success)
        {
            return BadRequest(new { message });
        }

        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Geçersiz e-posta veya şifre." });

        // Kullanıcının engellenmiş olup olmadığını tekrar kontrol et     Not : buraya debug ederken zaten hiç düşmüyor.kaldırılabilir..
        if (user.IsBlocked)
            return Unauthorized(new { message = "Hesabınız çok fazla başarısız giriş nedeniyle kilitlendi. Lütfen daha sonra tekrar deneyin." });

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token });
    }




}















