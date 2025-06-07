using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.DTOs
{
    public class CriarIncidenteRequestDto
    {
        [Required(ErrorMessage = "O tipo do incidente é obrigatório.")]
        [StringLength(100, ErrorMessage = "O tipo do incidente não pode exceder 100 caracteres.")]
        public string Tipo { get; set; }

        [Required(ErrorMessage = "A descrição do incidente é obrigatória.")]
        [StringLength(1000, ErrorMessage = "A descrição não pode exceder 1000 caracteres.")]
        public string Descricao { get; set; }

        // Removido Latitude/Longitude para simplificar conforme modelo Incidente.cs
        // Se você os re-adicionar ao modelo Incidente, adicione-os aqui também com [Required]
        // public double Latitude { get; set; }
        // public double Longitude { get; set; }

        [Required(ErrorMessage = "A data e hora da ocorrência são obrigatórias.")]
        public DateTime DataHoraOcorrencia { get; set; }

        // Anexo de mídia é opcional
        [StringLength(2048, ErrorMessage = "A URL da mídia não pode exceder 2048 caracteres.")]
        public string? AnexoMidiaUrl { get; set; }
    }
}
