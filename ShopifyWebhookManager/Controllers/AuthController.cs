using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using ShopifyWebhookManager.CacheHelper;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ShopifyWebhookManager.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IMemoryCacheClient _memoryCacheClient;
        private const string _webhookSecret = "##PLACE YOUR WEBHOOK SECRET HERE##";
        public AuthController(IMemoryCacheClient memoryCacheClient)
        {
            _memoryCacheClient = memoryCacheClient;
        }
        protected bool IsDuplicate => IsDuplicateTrigger();
        protected async Task VerifyWebhook(Action<bool, string, string> action, string? store = null)
        {
            string? hmacHeader = Request.Headers["X-Shopify-Hmac-SHA256"];
            string? domain = Request.Headers["X-Shopify-Shop-Domain"];
            if (string.IsNullOrWhiteSpace(hmacHeader)) return;
            byte[] data = null;
            string? body = null;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
                data = Encoding.UTF8.GetBytes(body);
            }

            bool isVerified = false;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret)))
            {
                var computedHmac = Convert.ToBase64String(hmac.ComputeHash(data));
                isVerified = hmacHeader.Equals(computedHmac, StringComparison.OrdinalIgnoreCase);
            }
            action(isVerified, body, domain);
        }
        private bool IsDuplicateTrigger()
        {
            if (Request.Headers == null) return true;
            // Extract the X-Shopify-Webhook-Id header from the incoming request
            var webhookId = Request.Headers["X-Shopify-Webhook-Id"].ToString();
            if (string.IsNullOrWhiteSpace(webhookId)) return false;

            ConcurrentDictionary<string, DateTime> processedWebhooks = _memoryCacheClient.Get<ConcurrentDictionary<string, DateTime>>(webhookId);

            if (processedWebhooks == null || processedWebhooks.Any(a => a.Key != webhookId))
            {
                if (processedWebhooks == null)
                    processedWebhooks = new ConcurrentDictionary<string, DateTime>();

                processedWebhooks.TryAdd(webhookId, DateTime.UtcNow);
                _memoryCacheClient.SetWithAbsoluteExpiration(webhookId, processedWebhooks, 40);
                return false;
            }

            return true;
        }
    }
}
