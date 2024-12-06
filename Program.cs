using Amazon.SQS;
using Amazon.SQS.Model;
using sqstest7;

var queueUrl=Environment.GetEnvironmentVariable("URL_SQS_AWS");
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Register AWS SQS client

builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var config = new AmazonSQSConfig
    {
        ServiceURL = "http://localhost:4566", // URL de LocalStack
        UseHttp = true                        // Usa HTTP en lugar de HTTPS
    };

    return new AmazonSQSClient("test", "test", config); // Credenciales ficticias
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint para enviar mensajes
app.MapPost("/messages", async (IAmazonSQS sqsClient, string messageBody) =>
{

    var request = new SendMessageRequest
    {
        QueueUrl = queueUrl,
        MessageBody = messageBody
    };

    var response = await sqsClient.SendMessageAsync(request);
    return Results.Ok(new { MessageId = response.MessageId });
}).WithOpenApi();

// Endpoint para leer mensajes
app.MapGet("/messages", async (IAmazonSQS sqsClient) =>
{

    var request = new ReceiveMessageRequest
    {
        QueueUrl = queueUrl,
        MaxNumberOfMessages = 10,
        WaitTimeSeconds = 5 // Long polling
    };

    var response = await sqsClient.ReceiveMessageAsync(request);
    return Results.Ok(response.Messages.Select(m => new { m.MessageId, m.Body }));
}).WithOpenApi();

// Endpoint para eliminar mensajes
app.MapDelete("/messages/{receiptHandle}", async (string receiptHandle, IAmazonSQS sqsClient) =>
{

    var request = new DeleteMessageRequest
    {
        QueueUrl = queueUrl,
        ReceiptHandle = receiptHandle
    };

    await sqsClient.DeleteMessageAsync(request);
    return Results.NoContent();
}).WithOpenApi();

app.Run();
