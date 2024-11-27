using Database.DbEntities;
using Database.Repository.DbEntityRepo;
using Database.Repository.EFRepo;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System.Net;
using Utils.Helper;

namespace Domains.Shopify
{
    public interface IWebhook
    {
        Task ProcessShopifyOrderTigger(Models.ServiceBus.Order.DataModel data);
        Task ProcessShopifyRefundOrder(Models.ServiceBus.RefundOrder.DataModel data);
        Task ProcessShopifyProductTrigger(Models.ServiceBus.Product.DataModel data);
    }
    public class Webhook : IWebhook
    {
        private readonly IShopifyOrderWebhookLogRepository _log;
        private readonly ICustomersRepository _customersRepository;
        private readonly IOrderDiscountCodesRepository _orderDiscountCodesRepository;
        private readonly IOrderLineItemsRepository _orderLineItemsRepository;
        private readonly IOrderLineItemsTaxLinesRepository _orderLineItemsTaxLinesRepository;
        private readonly IOrderRefundLineItemsRepository _orderRefundLineItemsRepository;
        private readonly IOrderRefundsRepository _orderRefundsRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IOrdersShippingLinesDiscountAllocationsRepository _ordersShippingLinesDiscountAllocations;
        private readonly IOrdersLineItemsDiscountAllocationsRepository _ordersLineItemsDiscountAllocations;
        private readonly IOrdersShippingLinesRepository _ordersShippingLinesRepository;
        private readonly IOrdersShippingLinesTaxLinesRepository _ordersShippingLinesTaxLinesRepository;
        private readonly IOrdersTaxLinesRepository _ordersTaxLinesRepository;
        private readonly IProductsVariantsRepository _productsVariantsRepository;
        private readonly IRefundsOrderAdjustmentsRepository _refundsOrderAdjustmentsRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductsTrackingRepository _productsTrackingRepository;
        private readonly IProductsTrackingHistoryRepository _productsTrackingHistoryRepository;
        private readonly ITimeZoneHelper _timeZoneHelper;
        private readonly string _baseUrl = "##PLACE YOUR STORE BASE URL HERE##";
        private readonly string _shopifyApiToken = "##PLACE YOUR SHOPIFY API TOKEN HERE##";
        public Webhook(IShopifyOrderWebhookLogRepository log,
            ICustomersRepository customersRepository,
            IOrderDiscountCodesRepository orderDiscountCodesRepository,
            IOrderLineItemsRepository orderLineItemsRepository,
            IOrderLineItemsTaxLinesRepository orderLineItemsTaxLinesRepository,
            IOrderRefundLineItemsRepository orderRefundLineItemsRepository,
            IOrderRefundsRepository orderRefundsRepository,
            IOrdersRepository ordersRepository,
            IOrdersShippingLinesDiscountAllocationsRepository ordersShippingLinesDiscountAllocationsRepository,
            IOrdersShippingLinesRepository ordersShippingLinesRepository,
            IOrdersShippingLinesTaxLinesRepository ordersShippingLinesTaxLinesRepository,
            IOrdersTaxLinesRepository ordersTaxLinesRepository,
            IProductsVariantsRepository productsVariantsRepository,
            IRefundsOrderAdjustmentsRepository refundsOrderAdjustmentsRepository,
            IOrdersLineItemsDiscountAllocationsRepository ordersLineItemsDiscountAllocations,
            IProductRepository productRepository,
            IProductsTrackingRepository productsTrackingRepository,
            ITimeZoneHelper timeZoneHelper,
            IProductsTrackingHistoryRepository productsTrackingHistoryRepository)
        {
            _log = log;
            _customersRepository = customersRepository;
            _orderDiscountCodesRepository = orderDiscountCodesRepository;
            _orderLineItemsRepository = orderLineItemsRepository;
            _orderLineItemsTaxLinesRepository = orderLineItemsTaxLinesRepository;
            _orderRefundLineItemsRepository = orderRefundLineItemsRepository;
            _orderRefundsRepository = orderRefundsRepository;
            _ordersRepository = ordersRepository;
            _ordersShippingLinesDiscountAllocations = ordersShippingLinesDiscountAllocationsRepository;
            _ordersShippingLinesRepository = ordersShippingLinesRepository;
            _ordersShippingLinesTaxLinesRepository = ordersShippingLinesTaxLinesRepository;
            _ordersTaxLinesRepository = ordersTaxLinesRepository;
            _productsVariantsRepository = productsVariantsRepository;
            _refundsOrderAdjustmentsRepository = refundsOrderAdjustmentsRepository;
            _ordersLineItemsDiscountAllocations = ordersLineItemsDiscountAllocations;
            _productRepository = productRepository;
            _productsTrackingRepository = productsTrackingRepository;
            _timeZoneHelper = timeZoneHelper;
            _productsTrackingHistoryRepository = productsTrackingHistoryRepository;
        }

