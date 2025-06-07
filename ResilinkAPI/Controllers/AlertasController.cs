using Microsoft.AspNetCore.Mvc;
using ResilinkAPI.Attributes;
using ResilinkAPI.DTOs;
using ResilinkAPI.Models;
using ResilinkAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResilinkAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pelas operações relacionadas a Alertas na plataforma Resilink.
    /// Permite a criação, consulta e listagem de alertas de emergência ou informativos.
    /// O acesso a este controlador é protegido por autenticação via API Key.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiKeyAuth] // Aplica autenticação por API Key a todos os endpoints deste controller
    public class AlertasController : ControllerBase
    {
        private readonly IAlertaService _alertaService;
        private readonly ILoggingService _loggingService;

        /// <summary>
        /// Inicializa uma nova instância do controlador de Alertas.
        /// </summary>
        /// <param name="alertaService">Serviço para lógica de negócio de alertas.</param>
        /// <param name="loggingService">Serviço para registro de logs de eventos.</param>
        public AlertasController(IAlertaService alertaService, ILoggingService loggingService)
        {
            _alertaService = alertaService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Cria um novo alerta na plataforma.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que um usuário autenticado (via API Key com privilégios adequados)
        /// registre um novo alerta, especificando mensagem, nível de severidade e área de abrangência.
        /// 
        /// Exemplo de requisição:
        ///
        ///     POST /api/Alertas
        ///     {
        ///        "mensagem": "Risco de inundação na Zona Leste devido à forte chuva.",
        ///        "nivelSeveridade": "Perigo",
        ///        "areaAbrangencia": "Zona Leste, SP"
        ///     }
        ///
        /// </remarks>
        /// <param name="alertaDto">Objeto contendo os dados para a criação do novo alerta.</param>
        /// <returns>Retorna o alerta criado com status 201 Created em caso de sucesso,
        /// ou um erro 400 Bad Request se os dados forem inválidos,
        /// ou um erro 500 Internal Server Error em caso de falha interna.</returns>
        /// <response code="201">Retorna o alerta recém-criado.</response>
        /// <response code="400">Se os dados do alerta fornecidos forem inválidos.</response>
        /// <response code="401">Se a API Key não for fornecida ou for inválida.</response>
        /// <response code="500">Se ocorrer um erro inesperado no servidor.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Alerta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CriarAlerta([FromBody] CriarAlertaRequestDto alertaDto)
        {
            // A validação de DataAnnotations no DTO é feita automaticamente pelo [ApiController]
            if (!ModelState.IsValid)
            {
                // Logar o ModelState.Errors pode ser útil aqui para depuração
                return BadRequest(ModelState);
            }

            // Identificador do requisitante (ex: nome da API Key) obtido pelo atributo de autenticação
            string emissorId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "AdminAPIUser_Default";
            try
            {
                Alerta novoAlerta = await _alertaService.CriarAlertaAsync(alertaDto, emissorId);
                // Retorna 201 Created, com a URL para buscar o recurso criado e o próprio recurso no corpo
                return CreatedAtAction(nameof(GetAlertaPorId), new { id = novoAlerta.Id }, novoAlerta);
            }
            catch (ArgumentException ex) // Captura erros de validação de negócio do serviço
            {
                return BadRequest(new { message = $"Erro de validação ao criar alerta: {ex.Message}" });
            }
            catch (Exception ex) // Captura outras exceções inesperadas
            {
                // Logar a exceção completa (ex) é importante em um sistema real
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha interna ao tentar criar alerta. Mensagem: {ex.Message}", emissorId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua solicitação para criar o alerta." });
            }
        }

        /// <summary>
        /// Busca um alerta específico pelo seu ID.
        /// </summary>
        /// <param name="id">O ID do alerta a ser recuperado.</param>
        /// <returns>Retorna o alerta encontrado com status 200 OK,
        /// ou 404 Not Found se o alerta não existir,
        /// ou 500 Internal Server Error em caso de falha interna.</returns>
        /// <response code="200">Retorna o alerta solicitado.</response>
        /// <response code="401">Se a API Key não for fornecida ou for inválida.</response>
        /// <response code="404">Se o alerta com o ID especificado não for encontrado.</response>
        /// <response code="500">Se ocorrer um erro inesperado no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Alerta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAlertaPorId(int id)
        {
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_Default";
            try
            {
                var alerta = await _alertaService.GetAlertaPorIdAsync(id);
                if (alerta == null)
                {
                    // Logar a tentativa de busca por um ID não existente pode ser útil
                    await _loggingService.RegistrarEventoAsync("Busca Alerta", $"Alerta com ID {id} não foi encontrado.", requisitanteId);
                    return NotFound(new { message = $"Alerta com ID {id} não encontrado." });
                }
                return Ok(alerta);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha interna ao tentar buscar alerta com ID {id}. Mensagem: {ex.Message}", requisitanteId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua solicitação para buscar o alerta." });
            }
        }

        /// <summary>
        /// Lista todos os alertas registrados na plataforma, ordenados por data de emissão decrescente.
        /// </summary>
        /// <returns>Retorna uma lista de alertas com status 200 OK,
        /// ou 500 Internal Server Error em caso de falha interna.</returns>
        /// <response code="200">Retorna a lista de alertas.</response>
        /// <response code="401">Se a API Key não for fornecida ou for inválida.</response>
        /// <response code="500">Se ocorrer um erro inesperado no servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Alerta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarAlertas()
        {
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_Default";
            try
            {
                IEnumerable<Alerta> alertas = await _alertaService.ListarAlertasAsync();
                return Ok(alertas);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha interna ao tentar listar alertas. Mensagem: {ex.Message}", requisitanteId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua solicitação para listar os alertas." });
            }
        }
    }
}