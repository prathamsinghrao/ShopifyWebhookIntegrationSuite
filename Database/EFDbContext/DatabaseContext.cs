using Database.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace Database.EFDbContext
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public virtual DbSet<Customers> Customer { get; set; }
        public virtual DbSet<OrderDiscountCodes> OrderDiscountCodes { get; set; }
        public virtual DbSet<OrderLineItems> OrderLineItems { get; set; }
        public virtual DbSet<OrderLineItemsTaxLines> OrderLineItemsTaxLines { get; set; }
        public virtual DbSet<OrderRefundLineItems> OrderRefundLineItems { get; set; }
        public virtual DbSet<OrderRefunds> OrderRefunds { get; set; }
        public virtual DbSet<Orders> Order { get; set; }
        public virtual DbSet<OrdersShippingLines> OrdersShippingLines { get; set; }
        public virtual DbSet<OrdersShippingLinesDiscountAllocations> OrdersShippingLinesDiscountAllocations { get; set; }
        public virtual DbSet<OrdersLineItemsDiscountAllocations> OrdersLineItemsDiscountAllocations { get; set; }
        public virtual DbSet<OrdersShippingLinesTaxLines> OrdersShippingLinesTaxLines { get; set; }
        public virtual DbSet<OrdersTaxLines> OrdersTaxLines { get; set; }
        public virtual DbSet<ProductsVariants> ProductsVariants { get; set; }
        public virtual DbSet<RefundsOrderAdjustments> RefundsOrderAdjustments { get; set; }
        public virtual DbSet<Products> Product { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<TbShopifyOrderWebhookLog> Logs { get; set; }
        public virtual DbSet<ProductsTracking> ProductsTracking { get; set; }
        public virtual DbSet<ProductsTrackingHistory> ProductsTrackingHistory { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customers>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.CustomerId);
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.OrderId);
            });

            modelBuilder.Entity<OrderLineItems>(entity =>
            {
                entity.ToTable("OrderLineItems");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrdersTaxLines>(entity =>
            {
                entity.ToTable("OrdersTaxLines");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrderLineItemsTaxLines>(entity =>
            {
                entity.ToTable("OrderLineItemsTaxLines");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrderDiscountCodes>(entity =>
            {
                entity.ToTable("OrderDiscountCodes");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrderRefunds>(entity =>
            {
                entity.ToTable("OrderRefunds");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrderRefundLineItems>(entity =>
            {
                entity.ToTable("OrderRefundLineItems");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<RefundsOrderAdjustments>(entity =>
            {
                entity.ToTable("RefundsOrderAdjustments");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrdersShippingLines>(entity =>
            {
                entity.ToTable("OrdersShippingLines");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrdersShippingLinesTaxLines>(entity =>
            {
                entity.ToTable("OrdersShippingLinesTaxLines");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrdersShippingLinesDiscountAllocations>(entity =>
            {
                entity.ToTable("OrdersShippingLinesDiscountAllocations");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<OrdersLineItemsDiscountAllocations>(entity =>
            {
                entity.ToTable("OrdersLineItemsDiscountAllocations");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<ProductsVariants>(entity =>
            {
                entity.ToTable("ProductsVariants");
                entity.HasKey(e => e.Id);
            });
            
            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("tb_log");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
            });
            modelBuilder.Entity<TbShopifyOrderWebhookLog>(entity =>
            {
                entity.ToTable("tb_ShopifyOrderWebhookLog");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<ProductsTracking>(entity =>
            {
                entity.ToTable("ProductsTracking");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<ProductsTrackingHistory>(entity =>
            {
                entity.ToTable("ProductsTrackingHistory");
                entity.HasKey(e => e.Id);
            });
        }
    }
}
