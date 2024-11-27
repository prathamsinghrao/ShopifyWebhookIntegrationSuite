using Database.EFDbContext;
using Database.Repository.DbEntityRepo;
using Database.Repository.EFRepo;
using Microsoft.EntityFrameworkCore;
using ServiceBusQueue;
using ShopifyWebhookManager.CacheHelper;

var webApplicationOptions = new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args,
};
var builder = WebApplication.CreateBuilder(webApplicationOptions);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnectionString"), sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });
});
builder.Services.AddSingleton<IMemoryCacheClient, MemoryCacheClient>();
builder.Services.AddScoped<IShopifyOrderWebhookLogRepository, ShopifyOrderWebhookLogRepository>();
builder.Services.AddScoped<IServiceBusQueueClient, ServiceBusQueueClient>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.Configure<ServiceBusOptions>(option =>
{
    option.QueueName = builder.Configuration["ServiceBusQueueName"] ?? "";
    //option.QueueName = "theonetestingqueue";
    option.ServiceBusConnectionString = builder.Configuration["ServiceBusConnectionString"] ?? "";
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
