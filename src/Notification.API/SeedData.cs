using Notification.API.Contexts;
using Notification.API.Entities;

namespace Notification.API;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!dbContext.EmailOutbox.Any())
        {
            var emails = new List<EmailOutbox>();

            for (int i = 1; i <= 100; i++)
            {
                emails.Add(new EmailOutbox
                {
                    Recipient = $"user{i}@example.com",
                    Subject = $"Test Subject {i}",
                    Body = $"Test Body Content for email {i}",
                    IsSent = false
                });
            }

            dbContext.EmailOutbox.AddRange(emails);
            dbContext.SaveChanges();
        }
    }
}
