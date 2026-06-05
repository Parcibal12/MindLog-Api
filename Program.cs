using Microsoft.EntityFrameworkCore;
using MindLog.Api.Infrastructure.Data;
using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");
builder.Services.AddDbContext<MindLogDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();