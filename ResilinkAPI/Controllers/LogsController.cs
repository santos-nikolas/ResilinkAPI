// Controllers/LogsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    // Para usar ToListAsync, OrderByDescending, Skip, Take
using ResilinkAPI.Attributes;         // Meu atributo para autenticação via API Key
using ResilinkAPI.Data;             // Para o ApplicationDbContext
using ResilinkAPI.Models;           // Para o modelo LogEvento
using ResilinkAPI.Services;         // Para ILoggingService (embora eu decidi não logar aqui para evitar loops)
using System;
using System.Collections.Generic;     // Para List<T> e IEnumerable<T>
using System.Linq;                  // Para OrderByDescending, Skip, Take
using System.Threading.Tasks;       // Para Task

namespace ResilinkAPI.Controllers
{
    /// <summary>
    /// Controlador responsável por fornecer acesso aos logs de eventos da plataforma Resilink.
    /// Permite a consulta paginada dos logs registrados.
    /// O acesso a este controlador é protegido por autenticação via API Key,
    /// e idealmente seria restrito a perfis administrativos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Rota base será /api/Logs
    [ApiKeyAuth] // Protege o endpoint de listagem de logs
    public class LogsController : ControllerBase
    {
        // Acesso direto ao DbContext para esta consulta simples de listagem.
        // Em cenários mais complexos, poderia haver um LogQueryService.
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService; // Injetado, mas usado com cautela aqui

        /// <summary>
        /// Construtor do LogsController.
        /// </summary>
        /// <param name="context">Contexto do banco de dados para acesso direto aos logs.</param>
        /// <param name="loggingService">Serviço para registrar logs (usado com cautela neste controller).</param>
        public LogsController(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Lista os eventos de log registrados no sistema de forma paginada.
        /// Os logs são retornados em ordem decrescente de data (mais recentes primeiro).
        /// </summary>
        /// <param name="pageNumber">O número da página a ser retornada (padrão: 1).</param>
        /// <param name="pageSize">O número de registros por página (padrão: 20, máximo: 100).</param>
        /// <returns>Uma lista paginada de LogEvento.</returns>
        /// <response code="200">Retorna a lista de logs solicitada.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="500">Erro interno no servidor ao tentar buscar os logs.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LogEvento>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            // Pego o identificador do "usuário" da API Key para referência, embora não vá logar esta ação específica.
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "AdminAPIUser_Default";
            try
            {
                // Validações básicas para os parâmetros de paginação, evitando valores problemáticos.
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 1;
                if (pageSize > 100) pageSize = 100; // Coloco um limite para não sobrecarregar o sistema.

                List<LogEvento> logs = await _context.LogsEventos
                                     .OrderByDescending(l => l.Timestamp) // Mais recentes primeiro
                                     .Skip((pageNumber - 1) * pageSize)    // Lógica da paginação
                                     .Take(pageSize)                       // Pegar apenas o tamanho da página
                                     .ToListAsync();

                // Decidi não chamar _loggingService.RegistrarEventoAsync() aqui
                // para evitar um possível loop infinito se o próprio ato de logar falhar e tentar logar essa falha.
                // O acesso a este endpoint já está protegido pela API Key, o que já é uma forma de controle.

                return Ok(logs);
            }
            catch (Exception ex)
            {
                // Se der um erro aqui ao buscar os logs, é algo mais sério.
                // Logar no console é uma medida de emergência, já que o serviço de log pode estar envolvido.
                // Em um sistema de produção, teríamos um mecanismo de log alternativo (ex: log em arquivo, Application Insights).
                Console.WriteLine($"ERRO CRÍTICO NO LOGSCONTROLLER ao tentar buscar logs: {ex.ToString()}"); // Log completo da exceção no console do servidor
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao tentar listar os logs da aplicação." });
            }
        }
    }
}