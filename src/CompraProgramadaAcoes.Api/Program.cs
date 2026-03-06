using CompraProgramadaAcoes.Infrastructure;
using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configurar Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Sistema de Compra Programada Ações", 
        Version = "v1",
        Description = "API para gerenciamento de compra programada de ações",
        Contact = new() { Name = "Desafio" }
    });
    
    // Incluir comentários XML para documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddInfrastructure(builder.Configuration);

// Registrar serviços da aplicação
builder.Services.AddScoped<IMotorCompraProgramada, MotorCompraProgramada>();
builder.Services.AddScoped<IMotorRebalanceamento, MotorRebalanceamento>();

// Configure the HTTP request pipeline.
var app = builder.Build();

// Habilitar Swagger UI em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Compra Programada Ações API v1");
        c.RoutePrefix = string.Empty; // Swagger UI na raiz
    });
}

app.MapOpenApi();
app.MapControllers();

app.Run();
// para testes de integracao
public partial class Program { }
