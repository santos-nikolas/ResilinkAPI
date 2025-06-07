// Controllers/IncidentesController.cs
using Microsoft.AspNetCore.Mvc;
using ResilinkAPI.Attributes;         // Meu atributo para autenticação via API Key
using ResilinkAPI.DTOs;             // Para os Data Transfer Objects como CriarIncidenteRequestDto
using ResilinkAPI.Models;           // Para o modelo Incidente (usado no tipo de retorno)
using ResilinkAPI.Services;         // Para IIncidenteService e ILoggingService
using System;
using System.Collections.Generic;     // Para IEnumerable
using System.Threading.Tasks;       // Para Task

namespace ResilinkAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciar os incidentes reportados na plataforma Resilink.
    /// Inclui operações como registrar novos incidentes, buscar, listar e atualizar status.
    /// Todo o acesso é protegido por API Key.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Rota base será /api/Incidentes
    [ApiKeyAuth] // Aplica o filtro de autenticação por API Key para todos os métodos neste controller
    public class IncidentesController : ControllerBase
    {
        private readonly IIncidenteService _incidenteService;
        private readonly ILoggingService _loggingService;

        /// <summary>
        /// Construtor do IncidentesController.
        /// </summary>
        /// <param name="incidenteService">Serviço injetado para lidar com a lógica de negócios de incidentes.</param>
        /// <param name="loggingService">Serviço injetado para registrar logs de eventos da aplicação.</param>
        public IncidentesController(IIncidenteService incidenteService, ILoggingService loggingService)
        {
            _incidenteService = incidenteService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Registra um novo incidente na plataforma.
        /// </summary>
        /// <remarks>
        /// Exemplo de corpo da requisição:
        ///
        ///     POST /api/Incidentes
        ///     {
        ///        "tipo": "Falta de Energia",
        ///        "descricao": "Poste caído na Rua das Palmeiras, afetando o quarteirão.",
        ///        "dataHoraOcorrencia": "2024-06-04T14:30:00Z"
        ///     }
        ///
        /// </remarks>
        /// <param name="incidenteDto">Dados do incidente a ser criado.</param>
        /// <returns>Retorna o incidente criado com status 201 se bem-sucedido.</returns>
        /// <response code="201">Incidente registrado com sucesso. Retorna o incidente criado.</response>
        /// <response code="400">Se os dados fornecidos para o incidente forem inválidos.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Incidente), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CriarIncidente([FromBody] CriarIncidenteRequestDto incidenteDto)
        {
            // A validação do ModelState (baseada em DataAnnotations no DTO) é feita automaticamente pelo [ApiController]
            // Se for inválido, um 400 já é retornado. Posso adicionar um log se quiser.
            if (!ModelState.IsValid)
            {
                // Considerar logar os erros de validação se for útil para depuração.
                return BadRequest(ModelState);
            }

            // Pego o identificador do "usuário" da API Key para rastreabilidade
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_NaoIdentificado";
            try
            {
                Incidente novoIncidente = await _incidenteService.RegistrarIncidenteAsync(incidenteDto, requisitanteId);
                // Se deu tudo certo, retorno 201 Created, com o link para o novo recurso e o próprio recurso
                return CreatedAtAction(nameof(GetIncidentePorId), new { id = novoIncidente.Id }, novoIncidente);
            }
            catch (ArgumentException ex) // Erros de validação específicos da lógica de negócio no serviço
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Qualquer outro erro inesperado
            {
                // Logar o erro para ajudar na investigação depois
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar CRIAR INCIDENTE: {ex.Message}", requisitanteId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao tentar registrar o incidente. Por favor, tente novamente mais tarde." });
            }
        }

        /// <summary>
        /// Busca um incidente específico pelo seu ID.
        /// </summary>
        /// <param name="id">O ID numérico do incidente.</param>
        /// <returns>Retorna o incidente se encontrado (200 OK), ou 404 Not Found.</returns>
        /// <response code="200">Retorna o incidente encontrado.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="404">Se o incidente com o ID especificado não for encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Incidente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIncidentePorId(int id)
        {
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_NaoIdentificado";
            try
            {
                var incidente = await _incidenteService.GetIncidentePorIdAsync(id);
                if (incidente == null)
                {
                    // Log é importante para saber se estão buscando IDs que não existem
                    await _loggingService.RegistrarEventoAsync("Busca Incidente Falhou", $"Incidente com ID {id} não foi encontrado.", requisitanteId);
                    return NotFound(new { message = $"Incidente com ID {id} não encontrado." });
                }
                return Ok(incidente);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar BUSCAR INCIDENTE por ID {id}: {ex.Message}", requisitanteId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao buscar o incidente." });
            }
        }

        /// <summary>
        /// Lista todos os incidentes registrados, com filtros opcionais por status e tipo.
        /// </summary>
        /// <param name="status">Filtra incidentes pelo status (ex: "Aberto", "Resolvido"). Opcional.</param>
        /// <param name="tipo">Filtra incidentes pelo tipo (ex: "Falta de Energia"). Opcional, busca parcial.</param>
        /// <returns>Uma lista de incidentes (200 OK).</returns>
        /// <response code="200">Retorna a lista de incidentes conforme os filtros.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Incidente>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarIncidentes([FromQuery] string? status = null, [FromQuery] string? tipo = null)
        {
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_NaoIdentificado";
            try
            {
                IEnumerable<Incidente> incidentes = await _incidenteService.ListarIncidentesAsync(status, tipo);
                return Ok(incidentes);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar LISTAR INCIDENTES: {ex.Message}", requisitanteId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao listar os incidentes." });
            }
        }

        /// <summary>
        /// Atualiza o status de um incidente existente.
        /// </summary>
        /// <param name="id">O ID do incidente a ser atualizado.</param>
        /// <param name="dto">Objeto contendo o novo status para o incidente.</param>
        /// <returns>Retorna 204 No Content se a atualização for bem-sucedida, 404 Not Found se o incidente não existir, ou 400 Bad Request para dados inválidos.</returns>
        /// <response code="204">Status do incidente atualizado com sucesso.</response>
        /// <response code="400">Se o novo status fornecido for inválido.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="404">Se o incidente com o ID especificado não for encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPut("{id}/status")]
        // Geralmente, uma ação de atualização de status seria para um usuário administrativo
        // [Authorize(Roles = "Resilink_Administrators")] // Exemplo para autorização futura
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AtualizarStatusIncidente(int id, [FromBody] AtualizarStatusIncidenteRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Identificador do administrador ou sistema que está fazendo a atualização
            string adminId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "AdminAPIUser_Default";
            try
            {
                bool sucesso = await _incidenteService.AtualizarStatusIncidenteAsync(id, dto.NovoStatus, adminId);
                if (!sucesso)
                {
                    // O serviço já deve ter logado a falha de "não encontrado"
                    return NotFound(new { message = $"Incidente com ID {id} não encontrado ou não pôde ser atualizado." });
                }
                // Se deu tudo certo, 204 No Content é o ideal para PUTs que não retornam o objeto
                return NoContent();
            }
            catch (ArgumentException ex) // Erros de validação do novo status, por exemplo
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar ATUALIZAR STATUS do INCIDENTE ID {id}: {ex.Message}", adminId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao tentar atualizar o status do incidente." });
            }
        }
    }
}