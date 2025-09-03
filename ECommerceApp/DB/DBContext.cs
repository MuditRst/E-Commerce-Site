using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }

    public DbSet<Orders> Orders { get; set; }
    public DbSet<LoginDatabase> Logins { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<KafkaLog> KafkaLogs { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Orders>().ToContainer("Orders")
            .HasPartitionKey(o => o.UserId)
            .HasNoDiscriminator()
            .HasKey(o => new{o.ID,o.UserId});

        modelBuilder.Entity<Orders>()
            .Property(o => o.UserId)
            .ToJsonProperty("userId");

        modelBuilder.Entity<Orders>()
            .Property(o => o.ID)
            .ToJsonProperty("id");

        modelBuilder.Entity<LoginDatabase>().ToContainer("Users")
            .HasPartitionKey(u => u.Username)
            .HasKey(u => u.ID);

        modelBuilder.Entity<LoginDatabase>()
            .Property(u => u.Username)
            .ToJsonProperty("username");

        modelBuilder.Entity<LoginDatabase>()
            .Property(u => u.ID)
            .ToJsonProperty("id");

        modelBuilder.Entity<KafkaLog>().ToContainer("KafkaLogs")
            .HasPartitionKey(k => k.Topic)
            .HasNoDiscriminator()
            .HasKey(k => new { k.ID, k.Topic });

        modelBuilder.Entity<KafkaLog>()
            .Property(k => k.Topic)
            .ToJsonProperty("topic");

        modelBuilder.Entity<KafkaLog>()
            .Property(k => k.ID)
            .ToJsonProperty("id");

        modelBuilder.Entity<OrderStatusHistory>().ToContainer("OrderStatusHistories")
            .HasPartitionKey(h => h.UserId)
            .HasNoDiscriminator()
            .HasKey(o => new{o.ID,o.UserId});

        modelBuilder.Entity<OrderStatusHistory>()
            .Property(o => o.UserId)
            .ToJsonProperty("userId");
        
        modelBuilder.Entity<OrderStatusHistory>()
            .Property(o => o.OrderID)
            .ToJsonProperty("orderId");

        modelBuilder.Entity<OrderStatusHistory>()
            .Property(o => o.ID)
            .ToJsonProperty("id");

        base.OnModelCreating(modelBuilder);
    }
}
