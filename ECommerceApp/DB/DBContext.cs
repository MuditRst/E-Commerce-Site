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
                .HasNoDiscriminator(); 

            modelBuilder.Entity<LoginDatabase>().ToContainer("Users")
                .HasPartitionKey(u => u.Username)
                .HasNoDiscriminator();

            modelBuilder.Entity<KafkaLog>().ToContainer("KafkaLogs")
                .HasPartitionKey(k => k.Topic) 
                .HasNoDiscriminator();
           modelBuilder.Entity<OrderStatusHistory>().ToContainer("OrderStatusHistories")
                .HasPartitionKey(h => h.OrderID)
                .HasNoDiscriminator();

        base.OnModelCreating(modelBuilder);
    }
}
