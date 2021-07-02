using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Common;

namespace Producer
{
    internal class Program
    {
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

            // 6. Send a request message to a session-enabled queue & receive a response message back.
            //await SendRequestMessageWithResponse();
        }

        private static async Task SendTextMessage()
        {
            const string queueName = "sbq-text-message";

            var managementClient = new ServiceBusAdministrationClient(Config.Namespace, Config.Credential);

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(queueName);
            }

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            var sender = client.CreateSender(queueName);

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes("This is a simple test message"));

            Console.WriteLine("Press any key to send a message. Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Message Sent for {nameof(SendTextMessage)}");
            }

            Console.ReadLine();

            await managementClient.DeleteQueueAsync(queueName);
        }

        private static async Task SendTextMessageWithProperties()
        {
            const string queueName = "sbq-text-message-with-properties";

            var managementClient = new ServiceBusAdministrationClient(Config.Namespace, Config.Credential);

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(queueName);
            }

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            var sender = client.CreateSender(queueName);

            var message = new ServiceBusMessage
            {
                Body = new BinaryData("This is a simple test message"),
                ContentType = "text/plain",
                CorrelationId = Guid.NewGuid().ToString(),
                MessageId = Guid.NewGuid().ToString(),
                TimeToLive = TimeSpan.FromMinutes(10),
                ScheduledEnqueueTime = DateTime.UtcNow,
                ApplicationProperties = { { "custom-property", "Custom Value" } }
            };

            Console.WriteLine("Press any key to send a message. Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Message Sent for {nameof(SendTextMessageWithProperties)}");
            }

            Console.ReadLine();

            await managementClient.DeleteQueueAsync(queueName);
        }

        private static async Task SendComplexObjectMessage()
        {
            const string queueName = "sbq-complex-object-message";

            var managementClient = new ServiceBusAdministrationClient(Config.Namespace, Config.Credential);

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(queueName);
            }

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            var sender = client.CreateSender(queueName);

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                AccountNumber = "132456789",
                Amount = 1337m,
                PaymentDate = DateTime.Today.AddDays(1),
                Payee = "Mr John Smith"
            };

            var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payment));

            Console.WriteLine("Press any key to send a message. Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessage)}");
            }

            Console.ReadLine();

            await managementClient.DeleteQueueAsync(queueName);
        }

        private static async Task SendTextMessageToTopic()
        {
            const string topicName = "sbt-text-message";

            var managementClient = new ServiceBusAdministrationClient(Config.Namespace, Config.Credential);

            if (!await managementClient.TopicExistsAsync(topicName))
            {
                await managementClient.CreateTopicAsync(topicName);
            }

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            var sender = client.CreateSender(topicName);

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes("This is a simple test message"));

            Console.WriteLine("Press any key to send a message. Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Message Sent for {nameof(SendTextMessageToTopic)}");
            }

            Console.ReadLine();

            await managementClient.DeleteQueueAsync(topicName);
        }

        private static async Task SendComplexObjectMessageWithDuplicateDetection()
        {
            const string queueName = "sbq-complex-object-message-with-duplicate";

            var managementClient = new ServiceBusAdministrationClient(Config.Namespace, Config.Credential);

            var createQueueOptions = new CreateQueueOptions(queueName)
            {
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10)
            };

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(createQueueOptions);
            }

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            var sender = client.CreateSender(queueName);

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

            Console.WriteLine("Press any key to send all payment messages. Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                Console.WriteLine($"Total Payments to send: {payments.Count}");

                foreach (var payment in payments)
                {
                    var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payment))
                    {
                        MessageId = payment.PaymentId.ToString() // Needed to detect duplicate messages
                    };

                    var random = new Random();

                    if (random.NextDouble() > 0.4) // Randomly simulate sending duplicate messages
                    {
                        await sender.SendMessageAsync(message);
                        Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessageWithDuplicateDetection)} - Payment ID: {payment.PaymentId}");
                    }
                    else
                    {
                        await sender.SendMessageAsync(message);
                        Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessageWithDuplicateDetection)} - Payment ID: {payment.PaymentId}");

                        await sender.SendMessageAsync(message);
                        Console.WriteLine($"Message Sent for {nameof(SendComplexObjectMessageWithDuplicateDetection)} - Payment ID: {payment.PaymentId}");
                    }
                }
            }

            Console.ReadLine();

            await managementClient.DeleteQueueAsync(queueName);
        }

        private static async Task SendRequestMessageWithResponse()
        {
            const string requestQueue = "sbq-request-queue";
            const string responseQueue = "sbq-response-queue";

            var managementClient = new ServiceBusAdministrationClient(Config.Namespace, Config.Credential);

            if (!await managementClient.QueueExistsAsync(requestQueue))
            {
                await managementClient.CreateQueueAsync(requestQueue);
            }

            if (!await managementClient.QueueExistsAsync(responseQueue))
            {
                var createQueueOptions = new CreateQueueOptions(responseQueue)
                {
                    RequiresSession = true
                };

                await managementClient.CreateQueueAsync(createQueueOptions);
            }

            var responseSessionId = Guid.NewGuid().ToString();

            await using var requestClient = new ServiceBusClient(Config.Namespace, Config.Credential);

            var sender = requestClient.CreateSender(requestQueue);

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes("This is a simple test message"))
            {
                ReplyToSessionId = responseSessionId
            };

            Console.WriteLine("Press any key to send a message. Press Enter to exit.");

            await using var responseClient = new ServiceBusClient(Config.Namespace, Config.Credential);

            var serviceBusSessionProcessorOptions = new ServiceBusSessionProcessorOptions();

            serviceBusSessionProcessorOptions.SessionIds.Add(responseSessionId);

            var sessionProcessor = responseClient.CreateSessionProcessor(responseQueue, serviceBusSessionProcessorOptions);

            sessionProcessor.ProcessMessageAsync += MessageHandler;
            sessionProcessor.ProcessErrorAsync += ErrorHandler;

            static async Task MessageHandler(ProcessSessionMessageEventArgs args)
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"Received Message: { body }");

                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }

            await sessionProcessor.StartProcessingAsync();

            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Message Sent for {nameof(SendRequestMessageWithResponse)}");
            }

            Console.ReadLine();

            await sessionProcessor.StopProcessingAsync();

            await managementClient.DeleteQueueAsync(requestQueue);
            await managementClient.DeleteQueueAsync(responseQueue);
        }

        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            // the error source tells me at what point in the processing an error occurred
            Console.WriteLine(args.ErrorSource);
            // the fully qualified namespace is available
            Console.WriteLine(args.FullyQualifiedNamespace);
            // as well as the entity path
            Console.WriteLine(args.EntityPath);
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
