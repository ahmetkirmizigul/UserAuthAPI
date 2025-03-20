using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserAuthAPI.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [ForeignKey("Role")]
    public int RoleId { get; set; }
    public Role Role { get; set; }

    public string? ReferralCode { get; set; } // Manager olacaksa oluşturulacak

    public int FailedLoginAttempts { get; set; } = 0; // Brute Force koruması için
    public bool IsBlocked { get; set; } = false; // IP veya kullanıcı bazlı engelleme

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}