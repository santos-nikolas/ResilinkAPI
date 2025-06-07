namespace ResilinkAPI.DTOs
{
    public class LoginResponseDto // Ou poderia ser ApiKeyVerificationResponseDto
    {
        public string Message { get; set; } // Ex: "API Key válida. Acesso concedido."
        public string AuthenticatedUser { get; set; } // Identificador da API Key ou nome do usuário

        // Se fosse JWT ou AD real, você teria:
        // public string Token { get; set; }
        // public string NomeCompleto { get; set; }
        // public List<string> Grupos { get; set; }
    }
}
