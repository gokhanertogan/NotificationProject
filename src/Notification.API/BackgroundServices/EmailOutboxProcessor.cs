using NewRelic.Api.Agent;
using Notification.API.Contexts;

namespace Notification.API.BackgroundServices;

public class EmailOutboxProcessor(IServiceScopeFactory serviceScopeFactory, IAgent newRelicAgent) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IAgent _newRelicAgent = newRelicAgent;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine(Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY"));

        string profilerPath = Environment.GetEnvironmentVariable("CORECLR_PROFILER_PATH")!;
        // profilerPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net9.0", "newrelic", "NewRelic.Profiler.dll");

        if (File.Exists(profilerPath))
        {
            Console.WriteLine("Profiler DLL exists at the specified path");
        }
        else
        {
            Console.WriteLine("Profiler DLL does not exist at the specified path");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var transaction = _newRelicAgent.CurrentTransaction!;

            var unsentEmails = dbContext.EmailOutbox.Where(e => !e.IsSent).ToList();

            foreach (var email in unsentEmails)
            {
                try
                {
                    if (transaction != null)
                    {
                        transaction.AddCustomAttribute("Service", "EmailService");
                        transaction.AddCustomAttribute("Operation", "Processing Email");
                        transaction.AddCustomAttribute("EmailId", email.Id);
                    }

                    //await emailService.SendEmailAsync(email.Recipient, email.Subject, email.Body);
                    // email.IsSent = true;
                    // dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    transaction?.AddCustomAttribute("ErrorMessage", ex.Message);
                    transaction?.AddCustomAttribute("ErrorStackTrace", ex);
                    transaction?.AddCustomAttribute("EmailStatus", "Failed");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}