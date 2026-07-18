using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;

namespace CareConnect.API.Services;

public class S3Service
{
    private readonly IConfiguration _config;

    public S3Service(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> UploadFotoAsync(IFormFile ficheiro, string pastaDestino)
    {
        if (ficheiro == null || ficheiro.Length == 0)
            throw new ArgumentException("Ficheiro inválido ou vazio.");

        // 1. Lê os segredos que guardámos no cofre (User Secrets ou Variáveis de Ambiente)
        var accessKey = _config["AWS:AccessKey"];
        var secretKey = _config["AWS:SecretKey"];
        var bucketName = _config["AWS:BucketName"];
        var regionStr = _config["AWS:Region"] ?? "eu-west-1";

        var region = Amazon.RegionEndpoint.GetBySystemName(regionStr);
        var credenciais = new BasicAWSCredentials(accessKey, secretKey);

        using var s3Client = new AmazonS3Client(credenciais, region);

        // 2. Gera um nome único para a foto (para evitar que uma foto com o mesmo nome substitua outra)
        var extensao = Path.GetExtension(ficheiro.FileName);
        var nomeUnico = $"{pastaDestino}/{Guid.NewGuid()}{extensao}";

        // 3. Faz o Upload usando o TransferUtility (a forma mais eficiente no .NET)
        using var novaMemoria = new MemoryStream();
        await ficheiro.CopyToAsync(novaMemoria);
        novaMemoria.Position = 0;

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = novaMemoria,
            Key = nomeUnico,
            BucketName = bucketName,
            ContentType = ficheiro.ContentType
            // Nota: Se o teu bucket S3 for público, podes descomentar a linha abaixo:
            // CannedACL = S3CannedACL.PublicRead
        };

        var transferUtility = new TransferUtility(s3Client);
        await transferUtility.UploadAsync(uploadRequest);

        // 4. Devolve o link (URL) final da imagem para guardarmos no banco de dados!
        return $"https://{bucketName}.s3.{regionStr}.amazonaws.com/{nomeUnico}";
    }
}