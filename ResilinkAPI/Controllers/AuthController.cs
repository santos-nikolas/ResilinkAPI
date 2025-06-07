// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ResilinkAPI.Attributes;      // Meu atributo para autenticar pela API Key
using ResilinkAPI.Services;      // Para usar o serviço de logging
using ResilinkAPI.DTOs;          // Para o DTO de resposta
using System.Threading.Tasks;

namespace ResilinkAPI.Controllers
{
    /// <summary>
    /// Controller responsável pela autenticação e verificação de acesso à API Resilink.
    /// Atualmente, utiliza um sistema de API Key para proteger os endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILoggingService _loggingService;
        // IConfiguration é injetado, mas não usado ativamente neste endpoint específico.
        // Mantido para consistência ou futuras necessidades de configuração neste controller.
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Construtor do AuthController. Injeta os serviços necessários.
        /// </summary>
        /// <param name="loggingService">Serviço para registrar logs de eventos.</param>
        /// <param name="configuration">Serviço de configuração da aplicação.</param>
        public AuthController(ILoggingService loggingService, IConfiguration configuration)
        {
            _loggingService = loggingService;
            _configuration = configuration;
        }

        /// <summary>
        /// Verifica se a API Key fornecida no header 'X-API-KEY' é válida.
        /// Este endpoint em si também é protegido pelo atributo [ApiKeyAuth],
        /// então se a execução chegar aqui, a chave já foi considerada válida pelo atributo.
        /// Serve como um "ping" autenticado ou um ponto de verificação para o cliente.
        /// </summary>
        /// <returns>
        /// Retorna um status 200 OK com uma mensagem de sucesso e um identificador
        /// associado à API Key utilizada, caso a chave seja válida.
        /// Retorna 401 Unauthorized se a API Key estiver ausente ou for inválida (tratado pelo atributo).
        /// </returns>
        /// <response code="200">API Key verificada com sucesso.</response>
        /// <response code="401">Não autorizado devido à API Key ausente ou inválida.</response>
        [HttpPost("verify-apikey")]
        [ApiKeyAuth] // Este atributo garante que só chegue aqui se a API Key no header X-API-KEY for válida
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyApiKey()
        {
            // O ApiKeyAuthAttribute deve popular HttpContext.Items["ApiKeyIdentifier"]
            // com alguma identificação da chave que passou na validação.
            string apiKeyUserIdentifier = HttpContext.Items["ApiKeyIdentifier"]?.ToString() ?? "ChaveAPI_Verificada";

            // Registro de log para auditoria do acesso
            await _loggingService.RegistrarEventoAsync(
                tipoEvento: "Verificação API Key",
                detalhes: "API Key validada com sucesso. Acesso permitido.",
                usuarioId: apiKeyUserIdentifier
            );

            // Preparando a resposta padronizada
            var response = new LoginResponseDto
            {
                Message = "API Key válida. Autenticação bem-sucedida.",
                AuthenticatedUser = apiKeyUserIdentifier
            };

            return Ok(response);
        }
    }
}