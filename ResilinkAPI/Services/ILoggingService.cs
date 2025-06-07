namespace ResilinkAPI.Services
{
    public interface ILoggingService
    {
        Task RegistrarEventoAsync(string tipoEvento, string detalhes, string? usuarioId = null);
    }
}
