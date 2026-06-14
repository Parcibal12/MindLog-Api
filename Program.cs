using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MindLog.Api.Infrastructure.Data;
using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Infrastructure.Repositories;
using MindLog.Api.Core.Application.Services;
using MindLog.Api.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");

builder.Services.AddDbContext<MindLogDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

builder.Services.AddScoped<IJournalService, JournalService>();
builder.Services.AddScoped<IClinicalReportService, ClinicalReportService>();

builder.Services.AddScoped<IReportGenerator, QuestPdfReportGenerator>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHttpClient<IAiFeedbackService, OpenAiFeedbackService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();