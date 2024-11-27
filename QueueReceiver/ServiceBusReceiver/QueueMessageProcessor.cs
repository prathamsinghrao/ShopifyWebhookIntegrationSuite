using Domains.Shopify;
using Models.Enum;
using Models.ServiceBus;
using Newtonsoft.Json;

namespace QueueReceiver.ServiceBusReceiver
{
    public interface IQueueMessageProcessor
    {
        void ProcessQueueMessage(QueueMessageBody message);
    }

    public class QueueMessageProcessor : IQueueMessageProcessor
    {
        private readonly IWebhook _webhook;
        public QueueMessageProcessor(
            IWebhook webhook)
        {
            _webhook = webhook;
        }

        public void ProcessQueueMessage(QueueMessageBody message)
        {
            if (message == null)
                return;

            switch (message.MessageType)
            {
                case QueueMessageTypes.OrderTrigger:
                    ProcessShopifyOrderTigger(JsonConvert.DeserializeObject<Models.ServiceBus.Order.DataModel>(message.Data)).Wait();
                    break;
                case QueueMessageTypes.RefundOrder:
                    ProcessShopifyRefundOrder(JsonConvert.DeserializeObject<Models.ServiceBus.RefundOrder.DataModel>(message.Data)).Wait();
                    break;
                case QueueMessageTypes.Product:
                    ProcessShopifyProductTrigger(JsonConvert.DeserializeObject<Models.ServiceBus.Product.DataModel>(message.Data)).Wait();
                    break;
            }
        }
        private Task ProcessShopifyOrderTigger(Models.ServiceBus.Order.DataModel data)
        {
            return _webhook.ProcessShopifyOrderTigger(data);
        }
        private Task ProcessShopifyRefundOrder(Models.ServiceBus.RefundOrder.DataModel data)
        {
            return _webhook.ProcessShopifyRefundOrder(data);
        }
        private Task ProcessShopifyProductTrigger(Models.ServiceBus.Product.DataModel data)
        {
            return _webhook.ProcessShopifyProductTrigger(data);
        }
    }
}
