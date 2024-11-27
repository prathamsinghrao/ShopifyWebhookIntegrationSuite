namespace ServiceBusQueue
{
    public class ServiceBusOptions
    {
        /// <summary>
        /// Azure Service Bus connection string.
        /// </summary>
        public string ServiceBusConnectionString { get; set; }

        /// <summary>
        /// Name of the Queue
        /// </summary>
        public string QueueName { get; set; }
    }
}
