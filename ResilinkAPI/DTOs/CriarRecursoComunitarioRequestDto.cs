using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.DTOs
{
    public class CriarRecursoComunitarioRequestDto
    {
        [Required(ErrorMessage = "O tipo do recurso é obrigatório.")]
        [StringLength(100)]
        public string TipoRecurso { get; set; }

        [Required(ErrorMessage = "A descrição do recurso é obrigatória.")]
        [StringLength(500)]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "A localização descritiva é obrigatória.")]
        [StringLength(200)]
        public string LocalizacaoDescritiva { get; set; }

        [Required(ErrorMessage = "O contato do disponibilizador é obrigatório.")]
        [StringLength(100)]
        public string ContatoDisponibilizador { get; set; }
    }
}
