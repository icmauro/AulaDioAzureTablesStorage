using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorage.API.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobStorage.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArquivoController : ControllerBase
    {
        //private readonly string caminhoPasta;
        private readonly string _conexao;
        private readonly string containerName;
        private BlobContainerClient _blobContainerClient { get; set; }
        private BlobClient _blobClient { get; set; }

        public ArquivoController(IConfiguration _configuration)
        {
            //caminhoPasta = _configuration.GetValue<string>("caminhoPasta");
            _conexao = _configuration.GetValue<string>("stringConexao");
            containerName = _configuration.GetValue<string>("containerName");

            _blobContainerClient = new BlobContainerClient(_conexao, containerName);
        }

        [HttpPost("Upload")]
        public IActionResult UploadArquivo(IFormFile formFile)
        {
            _blobClient = _blobContainerClient.GetBlobClient(formFile.FileName);

            using var data = formFile.OpenReadStream();

            _blobClient.Upload(data, new BlobUploadOptions
            {

                HttpHeaders = new BlobHttpHeaders { ContentType = formFile.ContentType }
            });

            return Ok(_blobClient.Uri.ToString());

        }

        [HttpGet("Download/{nome}")]
        public IActionResult DownloadArquivo(string nome)
        {
            _blobClient = _blobContainerClient.GetBlobClient(nome);

            if (!_blobClient.Exists())
            {
                return BadRequest();
            }

            var retorno = _blobClient.DownloadContent();

            return File(retorno.Value.Content.ToArray(),retorno.Value.Details.ContentType, _blobClient.Name);
        }

        [HttpDelete("Deletar/{nome}")]
        public IActionResult DeletarArquivo(string nome)
        {
            _blobClient = _blobContainerClient.GetBlobClient(nome);

            _blobClient.DeleteIfExists();

            return NoContent();
        }


        [HttpGet("Listar")]
        public IActionResult ListarArquivo(string nome)
        {
            _blobClient = _blobContainerClient.GetBlobClient(nome);

            List<BlobDto> listaBlob = new List<BlobDto>();

            foreach (var blob in _blobContainerClient.GetBlobs())
            {
                listaBlob.Add(new BlobDto
                {
                    Nome = blob.Name,
                    Tipo = blob.Properties.ContentType,
                    Uri = _blobContainerClient.Uri.AbsoluteUri + "/" + blob.Name
                });
            }

            return Ok(listaBlob);
        }
    }
}
