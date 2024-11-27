namespace ShopifyWebhookManager.CacheHelper
{
    public interface IMemoryCacheClient
    {
        T Get<T>(string key);
        void SetWithAbsoluteExpiration<T>(string key, T @object, int seconds);
    }
}
