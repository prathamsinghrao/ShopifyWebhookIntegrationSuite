using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Models.Enum;
using Models.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace ServiceBusQueue
{
    public class ServiceBusQueueClient : IServiceBusQueueClient
    {
        private ServiceBusClient _queueClient;
        private ServiceBusSender _sender;
        private readonly string _connectionString;
        private readonly string _queueName;
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="options">Azure storage connectivity options</param>
        public ServiceBusQueueClient(IOptions<ServiceBusOptions> options) : this(options.Value.ServiceBusConnectionString, options.Value.QueueName)
        {
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="connectionString">storage account credentials</param>
        /// <param name="queueName">files accessibility in days</param>
        public ServiceBusQueueClient(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
            _queueClient = new ServiceBusClient(_connectionString);
            _sender = _queueClient.CreateSender(_queueName);
        }
        public async Task Receive<T>(Action<T> action)
        {
            //var queueClient = GetQueueClient();

            var receiver = _queueClient.CreateReceiver(_queueName);
            var message = await receiver.ReceiveMessageAsync(TimeSpan.FromMinutes(25));
            if (message == null) return;

            try
            {
                var body = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body));
                await receiver.CompleteMessageAsync(message);
                action(body);
            }
            catch { }
        }
        public Task Send<T>(T data)
        {
            string messageBody = JsonConvert.SerializeObject(data);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

            try
            {
                return _sender.SendMessageAsync(message);
            }
            catch (System.ObjectDisposedException ex)
            {
                //In case the insntace is closed or disposed
                if (ex.Message.Contains("create a new instance"))
                {
                    _queueClient = new ServiceBusClient(_connectionString);
                    _sender = _queueClient.CreateSender(_queueName);
                    return _sender.SendMessageAsync(message);
                }
                return null;
            }
        }
        private QueueMessageBody BuildMessage(object data, QueueMessageTypes messageType)
        {
            return new QueueMessageBody
            {
                Data = JsonConvert.SerializeObject(data),
                MessageType = messageType
            };
        }
        public Task OrderTrigger(Models.ServiceBus.Order.DataModel data)
        {
            return Send(BuildMessage(data, QueueMessageTypes.OrderTrigger));
        }
        public Task ProcessRefundOrder(Models.ServiceBus.RefundOrder.DataModel data)
        {
            return Send(BuildMessage(data, QueueMessageTypes.RefundOrder));
        }
        public Task ProcessProduct(Models.ServiceBus.Product.DataModel data)
        {
            return Send(BuildMessage(data, QueueMessageTypes.Product));
        }
    }
}
