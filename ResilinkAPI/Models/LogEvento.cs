using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.Models
{
    public class LogEvento
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "O tipo do evento é obrigatório.")]
        [StringLength(100, ErrorMessage = "O tipo do evento não pode exceder 100 caracteres.")]
        public string TipoEvento { get; set; } // Ex: "Login Sucesso", "Login Falha", "Incidente Criado", "API Key Acesso"

        [Required(ErrorMessage = "Os detalhes do evento são obrigatórios.")]
        [StringLength(1000, ErrorMessage = "Os detalhes não podem exceder 1000 caracteres.")]
        public string Detalhes { get; set; }

        [StringLength(100)]
        public string? UsuarioId { get; set; } // Identificador do usuário/sistema que realizou a ação, se aplicável
    }
}
