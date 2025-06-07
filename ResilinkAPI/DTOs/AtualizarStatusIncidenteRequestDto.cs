using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.DTOs
{
    public class AtualizarStatusIncidenteRequestDto
    {
        [Required(ErrorMessage = "O novo status é obrigatório.")]
        [StringLength(50, ErrorMessage = "O status não pode exceder 50 caracteres.")]
        public string NovoStatus { get; set; }
    }
}
