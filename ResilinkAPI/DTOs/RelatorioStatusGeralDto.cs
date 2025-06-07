namespace ResilinkAPI.DTOs
{
    public class RelatorioStatusGeralDto
    {
        public int TotalIncidentesAbertos { get; set; }
        public int TotalAlertasAtivos { get; set; }
        public int UsuariosAtivosUltimas24h { get; set; }
        public Dictionary<string, int> IncidentesPorTipo { get; set; } = new Dictionary<string, int>();
    }
}
