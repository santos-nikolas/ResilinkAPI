using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.DTOs
{
    public class ModerarRecursoRequestDto
    {
        [Required(ErrorMessage = "O novo status de moderação é obrigatório.")]
        [RegularExpression("^(Aprovado|Rejeitado)$", ErrorMessage = "O status de moderação deve ser 'Aprovado' ou 'Rejeitado'.")]
        public string NovoStatusModeracao { get; set; }
    }
}
