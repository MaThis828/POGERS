using Pogers.Core.Services;
using Pogers.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Die Api liest nur, Import und Migration macht der Worker
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddAbfragen();

builder.Services.AddProblemDetails();
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = false);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Fehler immer als ProblemDetails-JSON statt HTML-Seite
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/personen", async (
    string? fraktion, IPersonQueryService abfrage, CancellationToken ct, int wahlperiode = 21) =>
    Results.Ok(await abfrage.GetPersonenAsync(wahlperiode, fraktion, ct)));

app.Run();
