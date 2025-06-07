using System.ComponentModel.DataAnnotations;

namespace ResilinkAPI.DTOs
{
    public class LoginRequestDto
    {
        // Usado se fôssemos implementar um endpoint de login tradicional,
        // mas com ApiKeyAuth, este DTO pode não ser usado diretamente para autenticação inicial.
        // Mantendo para consistência ou futuro uso.
        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Password { get; set; }
    }
}
