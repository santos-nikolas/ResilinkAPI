// Controllers/RecursosComunitariosController.cs
using Microsoft.AspNetCore.Mvc;
using ResilinkAPI.Attributes;         // Meu atributo para autenticação via API Key
using ResilinkAPI.DTOs;             // Para os Data Transfer Objects
using ResilinkAPI.Models;           // Para o modelo RecursoComunitario
using ResilinkAPI.Services;         // Para IRecursoComunitarioService e ILoggingService
using System;
using System.Collections.Generic;     // Para IEnumerable
using System.Threading.Tasks;       // Para Task

namespace ResilinkAPI.Controllers
{
    /// <summary>
    /// Controlador para gerenciar Recursos Comunitários na plataforma Resilink.
    /// Permite que usuários (ou sistemas autenticados) ofereçam recursos, listem os disponíveis
    /// e que administradores moderem esses recursos.
    /// O acesso é protegido por API Key.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Rota base será /api/RecursosComunitarios
    [ApiKeyAuth] // Autenticação por API Key para todos os endpoints
    public class RecursosComunitariosController : ControllerBase
    {
        private readonly IRecursoComunitarioService _recursoService;
        private readonly ILoggingService _loggingService;
        // O ApplicationDbContext foi removido daqui pois GetRecursoPorId agora está no IRecursoComunitarioService

        /// <summary>
        /// Construtor do RecursosComunitariosController.
        /// </summary>
        /// <param name="recursoService">Serviço para lógica de negócio de recursos comunitários.</param>
        /// <param name="loggingService">Serviço para registro de logs de eventos.</param>
        public RecursosComunitariosController(IRecursoComunitarioService recursoService, ILoggingService loggingService)
        {
            _recursoService = recursoService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Permite que um usuário/sistema ofereça um novo recurso comunitário.
        /// </summary>
        /// <remarks>
        /// Exemplo de corpo da requisição:
        ///
        ///     POST /api/RecursosComunitarios
        ///     {
        ///        "tipoRecurso": "Ponto de Recarga Celular",
        ///        "descricao": "Tomada disponível na varanda, das 8h às 18h.",
        ///        "localizacaoDescritiva": "Rua das Flores, 123, portão azul",
        ///        "contatoDisponibilizador": "João - (11) 99999-8888"
        ///     }
        ///
        /// O recurso criado entra com status de moderação "Pendente".
        /// </remarks>
        /// <param name="dto">Dados do recurso comunitário a ser oferecido.</param>
        /// <returns>Retorna o recurso criado com status 201 se bem-sucedido.</returns>
        /// <response code="201">Recurso oferecido com sucesso. Retorna o recurso criado.</response>
        /// <response code="400">Se os dados fornecidos forem inválidos.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPost]
        [ProducesResponseType(typeof(RecursoComunitario), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OferecerRecurso([FromBody] CriarRecursoComunitarioRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_Default";
            try
            {
                RecursoComunitario recurso = await _recursoService.OferecerRecursoAsync(dto, requisitanteId);
                // Retorno 201 Created, com a URL para buscar o novo recurso e o próprio recurso
                return CreatedAtAction(nameof(GetRecursoPorId), new { id = recurso.Id }, recurso);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar OFERECER RECURSO: {ex.Message}", requisitanteId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao tentar oferecer o recurso." });
            }
        }

        /// <summary>
        /// Lista todos os recursos comunitários disponíveis (com status de moderação "Aprovado" e marcados como disponíveis).
        /// </summary>
        /// <returns>Uma lista de recursos comunitários.</returns>
        /// <response code="200">Retorna a lista de recursos comunitários disponíveis.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RecursoComunitario>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarRecursosDisponiveis()
        {
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_Default";
            try
            {
                IEnumerable<RecursoComunitario> recursos = await _recursoService.ListarRecursosDisponiveisAsync();
                return Ok(recursos);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar LISTAR RECURSOS COMUNITÁRIOS: {ex.Message}", requisitanteId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao listar os recursos comunitários." });
            }
        }

        /// <summary>
        /// Busca um recurso comunitário específico pelo seu ID.
        /// </summary>
        /// <param name="id">O ID numérico do recurso comunitário.</param>
        /// <returns>Retorna o recurso comunitário se encontrado (200 OK), ou 404 Not Found.</returns>
        /// <response code="200">Retorna o recurso comunitário encontrado.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="404">Se o recurso com o ID especificado não for encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RecursoComunitario), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecursoPorId(int id)
        {
            string requisitanteId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "APIUser_Default";
            try
            {
                var recurso = await _recursoService.GetRecursoPorIdAsync(id); // Chamando o método do serviço
                if (recurso == null)
                {
                    await _loggingService.RegistrarEventoAsync("Busca Recurso Falhou", $"Recurso Comunitário com ID {id} não foi encontrado.", requisitanteId);
                    return NotFound(new { message = $"Recurso comunitário com ID {id} não encontrado." });
                }
                return Ok(recurso);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar BUSCAR RECURSO COMUNITÁRIO por ID {id}: {ex.Message}", requisitanteId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao buscar o recurso comunitário." });
            }
        }

        /// <summary>
        /// Modera um recurso comunitário, alterando seu status de moderação (ex: para "Aprovado" ou "Rejeitado").
        /// Este endpoint normalmente seria acessado por um administrador.
        /// </summary>
        /// <param name="id">O ID do recurso comunitário a ser moderado.</param>
        /// <param name="dto">Objeto contendo o novo status de moderação.</param>
        /// <returns>Retorna 204 No Content se a moderação for bem-sucedida, 404 Not Found se o recurso não existir, ou 400 Bad Request para dados inválidos.</returns>
        /// <response code="204">Recurso moderado com sucesso.</response>
        /// <response code="400">Se o novo status de moderação fornecido for inválido.</response>
        /// <response code="401">Não autorizado (API Key ausente ou inválida).</response>
        /// <response code="404">Se o recurso com o ID especificado não for encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPut("{id}/moderar")]
        // [Authorize(Roles = "Resilink_Administrators")] // Exemplo para autorização futura mais granular
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ModerarRecurso(int id, [FromBody] ModerarRecursoRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string adminId = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "AdminAPIUser_Default";
            try
            {
                bool sucesso = await _recursoService.ModerarRecursoAsync(id, dto.NovoStatusModeracao, adminId);
                if (!sucesso)
                {
                    // O serviço já deve ter logado a falha de "não encontrado"
                    return NotFound(new { message = $"Recurso comunitário com ID {id} não encontrado ou não pôde ser moderado." });
                }
                // Se deu tudo certo, 204 No Content é o ideal
                return NoContent();
            }
            catch (ArgumentException ex) // Captura erros de validação do serviço (ex: status inválido)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Controller", $"Falha ao tentar MODERAR RECURSO COMUNITÁRIO ID {id}: {ex.Message}", adminId);
                return StatusCode(500, new { message = "Ocorreu um erro inesperado ao tentar moderar o recurso comunitário." });
            }
        }
    }
}