using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.Models
{
    public class Incidente
    {
        [Key] // Define Id como chave primária
        public int Id { get; set; }

        [Required(ErrorMessage = "O tipo do incidente é obrigatório.")]
        [StringLength(100, ErrorMessage = "O tipo do incidente não pode exceder 100 caracteres.")]
        public string Tipo { get; set; } // Ex: "Falta de Energia", "Alagamento", "Árvore Caída"

        [Required(ErrorMessage = "A descrição do incidente é obrigatória.")]
        [StringLength(1000, ErrorMessage = "A descrição não pode exceder 1000 caracteres.")]
        public string Descricao { get; set; }

        // Para simplificar, vamos omitir Latitude/Longitude por enquanto,
        // mas em uma versão completa do Resilink seriam essenciais.
        // public double Latitude { get; set; }
        // public double Longitude { get; set; }

        [Required(ErrorMessage = "A data e hora da ocorrência são obrigatórias.")]
        public DateTime DataHoraOcorrencia { get; set; }

        [Required(ErrorMessage = "O status do incidente é obrigatório.")]
        [StringLength(50, ErrorMessage = "O status não pode exceder 50 caracteres.")]
        public string Status { get; set; } = "Aberto"; // Valores padrão: "Aberto", "Em Análise", "Resolvido"

        public DateTime DataHoraRegistro { get; set; } = DateTime.UtcNow;

        // Opcional: Quem reportou, se aplicável e se houver sistema de usuários cidadãos
        // public string? UsuarioReportouId { get; set; }
    }
}
