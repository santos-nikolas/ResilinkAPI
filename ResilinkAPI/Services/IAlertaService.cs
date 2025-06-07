using ResilinkAPI.DTOs;
using ResilinkAPI.Models;

namespace ResilinkAPI.Services
{
    public interface IAlertaService
    {
        Task<Alerta> CriarAlertaAsync(CriarAlertaRequestDto alertaDto, string emissorId);
        Task<Alerta?> GetAlertaPorIdAsync(int id);
        Task<IEnumerable<Alerta>> ListarAlertasAsync();
    }
}
