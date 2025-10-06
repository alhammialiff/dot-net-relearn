using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NSwag;
using NSwag.CodeGeneration.CSharp;

public class ClientGenerator
{

    public async Task GenerateClient()
    {
        using var httpClient = new HttpClient();
        var swaggerJson = await httpClient.GetStringAsync("http://localhost:5254/swagger/v1/swagger.json");
        var document = await OpenApiDocument.FromJsonAsync(swaggerJson);

        var settings = new CSharpClientGeneratorSettings
        {
            ClassName = "CustomApiClient",
            CSharpGeneratorSettings = { Namespace = "MyClientNamespace" }
        };

        var generator = new CSharpClientGenerator(document, settings);
        var code = generator.GenerateFile();

        await File.WriteAllTextAsync("GenerateApiClient.cs", code);

    }

}