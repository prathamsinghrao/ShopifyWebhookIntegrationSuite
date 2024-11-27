using Models.Enum;

namespace Models.ServiceBus
{
    [Serializable]
    public class QueueMessageBody
    {
        public QueueMessageTypes MessageType { get; set; }
        public string Data { get; set; }
    }
}
