using UserAuthAPI.DataAccess;
using UserAuthAPI.Helpers;
using UserAuthAPI.Models;
using Microsoft.EntityFrameworkCore;
using UserAuthAPI.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace UserAuthAPI.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> RegisterUser(RegisterDto dto)
    {     
        var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (existingUser)
            return "Bu e-posta adresi zaten kayıtlı.";

        // Referral Code varsa Manager olacak
        var role = dto.ReferralCode != null ? "Manager" : "Customer";
        var roleId = await _context.Roles.Where(r => r.Name == role).Select(r => r.Id).FirstOrDefaultAsync();

        var newUser = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            RoleId = roleId,
            ReferralCode = role == "Manager" ? Guid.NewGuid().ToString() : null,
            CreatedAt = DateTime.UtcNow,
            IsBlocked = false, 
            FailedLoginAttempts = 0 
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return $"Kullanıcı başarıyla kaydedildi. ID: {newUser.Id}";
    }


    public async Task<(bool success, string message)> LoginUser(LoginDto dto, string ipAddress)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            await RecordFailedLoginAttempt(null, ipAddress);
            return (false, "Geçersiz e-posta veya şifre.");
        }

        // Eğer hesap engellenmişse
        if (user.IsBlocked)
        {
            return (false, "Bu hesap çok fazla başarısız giriş nedeniyle kilitlendi. Admin ile iletişime geçin.");
        }
        
        // Kullanıcı bazlı Rate Limiting kontrolü
        var rateLimit = await _context.UserRateLimits.FirstOrDefaultAsync(r => r.UserId == user.Id);
        if (rateLimit == null)
        {
            rateLimit = new UserRateLimit { UserId = user.Id };
            _context.UserRateLimits.Add(rateLimit);
        }
        else
        {
            var timeDifference = DateTime.UtcNow - rateLimit.LastRequestTime;

            //Aynı user 1 dk içinde 5 kez giriş denemesi yaptıysa,geçici oalrak engelle(Bot saldırısı için)
            if (timeDifference.TotalSeconds < 60)
            {
                if (rateLimit.RequestCount >= 5)
                {
                    return (false, "Çok fazla giriş denemesi yaptınız.Lütfen 1 dakika bekleyin.");
                }
                else
                {
                    rateLimit.RequestCount++;
                }
            }
            else
            {
                rateLimit.RequestCount = 1; // 1 dk geçtiyse sayacı sıfırla
            }
        }

        rateLimit.LastRequestTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        //Şifre Doğru mu kontrolü
        if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            await RecordFailedLoginAttempt(user.Id, ipAddress);

            // Eğer 10 başarısız giriş olduysa kullanıcıyı engelle
            var failedAttempts = await _context.LoginAttempts
                .Where(l => l.UserId == user.Id && !l.IsSuccessful && l.AttemptDate > DateTime.UtcNow.AddMinutes(-15))
                .CountAsync();

            if (failedAttempts >= 10)
            {
                user.IsBlocked = true;
                await _context.SaveChangesAsync();
                return (false, "Bu hesap çok fazla başarısız giriş nedeniyle kilitlendi. Lütfen daha sonra tekrar deneyin.");
            }

            return (false, "Geçersiz e-posta veya şifre.");
        }

        // Başarılı giriş yapıldıysa, giriş sayacını sıfırla
        await ResetFailedAttempts(user.Id);
        //Başarılı giriş yapıldıysa rate limting sıfırla
        rateLimit.RequestCount = 0;
        return (true, "Giriş başarılı!");
    }


    private async Task RecordFailedLoginAttempt(int? userId, string ipAddress)
    {
        var loginAttempt = new LoginAttempt
        {
            UserId = userId,
            IPAddress = ipAddress,
            IsSuccessful = false
        };

        _context.LoginAttempts.Add(loginAttempt);
        await _context.SaveChangesAsync();
    }
    private async Task ResetFailedAttempts(int userId)
    {
        var failedAttempts = await _context.LoginAttempts
            .Where(l => l.UserId == userId && !l.IsSuccessful)
            .ToListAsync();

        _context.LoginAttempts.RemoveRange(failedAttempts);
        await _context.SaveChangesAsync();
    }





}



















