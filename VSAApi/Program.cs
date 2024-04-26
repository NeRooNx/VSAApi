using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TwitchProject1Model.Models;
using VSAApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(t => t.FullName?.Replace('+', '.')));

builder.Services.AddDbContext<VSAApiDBContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("VSAApi")));

var assembly = typeof(Program).Assembly;

builder.Services.AddHandlers();

builder.Services.AddValidatorsFromAssembly(assembly);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapVSAApiEndpoints();

app.UseAuthorization();

app.Run();
