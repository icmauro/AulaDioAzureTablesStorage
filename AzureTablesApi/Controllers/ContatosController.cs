using Azure.Data.Tables;
using AzureTablesApi.DTO;
using AzureTablesApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTablesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContatosController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        private TableServiceClient _tableServiceClient;
        private TableClient _tableClient;

        public ContatosController(IConfiguration _configuration)
        {
            _connectionString = _configuration.GetValue<string>("SAConnectionStrings");
            _tableName = _configuration.GetValue<string>("Contatos");

            _tableServiceClient = new TableServiceClient(_connectionString);
            _tableClient = _tableServiceClient.GetTableClient(_tableName);
            _tableClient.CreateIfNotExists();
        }

        [HttpPost]
        public IActionResult Criar(ContatoDto contatoDto)
        {
            var contatoModel = TrocaObjModel(contatoDto);

            _tableClient.UpsertEntity(contatoModel);

            return Ok(contatoModel);
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(string id, ContatoDto contatoDto)
        {
            var contato = _tableClient.GetEntity<Contato>(id, id).Value;

            contato.Nome = contatoDto.Nome;
            contato.Telefone = contatoDto.Telefone;
            contato.Email = contatoDto.Email;

            _tableClient.UpsertEntity(contato);

            return Ok(contato);
        }

        [HttpGet()]
        public IActionResult ObterTodos()
        {
            var contatos = _tableClient.Query<Contato>().ToList();

            return Ok(contatos);
        }

        [HttpGet("ObterPorNome/{nome}")]
        public IActionResult ObterPorNome(string nome)
        {
            var contatos = _tableClient.Query<Contato>(x => x.Nome == nome).ToList();

            return Ok(contatos);
        }

        [HttpDelete("{id}")]
        public IActionResult Deletar(string id)
        {
            var contato = _tableClient.DeleteEntity(id, id);

            return NoContent();
        }


        private Contato TrocaObjModel(ContatoDto contatoDto)
        {
            return new Contato
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Nome = contatoDto.Nome,
                Telefone = contatoDto.Telefone,
                Email = contatoDto.Email
            };
        }

    }
}
