using System.ComponentModel;

namespace Models.Enum
{
    [Serializable]
    public enum QueueMessageTypes
    {
        [Description("Order Trigger")]
        OrderTrigger,

        [Description("RefundOrder")]
        RefundOrder,

        [Description("Product Trigger")]
        Product
    }
}
