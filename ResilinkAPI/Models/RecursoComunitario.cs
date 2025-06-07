using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.Models
{
    public class RecursoComunitario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O tipo do recurso é obrigatório.")]
        [StringLength(100)]
        public string TipoRecurso { get; set; } // Ex: "Ponto de Recarga Celular", "Abrigo Temporário"

        [Required(ErrorMessage = "A descrição do recurso é obrigatória.")]
        [StringLength(500)]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "A localização descritiva é obrigatória.")]
        [StringLength(200)]
        public string LocalizacaoDescritiva { get; set; }

        [Required(ErrorMessage = "O contato do disponibilizador é obrigatório.")]
        [StringLength(100)]
        public string ContatoDisponibilizador { get; set; }

        public bool Disponivel { get; set; } = true;

        [Required(ErrorMessage = "O status de moderação é obrigatório.")]
        [StringLength(50)]
        public string StatusModeracao { get; set; } = "Pendente"; // "Pendente", "Aprovado", "Rejeitado"

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public string? UsuarioIdDisponibilizador { get; set; }
    }
}
