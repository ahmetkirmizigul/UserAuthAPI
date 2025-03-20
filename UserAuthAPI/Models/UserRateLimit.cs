using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAuthAPI.Models;

public class UserRateLimit
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }

    public int RequestCount { get; set; } = 0; // Kullanıcı kaç kez giriş denemesi yaptı?
    public DateTime LastRequestTime { get; set; } = DateTime.UtcNow; // Son giriş denemesi 
}
