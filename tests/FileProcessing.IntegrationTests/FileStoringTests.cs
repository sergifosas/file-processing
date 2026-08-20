using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FileProcessing.IntegrationTests;

public class FileStoringTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FileStoringTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Process_WithValidFile_ReturnsOk()
    {
        var client = _factory.CreateClient();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("contenido de prueba"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "test.txt");

        var response = await client.PostAsync("/files/process", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Process_WithEmptyFile_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Array.Empty<byte>());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "empty.txt");

        var response = await client.PostAsync("/files/process", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Process_WithoutFile_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        using var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/files/process", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}