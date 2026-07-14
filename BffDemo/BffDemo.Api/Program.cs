using BffDemo.Api.Common.Errors;
using BffDemo.Application;
using BffDemo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// CAMADA API = "composition root". Ela conhece TODAS as camadas apenas para
// montá-las na DI, mas delega os detalhes a cada AddXxx. Repare como o
// Program.cs fica enxuto: cada camada sabe registrar a si mesma.
// ---------------------------------------------------------------------------

// Apresentação (MVC + Swagger)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Camadas internas (a ordem não importa aqui; o DI resolve por interface)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Tratamento global de erros (converte ValidationException -> HTTP 400)
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline HTTP
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
