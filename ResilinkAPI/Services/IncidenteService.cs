using Microsoft.EntityFrameworkCore;
using ResilinkAPI.Data;
using ResilinkAPI.DTOs;
using ResilinkAPI.Models;

namespace ResilinkAPI.Services
{
    public class IncidenteService : IIncidenteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public IncidenteService(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<Incidente> RegistrarIncidenteAsync(CriarIncidenteRequestDto dto, string? requisitanteId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            // Adicione mais validações do DTO aqui se não estiverem sendo feitas por DataAnnotations
            // ou se forem validações de negócio mais complexas.

            var incidente = new Incidente
            {
                Tipo = dto.Tipo,
                Descricao = dto.Descricao,
                // Latitude = dto.Latitude, // Descomente se adicionou Latitude/Longitude ao DTO e Modelo
                // Longitude = dto.Longitude,
                DataHoraOcorrencia = dto.DataHoraOcorrencia,
                Status = "Aberto", // Status inicial padrão
                DataHoraRegistro = DateTime.UtcNow
                // UsuarioReportouId = requisitanteId; // Se tiver um sistema de usuários cidadãos
            };

            try
            {
                _context.Incidentes.Add(incidente);
                await _context.SaveChangesAsync();
                await _loggingService.RegistrarEventoAsync("Incidente Criado", $"ID: {incidente.Id}, Tipo: {incidente.Tipo}", requisitanteId);
                return incidente;
            }
            catch (DbUpdateException ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao registrar incidente: {ex.InnerException?.Message ?? ex.Message}", requisitanteId);
                throw new Exception("Erro ao salvar incidente no banco de dados.", ex);
            }
            // Adicionar mais blocos catch para exceções específicas se necessário
        }

        public async Task<Incidente?> GetIncidentePorIdAsync(int id)
        {
            try
            {
                return await _context.Incidentes.FindAsync(id);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao buscar incidente ID {id}: {ex.Message}", "Sistema");
                throw new Exception("Erro ao buscar incidente.", ex);
            }
        }

        public async Task<IEnumerable<Incidente>> ListarIncidentesAsync(string? status = null, string? tipo = null)
        {
            try
            {
                var query = _context.Incidentes.AsQueryable();
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(i => i.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrEmpty(tipo))
                {
                    query = query.Where(i => i.Tipo.Contains(tipo, StringComparison.OrdinalIgnoreCase));
                }
                return await query.OrderByDescending(i => i.DataHoraRegistro).ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao listar incidentes: {ex.Message}", "Sistema");
                throw new Exception("Erro ao listar incidentes.", ex);
            }
        }

        public async Task<bool> AtualizarStatusIncidenteAsync(int id, string novoStatus, string usuarioAdminId)
        {
            if (string.IsNullOrWhiteSpace(novoStatus))
                throw new ArgumentException("Novo status não pode ser vazio.", nameof(novoStatus));

            try
            {
                var incidente = await _context.Incidentes.FindAsync(id);
                if (incidente == null)
                {
                    await _loggingService.RegistrarEventoAsync("Atualização Falhou", $"Incidente ID {id} não encontrado.", usuarioAdminId);
                    return false;
                }

                string statusAntigo = incidente.Status;
                incidente.Status = novoStatus;
                await _context.SaveChangesAsync();
                await _loggingService.RegistrarEventoAsync("Status Incidente Atualizado", $"ID: {id}, De: {statusAntigo}, Para: {novoStatus}", usuarioAdminId);
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao atualizar status do incidente ID {id}: {ex.Message}", usuarioAdminId);
                throw new Exception("Erro ao atualizar status do incidente.", ex);
            }
        }
    }
}
