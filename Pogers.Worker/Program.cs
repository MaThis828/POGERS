using Microsoft.EntityFrameworkCore;
using Pogers.Infrastructure;
using Pogers.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddImport(builder.Configuration);
builder.Services.AddHostedService<ImportHostedService>();

var host = builder.Build();

// Nur der Worker migriert, damit Api und Worker nicht um dieselbe SQLite-Datei konkurrieren
using (var bereich = host.Services.CreateScope())
{
    var db = bereich.ServiceProvider.GetRequiredService<PogersDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
