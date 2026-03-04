using CompraProgramadaAcoes.Infrastructure;
using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar serviços da aplicação
builder.Services.AddScoped<IMotorCompraProgramada, MotorCompraProgramada>();
builder.Services.AddScoped<IMotorRebalanceamento, MotorRebalanceamento>();

// Configure the HTTP request pipeline.
var app = builder.Build();

app.MapOpenApi();
app.MapControllers();

app.Run();
// para testes de integracao
public partial class Program { }
