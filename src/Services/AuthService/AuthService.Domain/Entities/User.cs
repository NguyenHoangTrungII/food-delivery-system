using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsLocking { get; set; }
    public bool IsOnline { get; set; }
    public int FailedPWAttempt { get; set; }
    public DateTime? LastLogin { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ModifiedOn { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;

    // Navigation properties
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>(); // Thêm quan hệ

    public List<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
