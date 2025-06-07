using Microsoft.EntityFrameworkCore;
using ResilinkAPI.Models;

namespace ResilinkAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet para cada uma das suas entidades (tabelas no banco de dados)
        public DbSet<Incidente> Incidentes { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<LogEvento> LogsEventos { get; set; }
        public DbSet<RecursoComunitario> RecursosComunitarios { get; set; }
        // Não precisamos de um DbSet para UsuarioADInfo, pois ele não será uma tabela no banco,
        // mas sim um DTO ou modelo para representar dados vindos de uma autenticação (mesmo que simulada).

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações adicionais do modelo podem ser feitas aqui, se necessário.
            // Por exemplo, definir chaves compostas, índices, relações mais complexas, etc.
            // Para os modelos simples que criamos, o EF Core geralmente consegue inferir
            // as chaves primárias (propriedades chamadas 'Id' ou '[NomeDaClasse]Id') e
            // os tipos de dados das colunas.

            // Exemplo de configuração (se necessário no futuro):
            // modelBuilder.Entity<Incidente>()
            //     .Property(i => i.Descricao)
            //     .HasMaxLength(1000); // Já temos StringLength no modelo, mas pode ser feito aqui também

            // modelBuilder.Entity<Alerta>()
            //     .HasIndex(a => a.DataHoraEmissao); // Criar um índice para consultas mais rápidas por data
        }
    }
}
