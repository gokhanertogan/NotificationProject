using Microsoft.EntityFrameworkCore;
using Notification.API.Entities;

namespace Notification.API.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<EmailOutbox> EmailOutbox { get; set; }
}