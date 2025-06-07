using ResilinkAPI.DTOs;
using ResilinkAPI.Models;

namespace ResilinkAPI.Services
{
    public interface IIncidenteService
    {
        Task<Incidente> RegistrarIncidenteAsync(CriarIncidenteRequestDto incidenteDto, string? requisitanteId);
        Task<Incidente?> GetIncidentePorIdAsync(int id);
        Task<IEnumerable<Incidente>> ListarIncidentesAsync(string? status = null, string? tipo = null);
        Task<bool> AtualizarStatusIncidenteAsync(int id, string novoStatus, string usuarioAdminId);
    }
}
