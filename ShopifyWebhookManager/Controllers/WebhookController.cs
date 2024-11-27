using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBusQueue;
using Database.Repository.DbEntityRepo;
using ShopifyWebhookManager.CacheHelper;
using Database.DbEntities;
using Newtonsoft.Json;

namespace ShopifyWebhookManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : AuthController
    {
        private readonly IServiceBusQueueClient _queueClient;
        private readonly IShopifyOrderWebhookLogRepository _log;
        private readonly IMemoryCacheClient _memoryCacheClient;
        public WebhookController(IServiceBusQueueClient queueClient,
            IShopifyOrderWebhookLogRepository log,
            IMemoryCacheClient memoryCacheClient
            ) : base(memoryCacheClient)
        {
            _queueClient = queueClient;
            _log = log;
            _memoryCacheClient = memoryCacheClient;
        }

        [AllowAnonymous]
        [HttpPost("OrderAddEdit")]
        public async Task<IActionResult> OrderTrigger()
        {
            try
            {
                if (!IsDuplicate)
                {
                    await VerifyWebhook((isVerified, body, domain) =>
                    {
                        if (isVerified && !string.IsNullOrWhiteSpace(body))
                        {
                            Models.ServiceBus.Order.DataModel dataModel = new Models.ServiceBus.Order.DataModel
                            {
                                Domain = domain,
                                Data = body
                            };
                            _queueClient.OrderTrigger(dataModel);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog("Error while order trigger", ex).Wait();
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("ProcessRefundOrder")]
        public async Task<IActionResult> ProcessRefundOrder()
        {
            try
            {
                if (!IsDuplicate)
                {
                    await VerifyWebhook((isVerified, body, domain) =>
                    {
                        if (isVerified && !string.IsNullOrWhiteSpace(body))
                        {
                            Models.ServiceBus.RefundOrder.DataModel dataModel = new Models.ServiceBus.RefundOrder.DataModel
                            {
                                Data = body
                            };
                            _queueClient.ProcessRefundOrder(dataModel);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog("Error while Process Refund Order", ex).Wait();
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("ProductAddEdit")]
        public async Task<IActionResult> ProductTrigger()
        {
            try
            {
                if (!IsDuplicate)
                {
                    await VerifyWebhook((isVerified, body, domain) =>
                    {
                        if (isVerified && !string.IsNullOrWhiteSpace(body))
                        {
                            Models.ServiceBus.Product.DataModel dataModel = new Models.ServiceBus.Product.DataModel
                            {
                                Domain = domain,
                                Data = body
                            };
                            _queueClient.ProcessProduct(dataModel);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AddDbErrorLog("Error while Product Trigger", ex).Wait();
            }
            return Ok();
        }

        private async Task AddDbErrorLog(string message, Exception? ex = null, string? body = null)
        {
            TbShopifyOrderWebhookLog log = new()
            {
                Message = message,
                RequestBody = body ?? string.Empty,
                Exception = ex != null ? JsonConvert.SerializeObject(ex) : string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            await _log.InsertAsync(log);
        }
    }
}
