using Microsoft.EntityFrameworkCore;
using NewRelic.Api.Agent;
using Notification.API;
using Notification.API.BackgroundServices;
using Notification.API.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));
builder.Services.AddSingleton<IAgent>(sp =>
       {
           var newRelicApi = NewRelic.Api.Agent.NewRelic.GetAgent();
           return newRelicApi;
       });
builder.Services.AddHostedService<EmailOutboxProcessor>();

// builder.Services.AddSingleton<ITransaction, Transaction>(); 
// builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();
SeedData.Initialize(app.Services.CreateScope().ServiceProvider);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
