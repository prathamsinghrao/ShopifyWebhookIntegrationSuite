using ServiceBusQueue;
using Microsoft.Extensions.DependencyInjection;
using Models.ServiceBus;

namespace QueueReceiver.ServiceBusReceiver
{
    public interface IReceiver
    {
        void Stop();
        Task Listen(IServiceProvider services);
    }
    public class Receiver : IReceiver
    {
        private readonly IServiceBusQueueClient _queueClient;
        public Receiver(IServiceBusQueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        private bool _stop = false;
        public async Task Listen(IServiceProvider services)
        {
            while (!_stop)
            {
                try
                {
                    Console.WriteLine($"Waiting for message in queue");

                    await _queueClient.Receive<QueueMessageBody>((message) =>
                    {
                        Task.Run(() =>
                        {
                            using IServiceScope serviceScope = services.CreateScope();
                            IServiceProvider provider = serviceScope.ServiceProvider;
                            Console.WriteLine($"Received and processing message");
                            IQueueMessageProcessor queueMessageProcessor = provider.GetRequiredService<IQueueMessageProcessor>();
                            queueMessageProcessor.ProcessQueueMessage(message);
                            Console.WriteLine($"Finished processing message");
                        });
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while processing message!");
                    Console.WriteLine(e);
                }
            }
        }
        public void Stop()
        {
            _stop = true;
        }
    }
}