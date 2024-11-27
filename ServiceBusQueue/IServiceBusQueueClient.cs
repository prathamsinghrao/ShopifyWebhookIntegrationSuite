namespace ServiceBusQueue
{
    public interface IServiceBusQueueClient
    {
        Task Send<T>(T data);
        Task Receive<T>(Action<T> action);
        Task OrderTrigger(Models.ServiceBus.Order.DataModel data);
        Task ProcessRefundOrder(Models.ServiceBus.RefundOrder.DataModel data);
        Task ProcessProduct(Models.ServiceBus.Product.DataModel data);
    }
}
