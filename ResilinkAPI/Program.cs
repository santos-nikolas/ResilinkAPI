// Program.cs - Arquivo principal de configuração e inicialização da nossa API Resilink

// Usings essenciais para o ASP.NET Core, Entity Framework, Swagger, e nossos próprios componentes
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ResilinkAPI.Data;         // Onde nosso ApplicationDbContext vive
using ResilinkAPI.Services;     // Nossas interfaces e classes de serviço (lógica de negócio)
using ResilinkAPI.Attributes;   // Nosso atributo customizado para autenticação com API Key
using System.Text.Json.Serialization; // Para configurar opções de serialização JSON, como ignorar ciclos

// Cria o construtor da aplicação web. É o ponto de partida.
var builder = WebApplication.CreateBuilder(args);

// ----- CONFIGURAÇÃO DOS SERVIÇOS (Injeção de Dependência) -----
// Aqui a gente "diz" para o ASP.NET Core quais serviços nossa aplicação vai usar
// e como eles devem ser criados e gerenciados.

// 1. Configuração do CORS (Cross-Origin Resource Sharing)
// Isso é importante para permitir que nosso frontend React (que pode rodar em uma porta diferente
// durante o desenvolvimento) possa fazer requisições para esta API sem ser bloqueado pelo navegador.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", // Damos um nome para nossa política de CORS
        policy =>
        {
            // Pegamos as origens permitidas do appsettings.json ou usamos um padrão para desenvolvimento.
            // Em produção, aqui iriam apenas os domínios reais do nosso frontend.
            var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',')
                                ?? new string[] { "http://localhost:3000", "https://localhost:5173", "http://localhost:5173" }; // Portas comuns do React/Vite

            policy.WithOrigins(allowedOrigins) // Permite estas origens
                  .AllowAnyHeader()             // Permite qualquer cabeçalho na requisição
                  .AllowAnyMethod();            // Permite qualquer método HTTP (GET, POST, PUT, etc.)
        });
});

// 2. Configuração dos Controllers e opções de serialização JSON
builder.Services.AddControllers(options =>
{
    // Se quiséssemos que TODOS os controllers fossem protegidos pela nossa API Key por padrão,
    // descomentaríamos a linha abaixo. Por enquanto, estamos aplicando [ApiKeyAuth] em cada controller.
    // options.Filters.Add<ApiKeyAuthAttribute>();
})
.AddJsonOptions(options => // Configurações para como os objetos C# são convertidos para JSON e vice-versa
{
    // Ajuda a evitar erros se tivermos entidades com referências cíclicas (ex: Navegação A -> B e B -> A)
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // Não inclui propriedades com valor nulo no JSON de resposta, para deixar mais limpo.
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// 3. Configuração do Entity Framework Core e do nosso ApplicationDbContext
// Dizemos ao EF Core para usar o SQL Server e onde encontrar a string de conexão.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Registro dos Nossos Serviços customizados para Injeção de Dependência
// Isso permite que a gente peça uma instância de ILoggingService (por exemplo) no construtor
// de um controller, e o ASP.NET Core vai saber qual classe concreta (LoggingService) instanciar.
// AddScoped significa que uma nova instância do serviço é criada para cada requisição HTTP.
builder.Services.AddScoped<ILoggingService, LoggingService>(); // O LoggingService é bom registrar primeiro.
builder.Services.AddScoped<IIncidenteService, IncidenteService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IRecursoComunitarioService, RecursoComunitarioService>();
// Se fôssemos usar o AuthServiceAD para autenticação real com Active Directory, registraríamos aqui:
// builder.Services.AddScoped<IAuthServiceAD, AuthServiceAD>(); 
// Ou o Mock para testes:
// builder.Services.AddScoped<IAuthServiceAD, MockAuthServiceAD>();


// 5. Configuração do Swagger/OpenAPI para documentação e teste da API
builder.Services.AddEndpointsApiExplorer(); // Necessário para o Swagger descobrir os endpoints da API
builder.Services.AddSwaggerGen(c =>
{
    // Define as informações básicas da nossa API que aparecerão no Swagger UI
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Resilink Fast API", // Nome da nossa API
        Version = "v1",               // Versão
        Description = "API Backend para a plataforma Resilink de gestão de crises e resiliência comunitária."
    });

    // Configuração para que o Swagger UI entenda nossa autenticação por API Key
    // Isso adiciona um campo no Swagger para a gente poder inserir a X-API-KEY facilmente nos testes.
    c.AddSecurityDefinition("ApiKeyAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,    // Tipo de segurança
        In = ParameterLocation.Header,       // Onde a chave vai (no cabeçalho da requisição)
        Name = "X-API-KEY",                  // Nome exato do header que nosso ApiKeyAuthAttribute espera
        Description = "Chave de API para autenticação. Cole sua chave aqui para testar os endpoints protegidos."
    });
    // Diz ao Swagger para aplicar esse esquema de segurança (ApiKeyAuth) aos endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKeyAuth" },
                In = ParameterLocation.Header // Redundante, mas bom para clareza
            },
            new List<string>() // Lista de escopos, não aplicável para API Key simples
        }
    });
});

// ----- Construção da Aplicação -----
// Aqui a gente efetivamente constrói a aplicação web com todos os serviços configurados.
var app = builder.Build();

// ----- Configuração do Pipeline de Requisições HTTP -----
// A ordem dos middlewares (app.Use...) aqui é importante!

// Em ambiente de desenvolvimento, queremos ver páginas de erro detalhadas e usar o Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Mostra detalhes de exceções não tratadas
    app.UseSwagger();                // Middleware que gera o arquivo swagger.json (a "descrição" da API)
    app.UseSwaggerUI(c => {          // Middleware que serve a interface gráfica do Swagger
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resilink Fast API V1"); // Onde encontrar o .json
        c.RoutePrefix = string.Empty; // Faz o Swagger UI ser acessível na raiz da URL (ex: http://localhost:xxxx/)
    });
}
else // Em ambiente de produção
{
    app.UseExceptionHandler("/Error"); // Redireciona para uma página de erro amigável (precisa ser criada)
    app.UseHsts();                     // Adiciona o header HSTS para forçar HTTPS (segurança)
}

// app.UseHttpsRedirection(); // Descomentei para simplificar testes locais com HTTP. Em produção, é essencial.

app.UseCors("AllowSpecificOrigin"); // Aplica a política de CORS que definimos lá em cima.

app.UseRouting(); // Habilita o sistema de roteamento do ASP.NET Core para encontrar os controllers e actions.

// Middlewares de Autenticação e Autorização
// Mesmo usando nosso ApiKeyAuthAttribute, ter app.UseAuthorization() é uma boa prática
// e necessário se formos adicionar autorização baseada em roles/políticas no futuro.
// app.UseAuthentication(); // Seria usado se tivéssemos um esquema de autenticação como JWT configurado aqui.
app.UseAuthorization();

app.MapControllers(); // Mapeia as requisições para os métodos (Actions) nos nossos Controllers.

app.Run(); // Inicia a aplicação e a faz escutar por requisições HTTP.