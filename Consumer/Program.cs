using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Consumer
{
    internal class Program
    {
        private const string ConnectionString = "Endpoint=sb://sb-marcel-michau-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Tttww5okQ7576esE5o6fFWc4DusKbw03Jop0O4YZfXk=";

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
            //await RecieveComplexObjectMessageWithDuplicateDetection();
        }

        private static async Task ReceiveTextMessage()
        {
            const string queuePath = "sbq-text-message";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.QueueExistsAsync(queuePath))
            {
                await managementClient.CreateQueueAsync(queuePath);
            }

            static async Task ProcessMessagesAsync(Message message, CancellationToken token)
            {
                var text = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"Received Message: { text }");
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveTextMessage)}...");

            var queueClient = new QueueClient(ConnectionString, queuePath);

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionReceivedHandler);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await queueClient.CloseAsync();
        }

        private static async Task ReceiveTextMessageWithProperties()
        {
            const string queuePath = "sbq-text-message-with-properties";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.QueueExistsAsync(queuePath))
            {
                await managementClient.CreateQueueAsync(queuePath);
            }

            static async Task ProcessMessagesAsync(Message message, CancellationToken token)
            {
                Console.WriteLine($"Message Body: { Encoding.UTF8.GetString(message.Body) }");
                Console.WriteLine($"Content Type: { message.ContentType }");
                Console.WriteLine($"Correlation ID: { message.CorrelationId }");
                Console.WriteLine($"Label: { message.Label }");
                Console.WriteLine($"Message Id: { message.MessageId }");
                Console.WriteLine($"Time to Live: { message.TimeToLive }");
                Console.WriteLine($"Scheduled Enqueue Time Utc: { message.ScheduledEnqueueTimeUtc }");

                foreach (var (key, value) in message.UserProperties)
                {
                    Console.WriteLine($"UserProperty ({key}): { value }");
                }
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveTextMessageWithProperties)}...");

            var queueClient = new QueueClient(ConnectionString, queuePath);

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionReceivedHandler);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await queueClient.CloseAsync();
        }

        private static async Task ReceiveComplexObjectMessage()
        {
            const string queuePath = "sbq-complex-object-message";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.QueueExistsAsync(queuePath))
            {
                await managementClient.CreateQueueAsync(queuePath);
            }

            static async Task ProcessMessagesAsync(Message message, CancellationToken token)
            {
                var rawMessage = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"Received Message: { rawMessage }");

                var payment = JsonSerializer.Deserialize<Payment>(rawMessage);
                Console.WriteLine($"Payment ID: { payment.PaymentId }");
                Console.WriteLine($"Account Number: { payment.AccountNumber }");
                Console.WriteLine($"Amount: { payment.Amount }");
                Console.WriteLine($"Date: { payment.PaymentDate }");
                Console.WriteLine($"Payee: { payment.Payee }");
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveComplexObjectMessage)}...");

            var queueClient = new QueueClient(ConnectionString, queuePath);

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionReceivedHandler);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await queueClient.CloseAsync();
        }

        private static async Task ReceiveTextMessageOnTopicSubscription()
        {
            const string topicPath = "sbt-text-message";
            const string subscriptionName = "sbs-text-message-consumer-subscription";

            var managementClient = new ManagementClient(ConnectionString);

            if (!await managementClient.TopicExistsAsync(topicPath))
            {
                await managementClient.CreateTopicAsync(topicPath);
            }

            if (!await managementClient.SubscriptionExistsAsync(topicPath, subscriptionName))
            {
                await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topicPath, subscriptionName));
            }

            static async Task ProcessMessagesAsync(Message message, CancellationToken token)
            {
                var text = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"Received Message: { text }");
            }

            Console.WriteLine($"Receiving messages for {nameof(ReceiveTextMessageOnTopicSubscription)}...");

            var subscriptionClient = new SubscriptionClient(ConnectionString, topicPath, subscriptionName);

            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionReceivedHandler);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await subscriptionClient.CloseAsync();
        }

        private static async Task RecieveComplexObjectMessageWithDuplicateDetection()
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

            static async Task ProcessMessagesAsync(Message message, CancellationToken token)
            {
                var rawMessage = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"Received Message: { rawMessage }");

                var payment = JsonSerializer.Deserialize<Payment>(rawMessage);
                Console.WriteLine($"Received Payment with ID: { payment.PaymentId } for Payee: {payment.Payee}");
            }

            Console.WriteLine($"Receiving messages for {nameof(RecieveComplexObjectMessageWithDuplicateDetection)}...");

            var queueClient = new QueueClient(ConnectionString, queuePath);

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionReceivedHandler);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await queueClient.CloseAsync();
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"An error occurred: {exceptionReceivedEventArgs.Exception}");

            return Task.CompletedTask;
        }
    }
}
