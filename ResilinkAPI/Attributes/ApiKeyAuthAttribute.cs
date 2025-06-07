using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ResilinkAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = configuration.GetValue<string>("Authentication:ApiKey");

            if (!apiKey.Equals(potentialApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            // Se chegou aqui, a chave é válida. Podemos adicionar um identificador.
            // Este é um exemplo simples, futuramente poderia ter uma lógica mais complexa para mapear chaves a usuários/sistemas.
            context.HttpContext.Items["ApiKeyIdentifier"] = $"APIKey_{potentialApiKey.ToString().Substring(0, Math.Min(5, potentialApiKey.ToString().Length))}"; // Exemplo: "APIKey_RESIL"


            await next();
        }
    }
}
