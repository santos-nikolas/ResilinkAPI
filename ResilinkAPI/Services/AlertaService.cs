using Microsoft.EntityFrameworkCore;
using ResilinkAPI.Data;
using ResilinkAPI.DTOs;
using ResilinkAPI.Models;

namespace ResilinkAPI.Services
{
    public class AlertaService : IAlertaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public AlertaService(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<Alerta> CriarAlertaAsync(CriarAlertaRequestDto alertaDto, string emissorId)
        {
            if (alertaDto == null) throw new ArgumentNullException(nameof(alertaDto));
            // Adicionar mais validações do DTO aqui

            var alerta = new Alerta
            {
                Mensagem = alertaDto.Mensagem,
                NivelSeveridade = alertaDto.NivelSeveridade,
                AreaAbrangencia = alertaDto.AreaAbrangencia,
                DataHoraEmissao = DateTime.UtcNow,
                EmissorId = emissorId
            };

            try
            {
                _context.Alertas.Add(alerta);
                await _context.SaveChangesAsync();
                await _loggingService.RegistrarEventoAsync("Alerta Criado", $"ID: {alerta.Id}, Severidade: {alerta.NivelSeveridade}", emissorId);
                // Simulação de "disparo"
                Console.WriteLine($"ALERTA SIMULADO: '{alerta.Mensagem}' para '{alerta.AreaAbrangencia}'");
                return alerta;
            }
            catch (DbUpdateException ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao criar alerta: {ex.InnerException?.Message ?? ex.Message}", emissorId);
                throw new Exception("Erro ao salvar alerta no banco de dados.", ex);
            }
        }

        public async Task<Alerta?> GetAlertaPorIdAsync(int id)
        {
            try
            {
                return await _context.Alertas.FindAsync(id);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao buscar alerta ID {id}: {ex.Message}", "Sistema");
                throw new Exception("Erro ao buscar alerta.", ex);
            }
        }

        public async Task<IEnumerable<Alerta>> ListarAlertasAsync()
        {
            try
            {
                return await _context.Alertas.OrderByDescending(a => a.DataHoraEmissao).ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao listar alertas: {ex.Message}", "Sistema");
                throw new Exception("Erro ao listar alertas.", ex);
            }
        }
    }
}
