using Microsoft.EntityFrameworkCore;


namespace DAL.Models
{
    public partial class MarketContext : DbContext
    {
        public MarketContext()
        {
        }

        public MarketContext(DbContextOptions<MarketContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<OrderHistory> Histories { get; set; }
     



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseNpgsql("Host=ec2-52-19-164-214.eu-west-1.compute.amazonaws.com;Port=5432;Database=dd4r6kpbqg47jk;Username=gfdbznyfxbcdes;Password=5a0f3f35273184ecd8fd51874c82db8e5cd3435074e26cdcd9c0cc129edc78e3;SslMode=Require;Trust Server Certificate=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderProduct>(p 
                => p.HasKey(c=>new{c.OrderId,c.Product}));
        }
    }
}
