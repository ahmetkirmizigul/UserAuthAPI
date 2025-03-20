using System.ComponentModel.DataAnnotations.Schema;

namespace UserAuthAPI.Models;

public class LoginAttempt
{
    public int Id { get; set; }

    [ForeignKey("User")]
    public int? UserId { get; set; }
    public User User { get; set; }

    public string IPAddress { get; set; }
    public DateTime AttemptDate { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; }
}
