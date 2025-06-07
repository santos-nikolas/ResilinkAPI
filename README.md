# Resilink API - Backend

## Global Solution FIAP - C# Software Development

Este repositório contém o código fonte da API Backend para a plataforma **Resilink**. O Resilink é uma solução projetada para fortalecer a resiliência comunitária e otimizar a resposta a eventos extremos, como enchentes e apagões de energia.

Esta API é o núcleo da plataforma, responsável por gerenciar a lógica de negócios, orquestrar o fluxo de dados, garantir a segurança e fornecer os serviços necessários para as futuras interfaces de frontend (Aplicativo Móvel e Dashboard Web).

## Sobre o Projeto

O objetivo principal deste projeto é desenvolver uma API Backend segura, escalável e eficiente em C# e ASP.NET Core, implementando funcionalidades críticas para a gestão de crises.

**Principais Funcionalidades Implementadas:**
1.  **Autenticação Segura:** Acesso aos endpoints da API protegido por um sistema de API Key.
2.  **Registro de Incidentes:** Permite o cadastro de ocorrências (ex: falta de energia, alagamentos).
3.  **Geração de Alertas:** Capacidade de criar e (simuladamente) disparar alertas.
4.  **Logging de Eventos:** Registro detalhado de ações importantes e erros na plataforma.
5.  **Relatórios de Status:** Fornecimento de dados agregados sobre a situação.
6.  **Gerenciamento de Recursos Comunitários:** Permite o oferecimento e moderação de recursos disponibilizados pela comunidade.

## Tecnologias Utilizadas

*   **Linguagem:** C#
*   **Framework Backend:** ASP.NET Core (.NET 8 ou versão LTS mais recente utilizada no projeto)
*   **Banco de Dados:** SQL Server LocalDB (para desenvolvimento, acessado via Entity Framework Core)
*   **Autenticação:** API Key customizada (via Atributo ASP.NET Core)
*   **Documentação da API:** Swagger (OpenAPI) integrado
*   **Controle de Versão:** Git e GitHub

## Configuração e Execução do Projeto Localmente

Siga os passos abaixo para configurar e rodar a API Resilink em seu ambiente de desenvolvimento:

### Pré-requisitos

*   [.NET SDK](https://dotnet.microsoft.com/download) (versão compatível com o projeto, ex: .NET 8.0)
*   [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (geralmente instalado com o Visual Studio)
*   Um editor de código ou IDE (ex: Visual Studio, VS Code)
*   (Opcional) [Postman](https://www.postman.com/downloads/) para testes de API mais avançados.

### Passos para Configuração

1.  **Clone o Repositório:**
    ```bash
    git clone [URL_DO_SEU_REPOSITORIO_AQUI]
    cd ResilinkAPI 
    ```
    *(Ajuste `ResilinkAPI` para o nome real da pasta do seu projeto se for diferente)*

2.  **Configure a String de Conexão:**
    *   Abra o arquivo `appsettings.Development.json` (ou `appsettings.json` se não estiver usando ambientes específicos).
    *   Verifique ou atualize a string de conexão `DefaultConnection` para o seu SQL Server LocalDB. O padrão usado no desenvolvimento foi:
        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ResilinkAPIDB_Fast;Trusted_Connection=True;MultipleActiveResultSets=true"
        }
        ```
        O banco de dados `ResilinkAPIDB_Fast` será criado automaticamente pelas migrations se não existir.

3.  **Configure a API Key:**
    *   No mesmo arquivo `appsettings.Development.json` (ou `appsettings.json`), defina sua API Key:
        ```json
        "Authentication": {
          "ApiKey": "SUA_CHAVE_DE_API_SECRETA_AQUI" 
        }
        ```
        Substitua `"SUA_CHAVE_DE_API_SECRETA_AQUI"` por uma chave de sua escolha (ex: `QUICK_RESILINK_KEY_123` que usamos nos exemplos).

4.  **Aplique as Migrations do Entity Framework Core:**
    *   Abra um terminal ou o Package Manager Console na pasta raiz do projeto da API.
    *   Execute os seguintes comandos:
        ```bash
        dotnet restore 
        dotnet tool install --global dotnet-ef # Se ainda não tiver o EF Core tools instalado
        dotnet ef database update
        ```
        Isso criará o banco de dados e as tabelas necessárias.

5.  **Execute a Aplicação:**
    *   Você pode rodar o projeto diretamente pelo Visual Studio (pressionando F5 ou o botão Play).
    *   Ou via linha de comando, na pasta do projeto da API:
        ```bash
        dotnet run
        ```

### Acessando e Testando a API

*   Após iniciar a aplicação, a interface do **Swagger UI** estará disponível em:
    *   **`http://localhost:PORTA/`**
    *   *Substitua `PORTA` pela porta em que sua aplicação está rodando (ex: `5248` para HTTP ou uma porta na faixa de 7xxx para HTTPS se configurado). Verifique o output do console ao iniciar a aplicação para confirmar a porta correta.*
    *   No Swagger UI, clique no botão **"Authorize"** no canto superior direito.
    *   Na janela que abrir, no campo "ApiKeyAuth (apiKey)", digite o valor da `ApiKey` que você configurou no `appsettings.json` (ex: `QUICK_RESILINK_KEY_123`).
    *   Clique em "Authorize" e depois em "Close".
    *   Agora você pode expandir os controladores e usar o botão "Try it out" para testar os endpoints. A API Key será enviada automaticamente no header `X-API-KEY`.

## Estrutura de Pastas do Projeto (Backend)

*   **/Attributes**: Contém atributos customizados, como o `ApiKeyAuthAttribute.cs`.
*   **/Controllers**: Contém os controladores da API (endpoints RESTful).
*   **/Data**: Contém o `ApplicationDbContext.cs` para interação com o banco de dados.
*   **/DTOs**: Contém os Data Transfer Objects, usados para definir os "contratos" de entrada e saída da API.
*   **/Migrations**: Contém os arquivos de migração do Entity Framework Core.
*   **/Models**: Contém as classes de entidade que representam os dados da aplicação.
*   **/Services**: Contém as classes de serviço com a lógica de negócio da aplicação.
*   **appsettings.json**: Arquivo de configuração principal.
*   **Program.cs**: Ponto de entrada e configuração da aplicação ASP.NET Core.

## Contribuição
Este é um projeto acadêmico para a Global Solution da FIAP.

---
