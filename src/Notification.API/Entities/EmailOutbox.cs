namespace Notification.API.Entities;

using System;
using System.ComponentModel.DataAnnotations;

public class EmailOutbox
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Recipient { get; set; } = default!;

    [Required]
    public string Subject { get; set; } = default!;

    [Required]
    public string Body { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsSent { get; set; } = false;
}