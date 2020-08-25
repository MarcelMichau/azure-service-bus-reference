using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Producer
{
    internal class Program
    {
        private const string ConnectionString = "Endpoint=sb://sb-marcel-michau-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Tttww5okQ7576esE5o6fFWc4DusKbw03Jop0O4YZfXk=";

        private static async Task Main(string[] args)
        {
            // Each line demonstrates a particular example of sending a message to Azure Service Bus & has a corresponding line in the Consumer project
            // Uncomment the example you'd like to run, and make sure the corresponding line in the Consumer project is uncommented as well.

            // 1. The simplest example of sending a single text message to an Azure Service Bus Queue
            await SendTextMessage();

            // 2. Send a text message with some custom properties on the message
            //await SendTextMessageWithProperties();

            // 3. Receive a message for a complex object
            //await SendComplexObjectMessage();

            // 4. Send a text message to an Azure Service Bus Topic
            //await SendTextMessageToTopic();

            // 5. Send a complex object to an Azure Service Bus Queue with Duplicate Detection
            //await SendComplexObjectMessageWithDuplicateDetection();
        }

        private static async Task SendTextMessage()
        {
            const string queuePath = "sbq-text-message";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.QueueExistsAsync(queuePath))
            {
                await managementClient.CreateQueueAsync(queuePath);
            }

            var queueClient = new QueueClient(ConnectionString, queuePath);

            var message = new Message(Encoding.UTF8.GetBytes("This is a simple test message"));

            await queueClient.SendAsync(message);

            Console.WriteLine($"Message Sent for {nameof(SendTextMessage)}");

            await queueClient.CloseAsync();
            Console.ReadLine();
            await managementClient.DeleteQueueAsync(queuePath);
        }

        private static async Task SendTextMessageWithProperties()
        {
            const string queuePath = "sbq-text-message-with-properties";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.QueueExistsAsync(queuePath))
            {
                await managementClient.CreateQueueAsync(queuePath);
            }

            var queueClient = new QueueClient(ConnectionString, queuePath);

            var message = new Message
            {
                Body = Encoding.UTF8.GetBytes("This is a simple test message"),
                ContentType = "text/plain",
                CorrelationId = Guid.NewGuid().ToString(),
                Label = "Test Label",
                MessageId = Guid.NewGuid().ToString(),
                TimeToLive = TimeSpan.FromMinutes(10),
                ScheduledEnqueueTimeUtc = DateTime.UtcNow,
                UserProperties = { { "custom-property", "Custom Value" } }
            };

            await queueClient.SendAsync(message);

            Console.WriteLine($"Message Sent for {nameof(SendTextMessageWithProperties)}");

            await queueClient.CloseAsync();
            Console.ReadLine();
            await managementClient.DeleteQueueAsync(queuePath);
        }

        private static async Task SendComplexObjectMessage()
        {
            const string queuePath = "sbq-complex-object-message";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.QueueExistsAsync(queuePath))
            {
                await managementClient.CreateQueueAsync(queuePath);
            }

            var queueClient = new QueueClient(ConnectionString, queuePath);

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                AccountNumber = "132456789",
                Amount = 1337m,
                PaymentDate = DateTime.Today.AddDays(1),
                Payee = "Mr John Smith"
            };

            var message = new Message(JsonSerializer.SerializeToUtf8Bytes(payment));

            await queueClient.SendAsync(message);

            Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessage)}");

            await queueClient.CloseAsync();
            Console.ReadLine();
            await managementClient.DeleteQueueAsync(queuePath);
        }

        private static async Task SendTextMessageToTopic()
        {
            const string topicPath = "sbt-text-message";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.TopicExistsAsync(topicPath))
            {
                await managementClient.CreateTopicAsync(topicPath);
            }

            var topicClient = new TopicClient(ConnectionString, topicPath);

            var message = new Message(Encoding.UTF8.GetBytes("This is a simple test message"));

            await topicClient.SendAsync(message);

            Console.WriteLine($"Message Sent for {nameof(SendTextMessageToTopic)}");

            await topicClient.CloseAsync();
            Console.ReadLine();
            await managementClient.DeleteTopicAsync(topicPath);
        }

        private static async Task SendComplexObjectMessageWithDuplicateDetection()
        {
            const string queuePath = "sbq-complex-object-message-with-duplicate";

            var managementClient = new ManagementClient(ConnectionString);

            var queueDescription = new QueueDescription(queuePath)
            {
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10)
            };

            if (!await managementClient.QueueExistsAsync(queuePath))
            {
                await managementClient.CreateQueueAsync(queueDescription);
            }

            var queueClient = new QueueClient(ConnectionString, queuePath);

            var payments = new List<Payment>
            {
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    AccountNumber = "132456789",
                    Amount = 1337m,
                    PaymentDate = DateTime.Today.AddDays(1),
                    Payee = "Mr John Smith"
                },
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    AccountNumber = "1576321357",
                    Amount = 6984.56m,
                    PaymentDate = DateTime.Today.AddDays(3),
                    Payee = "Mrs Jane Doe"
                },
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    AccountNumber = "1867817635",
                    Amount = 13872m,
                    PaymentDate = DateTime.Today,
                    Payee = "Mr Robert Smith"
                },
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    AccountNumber = "1779584565",
                    Amount = 20000m,
                    PaymentDate = DateTime.Today.AddDays(9),
                    Payee = "Mrs James Doe"
                },
                new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    AccountNumber = "1657892587",
                    Amount = 900000m,
                    PaymentDate = DateTime.Today,
                    Payee = "Mr William Tell"
                }
            };

            Console.WriteLine($"Total Payments to send: {payments.Count}");

            foreach (var payment in payments)
            {
                var message = new Message(JsonSerializer.SerializeToUtf8Bytes(payment))
                {
                    MessageId = payment.PaymentId.ToString() // Needed to detect duplicate messages
                };

                var random = new Random();

                if (random.NextDouble() > 0.4) // Randomly simulate sending duplicate messages
                {
                    await queueClient.SendAsync(message);
                    Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessageWithDuplicateDetection)} - Payment ID: {payment.PaymentId}");
                }
                else
                {
                    await queueClient.SendAsync(message);
                    Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessageWithDuplicateDetection)} - Payment ID: {payment.PaymentId}");

                    await queueClient.SendAsync(message);
                    Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessageWithDuplicateDetection)} - Payment ID: {payment.PaymentId}");
                }
            }

            await queueClient.CloseAsync();
            Console.ReadLine();
            await managementClient.DeleteQueueAsync(queuePath);
        }
    }
}
