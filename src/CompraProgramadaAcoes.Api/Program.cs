using CompraProgramadaAcoes.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure the HTTP request pipeline.
var app = builder.Build();

app.MapOpenApi();
app.MapControllers();

app.Run();
