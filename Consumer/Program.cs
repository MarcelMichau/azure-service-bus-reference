using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Management;
using Common;

namespace Consumer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Each line demonstrates a particular example of receiving a message from Azure Service Bus & has a corresponding line in the Producer project
            // Uncomment the example you'd like to run, and make sure the corresponding line in the Producer project is uncommented as well.

            // 1. The simplest example of receiving a single text message from an Azure Service Bus Queue
            // To demonstrate the competing consumers pattern, start two or more instances of the Consumer before starting the Producer. Only one Consumer should pick up the message.
            await ReceiveTextMessage();

            // 2. Receive a text message with some custom properties on the message
            //await ReceiveTextMessageWithProperties();

            // 3. Receive a message for a complex object
            //await ReceiveComplexObjectMessage();

            // 4. Receive a text message from a Topic Subscription
            //await ReceiveTextMessageOnTopicSubscription();

            // 5. Send a complex object to an Azure Service Bus Queue with Duplicate Detection
            //await ReceiveComplexObjectMessageWithDuplicateDetection();
        }

        private static async Task ReceiveTextMessage()
        {
            const string queueName = "sbq-text-message";

            var managementClient = new ServiceBusManagementClient(Config.Namespace, Config.Credential);

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(queueName);
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveTextMessage)}...");

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            // get the options to use for configuring the processor
            var options = new ServiceBusProcessorOptions
            {
                // By default after the message handler returns, the processor will complete the message
                // If we want more fine-grained control over settlement, we can set this to false.
                AutoComplete = false,

                // I can also allow for multi-threading
                MaxConcurrentCalls = 2
            };

            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(queueName, options);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            static async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"Received Message: { body }");

                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }

            await processor.StartProcessingAsync();

            Console.WriteLine("Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

            await processor.StopProcessingAsync();
        }

        private static async Task ReceiveTextMessageWithProperties()
        {
            const string queueName = "sbq-text-message-with-properties";

            var managementClient = new ServiceBusManagementClient(Config.Namespace, Config.Credential);

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(queueName);
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveTextMessageWithProperties)}...");

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            // get the options to use for configuring the processor
            var options = new ServiceBusProcessorOptions
            {
                // By default after the message handler returns, the processor will complete the message
                // If we want more fine-grained control over settlement, we can set this to false.
                AutoComplete = false,

                // I can also allow for multi-threading
                MaxConcurrentCalls = 2
            };

            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(queueName, options);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            static async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"Message Body: { body }");
                Console.WriteLine($"Content Type: { args.Message.ContentType }");
                Console.WriteLine($"Correlation ID: { args.Message.CorrelationId }");
                Console.WriteLine($"Label: { args.Message.Label }");
                Console.WriteLine($"Message Id: { args.Message.MessageId }");
                Console.WriteLine($"Time to Live: { args.Message.TimeToLive }");
                Console.WriteLine($"Scheduled Enqueue Time: { args.Message.ScheduledEnqueueTime }");

                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }

            await processor.StartProcessingAsync();

            Console.WriteLine("Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

            await processor.StopProcessingAsync();
        }

        private static async Task ReceiveComplexObjectMessage()
        {
            const string queueName = "sbq-complex-object-message";

            var managementClient = new ServiceBusManagementClient(Config.Namespace, Config.Credential);

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(queueName);
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveComplexObjectMessage)}...");

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            // get the options to use for configuring the processor
            var options = new ServiceBusProcessorOptions
            {
                // By default after the message handler returns, the processor will complete the message
                // If we want more fine-grained control over settlement, we can set this to false.
                AutoComplete = false,

                // I can also allow for multi-threading
                MaxConcurrentCalls = 2
            };

            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(queueName, options);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            static async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var rawMessage = args.Message.Body.ToString();
                Console.WriteLine($"Received Message: { rawMessage }");

                var payment = JsonSerializer.Deserialize<Payment>(rawMessage);
                Console.WriteLine($"Payment ID: { payment.PaymentId }");
                Console.WriteLine($"Account Number: { payment.AccountNumber }");
                Console.WriteLine($"Amount: { payment.Amount }");
                Console.WriteLine($"Date: { payment.PaymentDate }");
                Console.WriteLine($"Payee: { payment.Payee }");

                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }

            await processor.StartProcessingAsync();

            Console.WriteLine("Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

            await processor.StopProcessingAsync();
        }

        private static async Task ReceiveTextMessageOnTopicSubscription()
        {
            const string topicName = "sbt-text-message";
            const string subscriptionName = "sbs-text-message-consumer-subscription";

            var managementClient = new ServiceBusManagementClient(Config.Namespace, Config.Credential);

            if (!await managementClient.TopicExistsAsync(topicName))
            {
                await managementClient.CreateTopicAsync(topicName);
            }

            if (!await managementClient.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                await managementClient.CreateSubscriptionAsync(topicName, subscriptionName);
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveTextMessageOnTopicSubscription)}...");

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            // get the options to use for configuring the processor
            var options = new ServiceBusProcessorOptions
            {
                // By default after the message handler returns, the processor will complete the message
                // If we want more fine-grained control over settlement, we can set this to false.
                AutoComplete = false,

                // I can also allow for multi-threading
                MaxConcurrentCalls = 2
            };

            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(topicName, subscriptionName, options);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            static async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"Received Message: { body }");

                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }

            await processor.StartProcessingAsync();

            Console.WriteLine("Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

            await processor.StopProcessingAsync();
        }

        private static async Task ReceiveComplexObjectMessageWithDuplicateDetection()
        {
            const string queueName = "sbq-complex-object-message-with-duplicate";

            var managementClient = new ServiceBusManagementClient(Config.Namespace, Config.Credential);

            var createQueueOptions = new CreateQueueOptions(queueName)
            {
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10)
            };

            if (!await managementClient.QueueExistsAsync(queueName))
            {
                await managementClient.CreateQueueAsync(createQueueOptions);
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveComplexObjectMessageWithDuplicateDetection)}...");

            await using var client = new ServiceBusClient(Config.Namespace, Config.Credential);

            // get the options to use for configuring the processor
            var options = new ServiceBusProcessorOptions
            {
                // By default after the message handler returns, the processor will complete the message
                // If we want more fine-grained control over settlement, we can set this to false.
                AutoComplete = false,

                // I can also allow for multi-threading
                MaxConcurrentCalls = 2
            };

            // create a processor that we can use to process the messages
            var processor = client.CreateProcessor(queueName, options);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            static async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var rawMessage = args.Message.Body.ToString();

                var payment = JsonSerializer.Deserialize<Payment>(rawMessage);
                Console.WriteLine($"Received Payment with ID: { payment.PaymentId } for Payee: {payment.Payee}");

                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }

            await processor.StartProcessingAsync();

            Console.WriteLine("Press Enter to exit.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

            await processor.StopProcessingAsync();
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
