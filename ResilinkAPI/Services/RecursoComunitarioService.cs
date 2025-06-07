using Microsoft.EntityFrameworkCore;
using ResilinkAPI.Data;
using ResilinkAPI.DTOs;
using ResilinkAPI.Models;

namespace ResilinkAPI.Services
{
    public class RecursoComunitarioService : IRecursoComunitarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public RecursoComunitarioService(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<RecursoComunitario> OferecerRecursoAsync(CriarRecursoComunitarioRequestDto dto, string? requisitanteId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            // Adicionar mais validações

            var recurso = new RecursoComunitario
            {
                TipoRecurso = dto.TipoRecurso,
                Descricao = dto.Descricao,
                LocalizacaoDescritiva = dto.LocalizacaoDescritiva,
                ContatoDisponibilizador = dto.ContatoDisponibilizador,
                UsuarioIdDisponibilizador = requisitanteId,
                Disponivel = true,
                StatusModeracao = "Pendente", // Todo novo recurso precisa ser moderado
                DataCriacao = DateTime.UtcNow
            };

            try
            {
                _context.RecursosComunitarios.Add(recurso);
                await _context.SaveChangesAsync();
                await _loggingService.RegistrarEventoAsync("Recurso Oferecido", $"ID: {recurso.Id}, Tipo: {recurso.TipoRecurso}", requisitanteId);
                return recurso;
            }
            catch (DbUpdateException ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao oferecer recurso: {ex.InnerException?.Message ?? ex.Message}", requisitanteId);
                throw new Exception("Erro ao salvar recurso no banco de dados.", ex);
            }
        }

        public async Task<IEnumerable<RecursoComunitario>> ListarRecursosDisponiveisAsync()
        {
            try
            {
                return await _context.RecursosComunitarios
                                     .Where(r => r.StatusModeracao == "Aprovado" && r.Disponivel)
                                     .OrderByDescending(r => r.DataCriacao)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao listar recursos disponíveis: {ex.Message}", "Sistema");
                throw new Exception("Erro ao listar recursos disponíveis.", ex);
            }
        }

        public async Task<RecursoComunitario?> GetRecursoPorIdAsync(int id)
        {
            try
            {
                return await _context.RecursosComunitarios.FindAsync(id);
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao buscar recurso comunitário ID {id}: {ex.Message}", "Sistema");
                throw new Exception($"Erro ao buscar recurso comunitário ID {id}.", ex);
            }
        }

        public async Task<bool> ModerarRecursoAsync(int id, string novoStatusModeracao, string adminId)
        {
            if (string.IsNullOrWhiteSpace(novoStatusModeracao) ||
                (novoStatusModeracao != "Aprovado" && novoStatusModeracao != "Rejeitado"))
                throw new ArgumentException("Status de moderação inválido. Use 'Aprovado' ou 'Rejeitado'.", nameof(novoStatusModeracao));

            try
            {
                var recurso = await _context.RecursosComunitarios.FindAsync(id);
                if (recurso == null)
                {
                    await _loggingService.RegistrarEventoAsync("Moderação Falhou", $"Recurso ID {id} não encontrado.", adminId);
                    return false;
                }

                string statusAntigo = recurso.StatusModeracao;
                recurso.StatusModeracao = novoStatusModeracao;
                if (novoStatusModeracao == "Rejeitado")
                {
                    recurso.Disponivel = false;
                }
                else if (novoStatusModeracao == "Aprovado")
                {
                    recurso.Disponivel = true; // Garante que está disponível se aprovado
                }

                await _context.SaveChangesAsync();
                await _loggingService.RegistrarEventoAsync("Recurso Moderado", $"ID: {id}, De: {statusAntigo}, Para: {novoStatusModeracao}", adminId);
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.RegistrarEventoAsync("Erro BD", $"Falha ao moderar recurso ID {id}: {ex.Message}", adminId);
                throw new Exception("Erro ao moderar recurso.", ex);
            }
        }
    }
}
