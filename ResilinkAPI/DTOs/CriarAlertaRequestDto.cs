using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.DTOs
{
    public class CriarAlertaRequestDto
    {
        [Required(ErrorMessage = "A mensagem do alerta é obrigatória.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "A mensagem deve ter entre 10 e 500 caracteres.")]
        public string Mensagem { get; set; }

        [Required(ErrorMessage = "O nível de severidade é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nível de severidade não pode exceder 50 caracteres.")]
        public string NivelSeveridade { get; set; } // Ex: "Informativo", "Atenção", "Perigo"

        [Required(ErrorMessage = "A área de abrangência é obrigatória.")]
        [StringLength(200, ErrorMessage = "A área de abrangência não pode exceder 200 caracteres.")]
        public string AreaAbrangencia { get; set; }
    }
}
