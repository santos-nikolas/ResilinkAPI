// Controllers/RelatoriosController.cs
using Microsoft.AspNetCore.Mvc;
using ResilinkAPI.Attributes;         // Meu atributo para autenticação via API Key
using ResilinkAPI.DTOs;             // Para o RelatorioStatusGeralDto
using ResilinkAPI.Services;         // Para IRelatorioService e ILoggingService
using System;
using System.Threading.Tasks;       // Para Task

namespace ResilinkAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela geração e fornecimento de relatórios agregados
    /// sobre o status da plataforma Resilink e os eventos gerenciados.
    /// O acesso é protegido por API Key.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Rota base será /api/Relatorios
    [ApiKeyAuth] // Protege todos os endpoints neste controller
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;
        private readonly ILoggingService _loggingService;

        /// <summary>
        /// Construtor do RelatoriosController.
        /// </summary>
        /// <param name="relatorioService">Serviço injetado para lidar com a lógica de geração de relatórios.</param>
        /// <param name="loggingService">Serviço injetado para registrar logs de eventos da aplicação.</param>
        public RelatoriosController(IRelatorioService relatorioService, ILoggingService loggingService)
        {
            _relatorioService = relatorioService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Gera e retorna um relatório de status geral da plataforma Resilink.
        /// </summary>
        /// <remarks>
        /// Este relatório inclui informações agregadas como:
        /// - Número total de incidentes abertos.
        /// - Número total de alertas ativos (simulação simplificada).
        /// - Contagem de usuários ativos nas últimas 24 horas (simulação baseada em logs de acesso).
        /// - Distribuição de incidentes por tipo.
        /// 
        /// Ideal para dashboards administrativos ou para obter uma visão rápida da situação.
        /// </remarks>
        /// <returns>Um objeto `RelatorioStatusGeralDto` contendo os dados agregados.</returns>
        /// <response code="200">Retorna o relatório de status geral.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="500">Erro interno no servidor ao tentar gerar o relatório.</response>
        [HttpGet("status-geral")]
        [ProducesResponseType(typeof(RelatorioStatusGeralDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatusGeral()
        {
            // Identificador do "usuário" da API Key para rastreabilidade
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_Relatorio";
            try
            {
                RelatorioStatusGeralDto relatorio = await _relatorioService.GerarRelatorioStatusGeralAsync();
                // O log de "Relatório Gerado" já é feito dentro do _relatorioService, o que é uma boa prática.
                return Ok(relatorio);
            }
            catch (Exception ex) // Captura exceções que possam ocorrer no serviço ou no controller
            {
                // Logar o erro antes de retornar uma resposta genérica para o cliente
                await _loggingService.RegistrarEventoAsync(
                    "Erro Controller",
                    $"Falha interna ao tentar gerar o relatório de status geral. Mensagem: {ex.Message}",
                    requisitanteId
                );
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro inesperado ao tentar gerar o relatório de status." });
            }
        }
    }
}