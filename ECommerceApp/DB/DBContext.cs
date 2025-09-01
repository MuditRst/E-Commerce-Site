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

        modelBuilder.Entity<LoginDatabase>().ToContainer("Users")
            .HasPartitionKey(u => u.Username)
            .HasKey(u => new { u.ID, u.Username });

        modelBuilder.Entity<KafkaLog>().ToContainer("KafkaLogs")
            .HasPartitionKey(k => k.Topic)
            .HasNoDiscriminator()
            .HasKey(k => new { k.ID, k.Topic });

        modelBuilder.Entity<OrderStatusHistory>().ToContainer("OrderStatusHistories")
            .HasPartitionKey(h => h.UserId)
            .HasNoDiscriminator()
            .HasKey(o => new{o.ID,o.UserId});

        base.OnModelCreating(modelBuilder);
    }
}
