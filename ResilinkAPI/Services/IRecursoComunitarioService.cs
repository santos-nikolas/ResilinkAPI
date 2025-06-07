using ResilinkAPI.DTOs;
using ResilinkAPI.Models;

namespace ResilinkAPI.Services
{
    public interface IRecursoComunitarioService
    {
        Task<RecursoComunitario> OferecerRecursoAsync(CriarRecursoComunitarioRequestDto recursoDto, string? requisitanteId);
        Task<IEnumerable<RecursoComunitario>> ListarRecursosDisponiveisAsync(); // Apenas os aprovados e disponíveis
        Task<RecursoComunitario?> GetRecursoPorIdAsync(int id); // Adicionado para o CreatedAtAction
        Task<bool> ModerarRecursoAsync(int id, string novoStatusModeracao, string adminId);
    }
}
