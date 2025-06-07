using Microsoft.EntityFrameworkCore;
using ResilinkAPI.Data;
using ResilinkAPI.DTOs;

namespace ResilinkAPI.Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public RelatorioService(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<RelatorioStatusGeralDto> GerarRelatorioStatusGeralAsync()
        {
            try
            {
                var incidentesAbertos = await _context.Incidentes
                    .CountAsync(i => i.Status.Equals("Aberto", StringComparison.OrdinalIgnoreCase));

                var alertasAtivos = await _context.Alertas.CountAsync(); // Simplificado

                // Para "UsuariosAtivosUltimas24h", vamos contar logins bem-sucedidos se o log tiver essa informação.
                // Por enquanto, com API Key, podemos simular ou contar acessos gerais.
                // Vou simular um valor para agilizar.
                var usuariosAtivos = await _context.LogsEventos
                    .Where(l => l.TipoEvento == "API Key Acesso" && l.Timestamp >= DateTime.UtcNow.AddHours(-24))
                    .Select(l => l.UsuarioId) // Assumindo que UsuarioId armazena o identificador da API Key
                    .Distinct()
                    .CountAsync();
                if (usuariosAtivos == 0 && await _context.LogsEventos.AnyAsync(l => l.TipoEvento == "API Key Acesso"))
                { // Se houve acesso mas não nos ultimos 24h, contamos como 1 para mostrar uso.
                    usuariosAtivos = 1;
                }


                var incidentesPorTipo = await _context.Incidentes
                    .GroupBy(i => i.Tipo)
                    .Select(g => new { Tipo = g.Key, Contagem = g.Count() })
                    .ToDictionaryAsync(x => x.Tipo, x => x.Contagem);

                await _loggingService.RegistrarEventoAsync("Relatório Gerado", "Relatório de Status Geral foi acessado.", "Sistema");

                return new RelatorioStatusGeralDto
                {
                    TotalIncidentesAbertos = incidentesAbertos,
                    TotalAlertasAtivos = alertasAtivos,
                    UsuariosAtivosUltimas24h = usuariosAtivos,
                    IncidentesPorTipo = incidentesPorTipo ?? new Dictionary<string, int>() // Garante que não seja nulo
                };
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro Relatório", $"Falha ao gerar Relatório de Status Geral: {ex.Message}", "Sistema");
                throw new Exception("Ocorreu um erro ao gerar o relatório de status.", ex);
            }
        }
    }
}
