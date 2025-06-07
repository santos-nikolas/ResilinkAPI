using ResilinkAPI.DTOs;

namespace ResilinkAPI.Services
{
    public interface IRelatorioService
    {
        Task<RelatorioStatusGeralDto> GerarRelatorioStatusGeralAsync();
    }
}
