using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.Models
{
    public class Alerta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A mensagem do alerta é obrigatória.")]
        [StringLength(500, ErrorMessage = "A mensagem do alerta não pode exceder 500 caracteres.")]
        public string Mensagem { get; set; }

        [Required(ErrorMessage = "O nível de severidade é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nível de severidade não pode exceder 50 caracteres.")]
        public string NivelSeveridade { get; set; } // Ex: "Informativo", "Atenção", "Perigo"

        [Required(ErrorMessage = "A área de abrangência é obrigatória.")]
        [StringLength(200, ErrorMessage = "A área de abrangência não pode exceder 200 caracteres.")]
        public string AreaAbrangencia { get; set; } // Pode ser uma descrição textual, coordenadas, ou nome de região

        public DateTime DataHoraEmissao { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "O ID do emissor é obrigatório.")]
        public string EmissorId { get; set; } // Identificador do administrador/sistema que emitiu
    }
}