        public async Task ProcessShopifyOrderTigger(Models.ServiceBus.Order.DataModel data)
        {
            if (data == null)
            {
                return;
            }

            string body = data.Data;
            try
            {
                string? store = data.Domain;
                OrderViewModel? orderDetails = JsonConvert.DeserializeObject<OrderViewModel>(body, new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                });

                if (orderDetails == null)
                {
                    return;
                }
                long orderId = orderDetails.Id;
                if (orderDetails.Customer == null)
                {
                    await AddDbErrorLog($"Customer object found as null for the order: {orderId}", body, null);
                }
                //Add log for the incoming request from shopify
                await AddDbErrorLog($"Shopify Trigger Order: {orderId}", body, null);

                #region create customer if not exists
                var customer = orderDetails.Customer;
                if (customer != null)
                {
                    //Check if customer exists or not
                    try
                    {
                        Customers? tbCustomer = await _customersRepository.FindById(customer.Id);
                        if (tbCustomer == null)
                        {
                            tbCustomer = new()
                            {
                                CustomerId = customer.Id,
                                CreatedAt = customer.CreatedAt,
                                Email = customer.Email ?? "",
                                Currency = customer.Currency ?? "",
                                FirstName = customer.FirstName ?? "",
                                LastName = customer.LastName ?? "",
                                UpdatedAt = customer.UpdatedAt,
                                Phone = customer.Phone,
                                DefaultAddressCity = customer.DefaultAddress != null ? customer.DefaultAddress.City ?? "" : "",
                                DefaultAddressCountry = customer.DefaultAddress != null ? customer.DefaultAddress.Country ?? "" : "",
                                DefaultAddressZip = customer.DefaultAddress != null ? customer.DefaultAddress.Zip ?? "" : "",
                            };
                            try
                            {
                                await _customersRepository.InsertAsync(tbCustomer);
                            }
                            catch (Exception ex)
                            {
                                if (!ex.Message.Contains("Cannot insert duplicate key in object 'db_Etl.Customers'"))
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await AddDbErrorLog("Error while creating customer for order webhook", body, ex);
                    }
                }
                #endregion

                #region create update order 
                //Check if order is exists or not
                Orders? order = await _ordersRepository.FindById(orderDetails.Id);
                if (order == null)
                {
                    try
                    {
                        order = new()
                        {
                            OrderId = orderDetails.Id,
                            AppId = orderDetails.AppId,
                            Currency = orderDetails.Currency ?? "",
                            CreatedAt = orderDetails.CreatedAt,
                            CustomerId = customer != null ? customer.Id : 0,
                            FinancialStatus = orderDetails.FinancialStatus ?? "",
                            OrderNumber = orderDetails.OrderNumber,
                            TotalTax = orderDetails.TotalTax,
                            FulfillmentStatus = orderDetails.FulfillmentStatus,
                            ProcessedAt = orderDetails.ProcessedAt,
                            UpdatedAt = orderDetails.UpdatedAt,
                            BillingAddressCity = orderDetails.BillingAddress != null ? orderDetails.BillingAddress.City ?? "" : "",
                            BillingAddressCountry = orderDetails.BillingAddress != null ? orderDetails.BillingAddress.Country ?? "" : "",
                            BillingAddressProvinceCode = orderDetails.BillingAddress != null ? orderDetails.BillingAddress.ProvinceCode ?? "" : "",
                            ShippingAddressCity = orderDetails.ShippingAddress != null ? orderDetails.ShippingAddress.City ?? "" : "",
                            ShippingAddressCountry = orderDetails.ShippingAddress != null ? orderDetails.ShippingAddress.Country ?? "" : "",
                            ShippingAddressProvinceCode = orderDetails.ShippingAddress != null ? orderDetails.ShippingAddress.ProvinceCode ?? "" : "",
                            CurrentTotalDutiesSetShopMoneyAmount = orderDetails.CurrentTotalDutiesSet != null && orderDetails.CurrentTotalDutiesSet.ShopMoney != null ? orderDetails.CurrentTotalDutiesSet.ShopMoney.Amount : null,
                            Quantity = orderDetails.LineItems != null && orderDetails.LineItems.Count > 0 ? orderDetails.LineItems.Sum(s => s.Quantity) : 0,
                            Store = store,
                            AdditionalFeesShopMoneyAmount = orderDetails.OriginalTotalAdditionalFeesSet != null && orderDetails.OriginalTotalAdditionalFeesSet.ShopMoney != null ? orderDetails.OriginalTotalAdditionalFeesSet.ShopMoney.Amount : null,
                            PaymentGateway = orderDetails.PaymentGatewayNames != null && orderDetails.PaymentGatewayNames.Count > 0 ? orderDetails.PaymentGatewayNames.FirstOrDefault() : null,
                            CancelledAt = orderDetails.CancelledAt
                        };
                        await _ordersRepository.InsertAsync(order);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Cannot insert duplicate key in object 'db_Etl.Orders'"))//Case where receives two duplcate messages and processed at same time with dfferent thread
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
                else
                {
                    order.AppId = orderDetails.AppId;
                    order.Currency = orderDetails.Currency ?? "";
                    order.CreatedAt = orderDetails.CreatedAt;
                    if (customer != null)
                    {
                        order.CustomerId = customer.Id;
                    }
                    order.FinancialStatus = orderDetails.FinancialStatus ?? "";
                    order.TotalTax = orderDetails.TotalTax;
                    order.FulfillmentStatus = orderDetails.FulfillmentStatus;
                    order.ProcessedAt = orderDetails.ProcessedAt;
                    order.UpdatedAt = orderDetails.UpdatedAt;
                    order.BillingAddressCity = orderDetails.BillingAddress != null ? orderDetails.BillingAddress.City ?? "" : "";
                    order.BillingAddressCountry = orderDetails.BillingAddress != null ? orderDetails.BillingAddress.Country ?? "" : "";
                    order.BillingAddressProvinceCode = orderDetails.BillingAddress != null ? orderDetails.BillingAddress.ProvinceCode ?? "" : "";
                    order.ShippingAddressCity = orderDetails.ShippingAddress != null ? orderDetails.ShippingAddress.City ?? "" : "";
                    order.ShippingAddressCountry = orderDetails.ShippingAddress != null ? orderDetails.ShippingAddress.Country ?? "" : "";
                    order.ShippingAddressProvinceCode = orderDetails.ShippingAddress != null ? orderDetails.ShippingAddress.ProvinceCode ?? "" : "";
                    order.CurrentTotalDutiesSetShopMoneyAmount = orderDetails.CurrentTotalDutiesSet != null && orderDetails.CurrentTotalDutiesSet.ShopMoney != null ? orderDetails.CurrentTotalDutiesSet.ShopMoney.Amount : null;
                    order.Quantity = orderDetails.LineItems != null && orderDetails.LineItems.Count > 0 ? orderDetails.LineItems.Sum(s => s.Quantity) : 0;
                    order.AdditionalFeesShopMoneyAmount = orderDetails.OriginalTotalAdditionalFeesSet != null && orderDetails.OriginalTotalAdditionalFeesSet.ShopMoney != null ? orderDetails.OriginalTotalAdditionalFeesSet.ShopMoney.Amount : null;
                    order.CancelledAt = orderDetails.CancelledAt;

                    await _ordersRepository.UpdateAsync(order);
                }
                #endregion

                #region OrderItems
                if (orderDetails.LineItems != null && orderDetails.LineItems.Count > 0)
                {
                    List<OrderLineItems> orderLineItems = await _orderLineItemsRepository.FindByOrderId(orderId);
                    List<OrdersLineItemsDiscountAllocations> ordersLineItemsDiscountAllocations = new();
                    List<OrdersLineItemsDiscountAllocations> insertedOrdersLineItemsDiscountAllocation = new();
                    List<OrderLineItemsTaxLines> orderLineItemsTaxLines = new();
                    if (orderLineItems != null && orderLineItems.Count > 0)//only fetch line items tax lines if order items is already there
                    {
                        List<long> lineItemIds = orderDetails.LineItems.Select(s => s.Id).ToList();
                        orderLineItemsTaxLines = await _orderLineItemsTaxLinesRepository.FetchByLineItemIds(lineItemIds);
                        ordersLineItemsDiscountAllocations = await _ordersLineItemsDiscountAllocations.FindByOrdersLineItemIds(lineItemIds);
                    }
                    List<OrderLineItems> insertedLineItems = new();
                    List<OrderLineItems> updatedLineItems = new();
                    List<OrderLineItemsTaxLines> updatedLineItemTaxLines = new();
                    List<OrderLineItemsTaxLines> insertedLineItemTaxLines = new();
                    foreach (var item in orderDetails.LineItems)
                    {
                        try
                        {
                            long orderLineItemsId = item.Id;
                            OrderLineItems? orderLineItem = orderLineItems != null && orderLineItems.Count > 0 ? orderLineItems.FirstOrDefault(f => f.Id == item.Id) : null;
                            if (orderLineItem == null)
                            {
                                orderLineItem = new()
                                {
                                    Id = orderLineItemsId,
                                    OrderId = orderId,
                                    Name = item.Name,
                                    Sku = item.Sku,
                                    Title = item.Title,
                                    Vendor = item.Vendor ?? "",
                                    ProductId = item.ProductId ?? 0,
                                    GiftCard = item.GiftCard,
                                    PreTaxPrice = item.PreTaxPrice ?? 0m,
                                    Price = item.Price,
                                    Quantity = item.Quantity,
                                    Taxable = item.Taxable,
                                    PreTaxPriceSetPresentmentMoneyCurrencyCode = item.PreTaxPriceSet != null && item.PreTaxPriceSet.PresentmentMoney != null ? item.PreTaxPriceSet.PresentmentMoney.CurrencyCode ?? "" : ""
                                };
                                insertedLineItems.Add(orderLineItem);
                            }
                            else
                            {
                                orderLineItem.Name = item.Name;
                                orderLineItem.Sku = item.Sku;
                                orderLineItem.Title = item.Title;
                                orderLineItem.Vendor = item.Vendor ?? "";
                                orderLineItem.ProductId = item.ProductId ?? 0;
                                orderLineItem.GiftCard = item.GiftCard;
                                orderLineItem.PreTaxPrice = item.PreTaxPrice ?? 0m;
                                orderLineItem.Price = item.Price;
                                orderLineItem.Quantity = item.Quantity;
                                orderLineItem.Taxable = item.Taxable;
                                orderLineItem.PreTaxPriceSetPresentmentMoneyCurrencyCode = item.PreTaxPriceSet != null && item.PreTaxPriceSet.PresentmentMoney != null ? item.PreTaxPriceSet.PresentmentMoney.CurrencyCode ?? "" : "";
                                updatedLineItems.Add(orderLineItem);
                            }

                            //Check for line items tax lines
                            if (item.TaxLines != null)
                            {
                                foreach (var taxLine in item.TaxLines)
                                {
                                    try
                                    {
                                        if (string.IsNullOrWhiteSpace(taxLine.Title)) continue;

                                        OrderLineItemsTaxLines? orderLineItemsTax = orderLineItemsTaxLines.FirstOrDefault(f => f.OrderLineItemsId == item.Id && f.Title == taxLine.Title);
                                        if (orderLineItemsTax != null)
                                        {
                                            orderLineItemsTax.Title = taxLine.Title;
                                            orderLineItemsTax.Price = taxLine.Price ?? 0m;
                                            orderLineItemsTax.Rate = taxLine.Rate ?? 0m;
                                            updatedLineItemTaxLines.Add(orderLineItemsTax);
                                            continue;
                                        }
                                        else
                                        {
                                            orderLineItemsTax = new()
                                            {
                                                Title = taxLine.Title,
                                                OrderLineItemsId = item.Id,
                                                Price = taxLine.Price ?? 0m,
                                                Rate = taxLine.Rate ?? 0m
                                            };
                                            insertedLineItemTaxLines.Add(orderLineItemsTax);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        await AddDbErrorLog("Error while iterating through order item tax lines", body, ex);
                                    }
                                }
                            }

                            if (item.DiscountAllocations != null && item.DiscountAllocations.Count > 0)
                            {
                                foreach (var discount in item.DiscountAllocations)
                                {
                                    if (string.IsNullOrWhiteSpace(discount.Amount)) continue;

                                    bool isAlreadyAdded = ordersLineItemsDiscountAllocations != null && ordersLineItemsDiscountAllocations.Count > 0 && ordersLineItemsDiscountAllocations.Any(f => f.Amount == discount.Amount);

                                    if (isAlreadyAdded)
                                    {
                                        continue;
                                    }
                                    OrdersLineItemsDiscountAllocations ordersLineItemsDiscountAllocation = new()
                                    {
                                        Amount = discount.Amount,
                                        OrderLineItemsId = orderLineItemsId,
                                    };
                                    insertedOrdersLineItemsDiscountAllocation.Add(ordersLineItemsDiscountAllocation);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await AddDbErrorLog("Error while iterating through order items", body, ex);
                        }
                    }
                    try
                    {
                        if (insertedLineItems.Count > 0 || updatedLineItems.Count > 0) //If any insert update made on line items
                        {
                            if (insertedLineItems.Count > 0)
                            {
                                try
                                {
                                    await _orderLineItemsRepository.InsertAsync(insertedLineItems);
                                }
                                catch (Exception ex)
                                {
                                    if (!ex.Message.Contains("Cannot insert duplicate key in object 'db_Etl.OrderLineItems'"))
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                            }
                            if (updatedLineItems.Count > 0)
                            {
                                await _orderLineItemsRepository.UpdateAsync(updatedLineItems);
                            }
                        }
                        if (insertedOrdersLineItemsDiscountAllocation.Count > 0)
                        {
                            await _ordersLineItemsDiscountAllocations.InsertAsync(insertedOrdersLineItemsDiscountAllocation);
                        }
                    }
                    catch (Exception ex)
                    {
                        await AddDbErrorLog("Error while inserting order items", body, ex);
                    }
                    try
                    {
                        if (insertedLineItemTaxLines.Count > 0 || updatedLineItemTaxLines.Count > 0) //If any insert update made on line item tax lines
                        {
                            if (insertedLineItemTaxLines.Count > 0)
                            {
                                await _orderLineItemsTaxLinesRepository.InsertAsync(insertedLineItemTaxLines);
                            }
                            if (updatedLineItemTaxLines.Count > 0)
                            {
                                await _orderLineItemsTaxLinesRepository.UpdateAsync(updatedLineItemTaxLines);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await AddDbErrorLog("Error while inserting order item tax lines", body, ex);
                    }
                }
                #endregion

                #region order discount codes
                if (orderDetails.DiscountCodes != null)
                {
                    try
                    {
                        List<OrderDiscountCodes> discountCodes = await _orderDiscountCodesRepository.FindByOrderId(orderId);
                        List<OrderDiscountCodes> insertedDiscountCodes = new();
                        foreach (var item in orderDetails.DiscountCodes)
                        {
                            if (string.IsNullOrWhiteSpace(item.Code))
                            {
                                continue;
                            }
                            OrderDiscountCodes? orderDiscountCodes = discountCodes != null && discountCodes.Count > 0 ? discountCodes.FirstOrDefault(f => f.Code == item.Code) : null;
                            if (orderDiscountCodes == null)
                            {
                                orderDiscountCodes = new()
                                {
                                    Code = item.Code,
                                    OrderId = orderId
                                };
                                insertedDiscountCodes.Add(orderDiscountCodes);
                            }
                        }
                        if (insertedDiscountCodes.Count > 0)
                        {
                            await _orderDiscountCodesRepository.InsertAsync(insertedDiscountCodes);
                        }
                    }
                    catch (Exception ex)
                    {
                        await AddDbErrorLog("Error while iterating through order discount codes", body, ex);
                    }
                }
                #endregion

                #region order tax lines
                if (orderDetails.TaxLines != null && orderDetails.TaxLines.Count > 0)
                {
                    try
                    {
                        List<OrdersTaxLines> ordersTaxLines = await _ordersTaxLinesRepository.FindByOrderId(orderId);
                        List<OrdersTaxLines> insertedOrdersTaxLines = new();
                        foreach (var item in orderDetails.TaxLines)
                        {
                            if (string.IsNullOrWhiteSpace(item.Title)) continue;

                            bool isAlreadyAdded = ordersTaxLines != null && ordersTaxLines.Count > 0 && ordersTaxLines.Any(f => f.Title == item.Title);

                            if (isAlreadyAdded) continue;

                            OrdersTaxLines ordersTaxLine = new OrdersTaxLines()
                            {
                                OrderId = orderId,
                                Price = item.Price ?? 0m,
                                Title = item.Title,
                                Rate = item.Rate ?? 0m
                            };
                            insertedOrdersTaxLines.Add(ordersTaxLine);
                        }
                        if (insertedOrdersTaxLines.Count > 0)
                        {
                            await _ordersTaxLinesRepository.InsertAsync(insertedOrdersTaxLines);
                        }
                    }
                    catch (Exception ex)
                    {
                        await AddDbErrorLog("Error while iterating through order tax lines", body, ex);
                    }
                }
                #endregion

                #region shipping lines
                if (orderDetails.ShippingLines != null && orderDetails.ShippingLines.Count > 0)
                {
                    try
                    {
                        List<OrdersShippingLines> ordersShippingLines = await _ordersShippingLinesRepository.FindByOrderId(orderId);
                        List<OrdersShippingLinesTaxLines> ordersShippingLinesTaxLines = new();
                        List<OrdersShippingLinesDiscountAllocations> ordersShippingLinesDiscountAllocations = new();
                        List<long> shippingLinesIds = orderDetails.ShippingLines.Select(s => s.Id).ToList();

                        List<OrdersShippingLinesTaxLines> insertedOrdersShippingLinesTaxLines = new();
                        List<OrdersShippingLinesTaxLines> updatedOrdersShippingLinesTaxLines = new();
                        List<OrdersShippingLinesDiscountAllocations> insertedOrdersShippingLinesDiscountAllocations = new();
                        if (ordersShippingLines != null && ordersShippingLines.Count > 0)
                        {
                            ordersShippingLinesTaxLines = await _ordersShippingLinesTaxLinesRepository.FindByShippingLinesId(shippingLinesIds);
                            ordersShippingLinesDiscountAllocations = await _ordersShippingLinesDiscountAllocations.FindByShippingLinesId(shippingLinesIds);
                        }

                        List<OrdersShippingLines> insertedOrdersShippingLines = new();
                        List<OrdersShippingLines> updatedOrdersShippingLines = new();
                        foreach (var item in orderDetails.ShippingLines)
                        {
                            long ordersShippingLinesId = item.Id;
                            if (insertedOrdersShippingLines.Count > 0 && insertedOrdersShippingLines.Any(a => a.Id == ordersShippingLinesId))
                            {
                                continue;
                            }
                            OrdersShippingLines? ordersShippingLine = ordersShippingLines != null && ordersShippingLines.Count > 0 ? ordersShippingLines.FirstOrDefault(f => f.Id == item.Id) : null;

                            if (ordersShippingLine == null)
                            {
                                ordersShippingLine = new()
                                {
                                    Id = ordersShippingLinesId,
                                    OrderId = orderId,
                                    Code = item.Code,
                                    Title = item.Title,
                                    DiscountedPriceSetPresentmentMoneyCurrencyCode = item.DiscountedPriceSet != null && item.DiscountedPriceSet.PresentmentMoney != null ? item.DiscountedPriceSet.PresentmentMoney.CurrencyCode : null,
                                    PriceSetShopMoneyAmount = item.PriceSet != null && item.PriceSet.ShopMoney != null ? item.PriceSet.ShopMoney.Amount : null

                                };
                                insertedOrdersShippingLines.Add(ordersShippingLine);
                            }
                            else
                            {
                                ordersShippingLine.Code = item.Code;
                                ordersShippingLine.Title = item.Title;
                                ordersShippingLine.DiscountedPriceSetPresentmentMoneyCurrencyCode = item.DiscountedPriceSet != null && item.DiscountedPriceSet.PresentmentMoney != null ? item.DiscountedPriceSet.PresentmentMoney.CurrencyCode : null;
                                ordersShippingLine.PriceSetShopMoneyAmount = item.PriceSet != null && item.PriceSet.ShopMoney != null ? item.PriceSet.ShopMoney.Amount : null;

                                updatedOrdersShippingLines.Add(ordersShippingLine);
                            }

                            if (item.TaxLines != null && item.TaxLines.Count > 0)
                            {
                                foreach (var taxLine in item.TaxLines)
                                {
                                    if (string.IsNullOrWhiteSpace(taxLine.Title)) continue;

                                    OrdersShippingLinesTaxLines? ordersShippingLinesTaxLine = ordersShippingLinesTaxLines != null && ordersShippingLinesTaxLines.Count > 0 ? ordersShippingLinesTaxLines.FirstOrDefault(f => f.OrdersShippingLinesId == item.Id && f.Title == taxLine.Title) : null;

                                    if (ordersShippingLinesTaxLine == null)
                                    {
                                        ordersShippingLinesTaxLine = new()
                                        {
                                            OrdersShippingLinesId = ordersShippingLinesId,
                                            Title = taxLine.Title,
                                            Rate = taxLine.Rate ?? 0m,
                                            PriceSetShopMoneyAmount = taxLine.PriceSet != null && taxLine.PriceSet.ShopMoney != null ? taxLine.PriceSet.ShopMoney.Amount : null
                                        };
                                        insertedOrdersShippingLinesTaxLines.Add(ordersShippingLinesTaxLine);
                                    }
                                    else
                                    {
                                        ordersShippingLinesTaxLine.Rate = taxLine.Rate ?? 0m;
                                        ordersShippingLinesTaxLine.PriceSetShopMoneyAmount = taxLine.PriceSet != null && taxLine.PriceSet.ShopMoney != null ? taxLine.PriceSet.ShopMoney.Amount : null;
                                        updatedOrdersShippingLinesTaxLines.Add(ordersShippingLinesTaxLine);
                                    }
                                }
                            }

                            if (item.DiscountAllocations != null && item.DiscountAllocations.Count > 0)
                            {
                                foreach (var discount in item.DiscountAllocations)
                                {
                                    if (string.IsNullOrWhiteSpace(discount.Amount)) continue;


                                    bool isAlreadyAdded = ordersShippingLinesDiscountAllocations != null && ordersShippingLinesDiscountAllocations.Count > 0 && ordersShippingLinesDiscountAllocations.Any(f => f.Amount == discount.Amount);

                                    if (isAlreadyAdded)
                                    {
                                        continue;
                                    }
                                    OrdersShippingLinesDiscountAllocations ordersShippingLinesDiscountAllocation = new()
                                    {
                                        Amount = discount.Amount,
                                        OrdersShippingLinesId = ordersShippingLinesId,
                                    };
                                    insertedOrdersShippingLinesDiscountAllocations.Add(ordersShippingLinesDiscountAllocation);
                                }
                            }
                        }
                        if (insertedOrdersShippingLines.Count > 0 || updatedOrdersShippingLines.Count > 0) //insert update order shipping lines
                        {
                            try
                            {
                                if (insertedOrdersShippingLines.Count > 0)
                                {
                                    await _ordersShippingLinesRepository.InsertAsync(insertedOrdersShippingLines);
                                }
                                if (updatedOrdersShippingLines.Count > 0)
                                {
                                    await _ordersShippingLinesRepository.UpdateAsync(updatedOrdersShippingLines);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!ex.Message.Contains("Cannot insert duplicate key in object 'db_Etl.OrdersShippingLines'"))
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }

                        if (insertedOrdersShippingLinesTaxLines.Count > 0 || updatedOrdersShippingLinesTaxLines.Count > 0) //insert update OrdersShippingLinesTaxLines
                        {
                            try
                            {
                                if (insertedOrdersShippingLinesTaxLines.Count > 0)
                                {
                                    await _ordersShippingLinesTaxLinesRepository.InsertAsync(insertedOrdersShippingLinesTaxLines);
                                }
                                if (updatedOrdersShippingLinesTaxLines.Count > 0)
                                {
                                    await _ordersShippingLinesTaxLinesRepository.UpdateAsync(updatedOrdersShippingLinesTaxLines);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!ex.Message.Contains("Cannot insert duplicate key in object 'db_Etl.OrdersShippingLinesTaxLines'"))
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }

                        if (insertedOrdersShippingLinesDiscountAllocations.Count > 0)
                        {
                            try
                            {
                                await _ordersShippingLinesDiscountAllocations.InsertAsync(insertedOrdersShippingLinesDiscountAllocations);
                            }
                            catch (Exception ex)
                            {
                                if (!ex.Message.Contains("Cannot insert duplicate key in object 'db_Etl.OrdersShippingLinesDiscountAllocations'"))
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await AddDbErrorLog("Error while iterating through shipping lines", body, ex);
                    }
                }
                #endregion

                #region order refunds
                if (orderDetails.Refunds != null && orderDetails.Refunds.Count > 0)
                {
                    try
                    {
                        await ProcessOrderRefunds(orderId, orderDetails.Refunds);
                    }
                    catch (Exception ex)
                    {
                        await AddDbErrorLog("Error while iterating through order refunds", body, ex);
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                await AddDbErrorLog("Error while processing order webhook", body, ex);
            }
        }
        private async Task AddDbErrorLog(string message, string body, Exception? ex)
        {
            TbShopifyOrderWebhookLog log = new()
            {
                Message = message,
                RequestBody = body,
                Exception = ex == null ? " " : JsonConvert.SerializeObject(ex),
                CreatedAt = DateTime.UtcNow
            };
            await _log.InsertAsync(log);
        }
        private async Task ProcessOrderRefunds(long orderId, List<Refund> refunds)
        {
            List<OrderRefunds> orderRefunds = await _orderRefundsRepository.FindByOrderId(orderId);
            List<OrderRefunds> insertedOrderRefunds = new();
            List<long> refundIds = refunds.Select(s => s.Id).ToList();
            List<OrderRefundLineItems> orderRefundLineItems = new();
            List<RefundsOrderAdjustments> refundsOrderAdjustments = new();

            List<OrderRefundLineItems> insertedOrderRefundLineItems = new();
            List<OrderRefundLineItems> updatedOrderRefundLineItems = new();
            List<RefundsOrderAdjustments> insertedRefundsOrderAdjustments = new();
            if (orderRefunds != null && orderRefunds.Count > 0)
            {
                orderRefundLineItems = await _orderRefundLineItemsRepository.FindByRefundIds(refundIds);
                refundsOrderAdjustments = await _refundsOrderAdjustmentsRepository.FindByRefundIds(refundIds);
            }
            foreach (var refund in refunds)
            {
                long refundId = refund.Id;
                OrderRefunds? orderRefund = orderRefunds != null && orderRefunds.Count > 0 ? orderRefunds.FirstOrDefault(f => f.Id == refundId) : null;
                if (orderRefund == null)
                {
                    orderRefund = new()
                    {
                        Id = refundId,
                        OrderId = orderId,
                        ProcessedAt = refund.ProcessedAt,
                        CreatedAt = refund.CreatedAt
                    };
                    insertedOrderRefunds.Add(orderRefund);
                }
                if (refund.RefundLineItems != null && refund.RefundLineItems.Count > 0)
                {
                    foreach (var refundLineItem in refund.RefundLineItems)
                    {
                        OrderRefundLineItems? orderRefundLineItem = orderRefundLineItems != null && orderRefundLineItems.Count > 0 ? orderRefundLineItems.FirstOrDefault(f => f.Id == refundLineItem.Id) : null;
                        if (orderRefundLineItem == null)
                        {
                            orderRefundLineItem = new()
                            {
                                Id = refundLineItem.Id,
                                OrderRefundId = refundId,
                                OrderLineItemId = refundLineItem.LineItemId,
                                TotalTax = refundLineItem.TotalTax,
                                Quantity = refundLineItem.Quantity,
                                Subtotal = refundLineItem.Subtotal,
                                LineItemPreTaxPrice = refundLineItem.LineItem != null ? refundLineItem.LineItem.PreTaxPrice ?? 0m : 0m,
                                LineItemPrice = refundLineItem.LineItem != null ? refundLineItem.LineItem.Price : 0m
                            };
                            insertedOrderRefundLineItems.Add(orderRefundLineItem);
                        }
                        else
                        {
                            orderRefundLineItem.TotalTax = refundLineItem.TotalTax;
                            orderRefundLineItem.Quantity = refundLineItem.Quantity;
                            orderRefundLineItem.Subtotal = refundLineItem.Subtotal;
                            orderRefundLineItem.LineItemPreTaxPrice = refundLineItem.LineItem != null ? refundLineItem.LineItem.PreTaxPrice ?? 0m : 0m;
                            orderRefundLineItem.LineItemPrice = refundLineItem.LineItem != null ? refundLineItem.LineItem.Price : 0m;
                            updatedOrderRefundLineItems.Add(orderRefundLineItem);
                        }
                    }
                }
                if (refund.OrderAdjustments != null && refund.OrderAdjustments.Count > 0)
                {
                    foreach (OrderAdjustments orderAdjustment in refund.OrderAdjustments)
                    {
                        bool isAlreadyAdded = refundsOrderAdjustments != null && refundsOrderAdjustments.Count > 0 && refundsOrderAdjustments.Any(f => f.Id == orderAdjustment.Id);

                        if (isAlreadyAdded) continue;

                        RefundsOrderAdjustments refundsOrderAdjustment = new()
                        {
                            Id = orderAdjustment.Id,
                            OrderId = orderId,
                            OrderRefundId = refundId,
                            Kind = orderAdjustment.Kind ?? "",
                            Amount = orderAdjustment.Amount,
                            TaxAmount = orderAdjustment.TaxAmount
                        };
                        insertedRefundsOrderAdjustments.Add(refundsOrderAdjustment);
                    }
                }
            }
            if (insertedOrderRefunds.Count > 0)
            {
                try
                {
                    await _orderRefundsRepository.InsertAsync(insertedOrderRefunds);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Cannot insert duplicate key in object")) //avoid this exception as it can be occurs dur to miliseconds delay between to concurrent request sent from shopify
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            if (insertedOrderRefundLineItems.Count > 0 || updatedOrderRefundLineItems.Count > 0)//Insert update refund line items
            {
                if (insertedOrderRefundLineItems.Count > 0)
                {
                    try
                    {
                        await _orderRefundLineItemsRepository.InsertAsync(insertedOrderRefundLineItems);
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Cannot insert duplicate key in object")) //avoid this exception as it can be occurs dur to miliseconds delay between to concurrent request sent from shopify
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
                if (updatedOrderRefundLineItems.Count > 0)
                {
                    await _orderRefundLineItemsRepository.UpdateAsync(updatedOrderRefundLineItems);
                }
            }
            if (insertedRefundsOrderAdjustments.Count > 0)//Insert refund order adjustment
            {
                try
                {
                    await _refundsOrderAdjustmentsRepository.InsertAsync(insertedRefundsOrderAdjustments);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Cannot insert duplicate key in object")) //avoid this exception as it can be occurs dur to miliseconds delay between to concurrent request sent from shopify
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
        }
        public async Task ProcessShopifyRefundOrder(Models.ServiceBus.RefundOrder.DataModel data)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                Refund? refundOrder = JsonConvert.DeserializeObject<Refund>(data.Data, new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                });

                if (refundOrder == null)
                {
                    await AddDbErrorLog($"Got refund trigger but the body seems to be empty or wrong", data.Data, null);
                    return;
                }

                //Add log for the incoming request for Refund Order
                await AddDbErrorLog($"Refund Order is process for refundId: {refundOrder.Id} with OrderId: {refundOrder.OrderId}", data.Data, null);

                List<Refund> refunds = new List<Refund> { refundOrder };

                await ProcessOrderRefunds(refundOrder.OrderId, refunds);
            }
            catch (Exception ex)
            {
                await AddDbErrorLog("Error while processing Refund order webhook", data.Data, ex);
            }
        }
        public async Task ProcessShopifyProductTrigger(Models.ServiceBus.Product.DataModel data)
        {
            if (data == null)
            {
                return;
            }
            string body = data.Data;
            try
            {
                string? store = data.Domain;
                Product? product = JsonConvert.DeserializeObject<Product>(body, new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                });

                if (product == null)
                {
                    await AddDbErrorLog($"Got Product trigger but the body seems to be empty or wrong", body, null);
                    return;
                }

                long productId = product.Id;
                await AddDbErrorLog($"Shopify Trigger Product: {productId}", body, null);

                Products existProduct = await _productRepository.FindById(product.Id);
                var productOldStatus = string.Empty; //case when Status and Variant both updated
                var ProductOldVendor = string.Empty;
                if (existProduct != null)
                {
                    productOldStatus = existProduct.status ?? "";
                    ProductOldVendor = existProduct.Vendor ?? "";
                    ProductsTrackingHistory productsTrackingHistory = new ProductsTrackingHistory();
                    if (existProduct.status != product.status)
                    {
                        ProductsTracking productsTracking = new ProductsTracking
                        {
                            ProductId = product.Id,
                            OldStatus = existProduct.status ?? "",
                            NewStatus = product.status ?? "",
                            UpdatedAt = product.UpdatedAt ?? DateTime.UtcNow
                        };
                        await _productsTrackingRepository.InsertAsync(productsTracking);

                        productsTrackingHistory.ProductId = product.Id;
                        productsTrackingHistory.OldStatus = existProduct.status ?? "";
                        productsTrackingHistory.NewStatus = product.status ?? "";
                        productsTrackingHistory.ProductUpdatedAt = product.UpdatedAt ?? DateTime.UtcNow;
                    }

                    if (product.Vendor != existProduct.Vendor)
                    {
                        productsTrackingHistory.ProductId = product.Id;
                        productsTrackingHistory.OldVendor = existProduct.Vendor ?? "";
                        productsTrackingHistory.NewVender = product.Vendor ?? "";
                        productsTrackingHistory.ProductUpdatedAt = product.UpdatedAt ?? DateTime.UtcNow;
                    }

                    if (productsTrackingHistory.ProductId > 0)
                    {
                        DateTime ukdateTime = _timeZoneHelper.ConvertUctToLocalTime(DateTime.UtcNow, "GMT Standard Time");
                        productsTrackingHistory.InsertDatetime = ukdateTime;
                        await _productsTrackingHistoryRepository.InsertAsync(productsTrackingHistory);
                    }

                    existProduct.Title = product.Title;
                    existProduct.ProductType = product.ProductType;
                    existProduct.Vendor = product.Vendor;
                    existProduct.status = product.status;
                    existProduct.UpdatedAt = product.UpdatedAt;
                    await _productRepository.UpdateAsync(existProduct);
                }
                else
                {
                    Products products = new Products
                    {
                        Id = productId,
                        Title = product.Title,
                        ProductType = product.ProductType,
                        Vendor = product.Vendor,
                        status = product.status,
                        CreatedAt = product.CreatedAt,
                        UpdatedAt = product.UpdatedAt,
                    };

                    await _productRepository.InsertAsync(products);

                }

                if (product.Variants == null || product.Variants.Count == 0)
                {
                    return;
                }

                List<long> variantIds = product.Variants.Select(s => s.Id).Distinct().ToList();
                List<long> itemInventoryId = product.Variants.Where(w => w.InventoryItemId.HasValue).Select(s => s.InventoryItemId.Value).Distinct().ToList();
                string inventoryItemids = string.Join(',', itemInventoryId);
                InventoryItemModel inventoryItems = await FetchInventoryItems(inventoryItemids, store);
                List<ProductsVariants> tbProductsVariants = await _productsVariantsRepository.FindByVariantIds(variantIds);
                List<ProductsVariants> insertedProductsVariants = new();
                List<ProductsVariants> updatedProductsVariants = new();

                List<ProductsTrackingHistory> needToInsertProductsTrackingHistory = new();
                foreach (var item in product.Variants)
                {
                    long variantId = item.Id;
                    if (insertedProductsVariants.Count > 0 && insertedProductsVariants.Any(a => a.Id == variantId))
                    {
                        continue;
                    }
                    ProductsVariants? productsVariants = tbProductsVariants != null && tbProductsVariants.Count > 0 ? tbProductsVariants.FirstOrDefault(f => f.Id == variantId) : null;
                    decimal cost = 0M;
                    if (inventoryItems != null && inventoryItems.InventoryItems.Count > 0)
                    {
                        cost = inventoryItems.InventoryItems.Where(x => x.Id == item.InventoryItemId && x.Cost.HasValue).Select(x => x.Cost.Value).FirstOrDefault();
                    }

                    if (productsVariants == null)
                    {
                        productsVariants = new()
                        {
                            Id = variantId,
                            ProductId = item.ProductId,
                            ShopifyProductsId = item.InventoryItemId,
                            AdminGraphqlApiId = item.CompareAtPrice,
                            Barcode = item.Barcode,
                            CompareAtPrice = item.CompareAtPrice,
                            CreatedAt = item.CreatedAt,
                            Grams = item.Grams,
                            Option1 = item.Option1,
                            Option2 = item.Option2,
                            Option3 = item.Option3,
                            Price = item.Price ?? 0m,
                            Sku = item.Sku,
                            SkuPosition = item.Position,
                            Taxable = item.Taxable,
                            Title = item.Title,
                            UpdatedAt = item.UpdatedAt,
                            Weight = item.Weight ?? 0m,
                            WeightUnit = item.WeightUnit,
                            Cost = cost
                        };
                        insertedProductsVariants.Add(productsVariants);
                    }
                    else
                    {
                        ProductsTrackingHistory productsTrackingHistory = new ProductsTrackingHistory();
                        if (productsVariants.Price != item.Price)
                        {
                            productsTrackingHistory.ProductId = productId;
                            productsTrackingHistory.ProductsVariantsId = productsVariants.Id;
                            productsTrackingHistory.OldStatus = productOldStatus;
                            productsTrackingHistory.NewStatus = product.status ?? "";
                            productsTrackingHistory.OldVendor = ProductOldVendor;
                            productsTrackingHistory.NewVender = product.Vendor ?? "";
                            productsTrackingHistory.OldPrice = productsVariants.Price;
                            productsTrackingHistory.NewPrice = item.Price;
                            productsTrackingHistory.OldCompareAtPrice = productsVariants.CompareAtPrice;
                            productsTrackingHistory.NewCompareAtPrice = item.CompareAtPrice;
                            productsTrackingHistory.ProductVariantsUpdatedAt = item.UpdatedAt;
                        }

                        if (productsVariants.CompareAtPrice != item.CompareAtPrice)
                        {
                            productsTrackingHistory.ProductId = productId;
                            productsTrackingHistory.ProductsVariantsId = productsVariants.Id;
                            productsTrackingHistory.OldStatus = productOldStatus;
                            productsTrackingHistory.NewStatus = product.status ?? "";
                            productsTrackingHistory.OldVendor = ProductOldVendor;
                            productsTrackingHistory.NewVender = product.Vendor ?? "";
                            productsTrackingHistory.OldPrice = productsVariants.Price;
                            productsTrackingHistory.NewPrice = item.Price;
                            productsTrackingHistory.OldCompareAtPrice = productsVariants.CompareAtPrice;
                            productsTrackingHistory.NewCompareAtPrice = item.CompareAtPrice;
                            productsTrackingHistory.ProductVariantsUpdatedAt = item.UpdatedAt;
                        }

                        productsVariants.ShopifyProductsId = item.InventoryItemId;
                        productsVariants.AdminGraphqlApiId = item.CompareAtPrice;
                        productsVariants.Barcode = item.Barcode;
                        productsVariants.CompareAtPrice = item.CompareAtPrice;
                        productsVariants.CreatedAt = item.CreatedAt;
                        productsVariants.Grams = item.Grams;
                        productsVariants.Option1 = item.Option1;
                        productsVariants.Option2 = item.Option2;
                        productsVariants.Option3 = item.Option3;
                        productsVariants.Price = item.Price ?? 0m;
                        productsVariants.Sku = item.Sku;
                        productsVariants.SkuPosition = item.Position;
                        productsVariants.Taxable = item.Taxable;
                        productsVariants.Title = item.Title;
                        productsVariants.UpdatedAt = item.UpdatedAt;
                        productsVariants.Weight = item.Weight ?? 0m;
                        productsVariants.WeightUnit = item.WeightUnit;
                        productsVariants.Cost = cost;
                        updatedProductsVariants.Add(productsVariants);

                        if (productsTrackingHistory.ProductsVariantsId > 0)
                        {
                            needToInsertProductsTrackingHistory.Add(productsTrackingHistory);
                        }
                    }
                }
                if (insertedProductsVariants.Count > 0 || updatedProductsVariants.Count > 0)//Insert update product variants
                {
                    if (insertedProductsVariants.Count > 0)
                    {
                        await _productsVariantsRepository.InsertAsync(insertedProductsVariants);
                    }
                    if (updatedProductsVariants.Count > 0)
                    {
                        await _productsVariantsRepository.UpdateAsync(updatedProductsVariants);
                    }
                }

                if (needToInsertProductsTrackingHistory.Count > 0)
                {
                    DateTime ukdateTime = _timeZoneHelper.ConvertUctToLocalTime(DateTime.UtcNow, "GMT Standard Time");
                    needToInsertProductsTrackingHistory.ForEach(x => x.InsertDatetime = ukdateTime);
                    await _productsTrackingHistoryRepository.InsertAsync(needToInsertProductsTrackingHistory);
                }
            }
            catch (Exception ex)
            {
                await AddDbErrorLog("Error while processing Product Webhook", body, ex);
            }
        }
        private async Task<InventoryItemModel> FetchInventoryItems(string inventoryItemids, string? store)
        {
            string token = _shopifyApiToken;
            string baseUrl = _baseUrl;
            RestClient client = new RestClient(baseUrl);
            try
            {
                RestRequest request = new RestRequest($"/admin/api/2024-04/inventory_items.json?ids={inventoryItemids}");
                request.AddHeader("X-Shopify-Access-Token", token);

                var response = await client.ExecuteAsync(request);

                if (response == null || response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Content))
                {
                    return null;
                }
                InventoryItemModel? result = JsonConvert.DeserializeObject<InventoryItemModel>(response.Content, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                await AddDbErrorLog("Error while trying to fetch InventoryItem", inventoryItemids, ex);
                return null;
            }
            finally
            {
                try
                {
                    client.Dispose();
                }
                catch { }
            }
        }
    }
}
