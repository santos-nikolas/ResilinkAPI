using ResilinkAPI.Data;
using ResilinkAPI.Models;

namespace ResilinkAPI.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ApplicationDbContext _context;
        // private readonly ILogger<LoggingService> _aspNetCoreLogger; // Para logar no console do ASP.NET Core

        public LoggingService(ApplicationDbContext context /*, ILogger<LoggingService> aspNetCoreLogger*/)
        {
            _context = context;
            // _aspNetCoreLogger = aspNetCoreLogger;
        }

        public async Task RegistrarEventoAsync(string tipoEvento, string detalhes, string? usuarioId = null)
        {
            // _aspNetCoreLogger.LogInformation($"Tentativa de Log: Tipo='{tipoEvento}', User='{usuarioId ?? "Sistema"}', Detalhes='{detalhes}'");

            if (string.IsNullOrWhiteSpace(tipoEvento))
            {
                // _aspNetCoreLogger?.LogWarning("Tentativa de registrar evento com tipoEvento vazio.");
                // Decidir se lança exceção ou apenas ignora
                return;
            }
            if (string.IsNullOrWhiteSpace(detalhes))
            {
                // _aspNetCoreLogger?.LogWarning($"Tentativa de registrar evento '{tipoEvento}' com detalhes vazios.");
                return;
            }

            var logEvento = new LogEvento
            {
                Timestamp = DateTime.UtcNow,
                TipoEvento = tipoEvento,
                Detalhes = detalhes.Length > 1000 ? detalhes.Substring(0, 1000) : detalhes, // Evitar strings muito longas
                UsuarioId = usuarioId
            };

            try
            {
                _context.LogsEventos.Add(logEvento);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // _aspNetCoreLogger?.LogError(ex, $"Falha CRÍTICA ao salvar LogEvento no BD. Tipo='{tipoEvento}', User='{usuarioId ?? "Sistema"}'.");
                // Em um sistema real, teríamos um mecanismo de fallback para logging (ex: arquivo local)
                Console.WriteLine($"ERRO CRÍTICO AO SALVAR LOG NO BANCO: {ex.Message}"); // Log de emergência no console
                // Não relançar a exceção para não quebrar a operação principal que tentou logar, a menos que o log seja mandatório para a transação.
            }
        }
    }
}
