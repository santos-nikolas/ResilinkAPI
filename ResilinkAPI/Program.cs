// Program.cs - Arquivo principal de configura��o e inicializa��o da nossa API Resilink

// Usings essenciais para o ASP.NET Core, Entity Framework, Swagger, e nossos pr�prios componentes
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ResilinkAPI.Data;         // Onde nosso ApplicationDbContext vive
using ResilinkAPI.Services;     // Nossas interfaces e classes de servi�o (l�gica de neg�cio)
using ResilinkAPI.Attributes;   // Nosso atributo customizado para autentica��o com API Key
using System.Text.Json.Serialization; // Para configurar op��es de serializa��o JSON, como ignorar ciclos

// Cria o construtor da aplica��o web. � o ponto de partida.
var builder = WebApplication.CreateBuilder(args);

// ----- CONFIGURA��O DOS SERVI�OS (Inje��o de Depend�ncia) -----
// Aqui a gente "diz" para o ASP.NET Core quais servi�os nossa aplica��o vai usar
// e como eles devem ser criados e gerenciados.

// 1. Configura��o do CORS (Cross-Origin Resource Sharing)
// Isso � importante para permitir que nosso frontend React (que pode rodar em uma porta diferente
// durante o desenvolvimento) possa fazer requisi��es para esta API sem ser bloqueado pelo navegador.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", // Damos um nome para nossa pol�tica de CORS
        policy =>
        {
            // Pegamos as origens permitidas do appsettings.json ou usamos um padr�o para desenvolvimento.
            // Em produ��o, aqui iriam apenas os dom�nios reais do nosso frontend.
            var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',')
                                ?? new string[] { "http://localhost:3000", "https://localhost:5173", "http://localhost:5173" }; // Portas comuns do React/Vite

            policy.WithOrigins(allowedOrigins) // Permite estas origens
                  .AllowAnyHeader()             // Permite qualquer cabe�alho na requisi��o
                  .AllowAnyMethod();            // Permite qualquer m�todo HTTP (GET, POST, PUT, etc.)
        });
});

// 2. Configura��o dos Controllers e op��es de serializa��o JSON
builder.Services.AddControllers(options =>
{
    // Se quis�ssemos que TODOS os controllers fossem protegidos pela nossa API Key por padr�o,
    // descomentar�amos a linha abaixo. Por enquanto, estamos aplicando [ApiKeyAuth] em cada controller.
    // options.Filters.Add<ApiKeyAuthAttribute>();
})
.AddJsonOptions(options => // Configura��es para como os objetos C# s�o convertidos para JSON e vice-versa
{
    // Ajuda a evitar erros se tivermos entidades com refer�ncias c�clicas (ex: Navega��o A -> B e B -> A)
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // N�o inclui propriedades com valor nulo no JSON de resposta, para deixar mais limpo.
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// 3. Configura��o do Entity Framework Core e do nosso ApplicationDbContext
// Dizemos ao EF Core para usar o SQL Server e onde encontrar a string de conex�o.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Registro dos Nossos Servi�os customizados para Inje��o de Depend�ncia
// Isso permite que a gente pe�a uma inst�ncia de ILoggingService (por exemplo) no construtor
// de um controller, e o ASP.NET Core vai saber qual classe concreta (LoggingService) instanciar.
// AddScoped significa que uma nova inst�ncia do servi�o � criada para cada requisi��o HTTP.
builder.Services.AddScoped<ILoggingService, LoggingService>(); // O LoggingService � bom registrar primeiro.
builder.Services.AddScoped<IIncidenteService, IncidenteService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IRecursoComunitarioService, RecursoComunitarioService>();
// Se f�ssemos usar o AuthServiceAD para autentica��o real com Active Directory, registrar�amos aqui:
// builder.Services.AddScoped<IAuthServiceAD, AuthServiceAD>(); 
// Ou o Mock para testes:
// builder.Services.AddScoped<IAuthServiceAD, MockAuthServiceAD>();


// 5. Configura��o do Swagger/OpenAPI para documenta��o e teste da API
builder.Services.AddEndpointsApiExplorer(); // Necess�rio para o Swagger descobrir os endpoints da API
builder.Services.AddSwaggerGen(c =>
{
    // Define as informa��es b�sicas da nossa API que aparecer�o no Swagger UI
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Resilink Fast API", // Nome da nossa API
        Version = "v1",               // Vers�o
        Description = "API Backend para a plataforma Resilink de gest�o de crises e resili�ncia comunit�ria."
    });

    // Configura��o para que o Swagger UI entenda nossa autentica��o por API Key
    // Isso adiciona um campo no Swagger para a gente poder inserir a X-API-KEY facilmente nos testes.
    c.AddSecurityDefinition("ApiKeyAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,    // Tipo de seguran�a
        In = ParameterLocation.Header,       // Onde a chave vai (no cabe�alho da requisi��o)
        Name = "X-API-KEY",                  // Nome exato do header que nosso ApiKeyAuthAttribute espera
        Description = "Chave de API para autentica��o. Cole sua chave aqui para testar os endpoints protegidos."
    });
    // Diz ao Swagger para aplicar esse esquema de seguran�a (ApiKeyAuth) aos endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKeyAuth" },
                In = ParameterLocation.Header // Redundante, mas bom para clareza
            },
            new List<string>() // Lista de escopos, n�o aplic�vel para API Key simples
        }
    });
});

// ----- Constru��o da Aplica��o -----
// Aqui a gente efetivamente constr�i a aplica��o web com todos os servi�os configurados.
var app = builder.Build();

// ----- Configura��o do Pipeline de Requisi��es HTTP -----
// A ordem dos middlewares (app.Use...) aqui � importante!

// Em ambiente de desenvolvimento, queremos ver p�ginas de erro detalhadas e usar o Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Mostra detalhes de exce��es n�o tratadas
    app.UseSwagger();                // Middleware que gera o arquivo swagger.json (a "descri��o" da API)
    app.UseSwaggerUI(c => {          // Middleware que serve a interface gr�fica do Swagger
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resilink Fast API V1"); // Onde encontrar o .json
        c.RoutePrefix = string.Empty; // Faz o Swagger UI ser acess�vel na raiz da URL (ex: http://localhost:xxxx/)
    });
}
else // Em ambiente de produ��o
{
    app.UseExceptionHandler("/Error"); // Redireciona para uma p�gina de erro amig�vel (precisa ser criada)
    app.UseHsts();                     // Adiciona o header HSTS para for�ar HTTPS (seguran�a)
}

// app.UseHttpsRedirection(); // Descomentei para simplificar testes locais com HTTP. Em produ��o, � essencial.

app.UseCors("AllowSpecificOrigin"); // Aplica a pol�tica de CORS que definimos l� em cima.

app.UseRouting(); // Habilita o sistema de roteamento do ASP.NET Core para encontrar os controllers e actions.

// Middlewares de Autentica��o e Autoriza��o
// Mesmo usando nosso ApiKeyAuthAttribute, ter app.UseAuthorization() � uma boa pr�tica
// e necess�rio se formos adicionar autoriza��o baseada em roles/pol�ticas no futuro.
// app.UseAuthentication(); // Seria usado se tiv�ssemos um esquema de autentica��o como JWT configurado aqui.
app.UseAuthorization();

app.MapControllers(); // Mapeia as requisi��es para os m�todos (Actions) nos nossos Controllers.

app.Run(); // Inicia a aplica��o e a faz escutar por requisi��es HTTP.