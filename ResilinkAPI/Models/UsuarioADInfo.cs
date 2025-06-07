namespace ResilinkAPI.Models
{
    public class UsuarioADInfo // Usado para simular o retorno de informações do usuário
    {
        public string NomeDeUsuario { get; set; }
        public string NomeCompleto { get; set; }
        public List<string> Grupos { get; set; } = new List<string>();
        public string Token { get; set; } // Para o token simulado
    }
}
