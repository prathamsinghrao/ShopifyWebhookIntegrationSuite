// See https://aka.ms/new-console-template for more information
using Database.EFDbContext;
using Database.Repository.DbEntityRepo;
using Database.Repository.EFRepo;
using Domains.Shopify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;
using QueueReceiver.ServiceBusReceiver;
using ServiceBusQueue;
using System.Diagnostics;
using Topshelf;
using Utils.Helper;

#region Configuration
IConfiguration config = new ConfigurationBuilder()
.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
.Build();
#endregion

#region Services
var serviceProvider = new ServiceCollection()
.Configure<ServiceBusOptions>(option =>
{
    option.QueueName = config["ServiceBusQueueName"] ?? "";
    //option.QueueName = "theonetestingqueue";
    option.ServiceBusConnectionString = config["ServiceBusConnectionString"] ?? "";
})
.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(config.GetConnectionString("DBConnectionString"), sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });
}, ServiceLifetime.Scoped)
.AddScoped<IReceiver, Receiver>() // Registering services
.AddScoped<IServiceBusQueueClient, ServiceBusQueueClient>()
.AddScoped<IWebhook, Webhook>()
.AddScoped<IShopifyOrderWebhookLogRepository, ShopifyOrderWebhookLogRepository>()
.AddScoped<IOrdersRepository, OrdersRepository>()
.AddScoped<ICustomersRepository, CustomersRepository>()
.AddScoped<IOrderDiscountCodesRepository, OrderDiscountCodesRepository>()
.AddScoped<IOrderLineItemsRepository, OrderLineItemsRepository>()
.AddScoped<IOrderLineItemsTaxLinesRepository, OrderLineItemsTaxLinesRepository>()
.AddScoped<IOrderRefundLineItemsRepository, OrderRefundLineItemsRepository>()
.AddScoped<IOrderRefundsRepository, OrderRefundsRepository>()
.AddScoped<IOrdersShippingLinesDiscountAllocationsRepository, OrdersShippingLinesDiscountAllocationsRepository>()
.AddScoped<IOrdersShippingLinesRepository, OrdersShippingLinesRepository>()
.AddScoped<IOrdersShippingLinesTaxLinesRepository, OrdersShippingLinesTaxLinesRepository>()
.AddScoped<IOrdersTaxLinesRepository, OrdersTaxLinesRepository>()
.AddScoped<IProductsVariantsRepository, ProductsVariantsRepository>()
.AddScoped<IRefundsOrderAdjustmentsRepository, RefundsOrderAdjustmentsRepository>()
.AddScoped<IOrdersLineItemsDiscountAllocationsRepository, OrdersLineItemsDiscountAllocationsRepository>()
.AddScoped<IQueueMessageProcessor, QueueMessageProcessor>()
.AddScoped<ILogRepository, LogRepository>()
.AddScoped<IProductRepository, ProductRepository>()
.AddScoped<IProductsTrackingRepository, ProductsTrackingRepository>()
.AddScoped<IProductsTrackingHistoryRepository, ProductsTrackingHistoryRepository>()
.AddScoped<ITimeZoneHelper, TimeZoneHelper>()
.BuildServiceProvider();
#endregion

var service = serviceProvider.GetService<IReceiver>();
if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
{
    Console.WriteLine("Application already running application");
    Console.WriteLine("Closing App....");
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
    Environment.Exit(-1);
    return;
}
HostFactory.Run(h =>
{
    h.Service<IReceiver>(c =>
    {
        c.ConstructUsing(() => service);
        c.WhenStarted(pump => pump.Listen(serviceProvider));
        c.WhenStopped(pump => pump.Stop());
    });
    h.SetServiceName("QueueProcessor");
    h.SetDisplayName("QueueProcessor Service");

    h.EnableServiceRecovery(c =>
    {
        c.RestartService(1);
    });

    h.StartAutomatically();
});
