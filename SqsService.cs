using Amazon.SQS;
using Amazon.SQS.Model;

namespace sqstest7;
public class SqsService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;

    public SqsService(IAmazonSQS sqsClient, string queueUrl)
    {
        _sqsClient = sqsClient;
        _queueUrl = queueUrl;
    }

    public async Task SendMessageAsync(string messageBody)
    {
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = messageBody
        };

        await _sqsClient.SendMessageAsync(sendMessageRequest);
    }

    public async Task ReceiveMessagesAsync()
    {
        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 5
        };

        var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

        foreach (var message in response.Messages)
        {
            // Process the message
            Console.WriteLine($"Received message: {message.Body}");

            // Delete the message after processing
            await _sqsClient.DeleteMessageAsync(_queueUrl, message.MessageId);
        }
    }
}
