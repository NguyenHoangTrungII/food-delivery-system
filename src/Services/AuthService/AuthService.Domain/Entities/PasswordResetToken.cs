﻿namespace AuthService.Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public User User { get; set; }
}