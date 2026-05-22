using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Windows Service support
builder.Host.UseWindowsService();

// Configure URLs for Windows Service
builder.WebHost.ConfigureKestrel(options => { });
if (!builder.WebHost.GetSetting("urls").Any())
{
    builder.WebHost.UseUrls("https://localhost:5001", "http://localhost:5000");
}

// Services
builder.Services.AddSingleton<ExamRepository>();
builder.Services.AddHostedService<NotificationWorker>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


#region Minimal API

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/exams", (ExamRepository repo) => repo.GetAll());

app.MapGet("/exams/{key}", (string key, ExamRepository repo) =>
{
    var exam = repo.Get(key);
    return exam is not null ? Results.Ok(exam) : Results.NotFound();
});

app.MapPost("/exams", (ExamData exam, ExamRepository repo) =>
{
    repo.Add(exam);
    return Results.Ok();
});

app.MapPut("/exams/{key}", (string key, ExamData updated, ExamRepository repo) =>
{
    return repo.Update(key, updated) ? Results.Ok() : Results.NotFound();
});

app.MapDelete("/exams/{key}", (string key, ExamRepository repo) =>
{
    return repo.Delete(key) ? Results.Ok() : Results.NotFound();
});

#endregion

app.Run();