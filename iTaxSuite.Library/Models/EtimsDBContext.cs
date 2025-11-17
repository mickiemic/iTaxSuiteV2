using iTaxSuite.Library.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace iTaxSuite.Library.Models
{
    public class ETimsDBContext : IdentityDbContext<ApplicationUser>
    {
        public ETimsDBContext(DbContextOptions<ETimsDBContext> options) : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity Structure Tweaks
            builder.Entity<ApplicationUser>(x =>
            {
                x.ToTable("SysUser");
                x.Property(e => e.Id).HasColumnName("UserID");
            });
            builder.Entity<IdentityRole>(x =>
            {
                x.ToTable("Role");
                x.Property(e => e.Id).HasColumnName("RoleID");
            });
            builder.Entity<IdentityUserRole<string>>(x => x.ToTable("UserRole"));
            builder.Entity<IdentityUserLogin<string>>(x => x.ToTable("UserLogin"));
            builder.Entity<IdentityUserClaim<string>>(x =>
            {
                x.ToTable("UserClaim");
                x.Property(e => e.Id).HasColumnName("UserClaimID");
            });
            builder.Entity<IdentityRoleClaim<string>>(x =>
            {
                x.ToTable("RoleClaim");
                x.Property(e => e.Id).HasColumnName("RoleClaimID");
            });
            builder.Entity<IdentityUserToken<string>>(x => x.ToTable("UserToken"));
            //

            builder.Entity<TaxClient>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.ToTable(t => t.HasCheckConstraint("CK_SingleRowOnly", "[LockColumn] = 'X'"));
                entity.HasIndex(e => e.LockColumn).IsUnique();
                entity.HasOne(e => e.ExtSystConfig).WithMany().HasForeignKey(e => e.SystemCode);
            });
            builder.Entity<ExtSystConfig>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.ToTable(t => t.HasCheckConstraint("CK_SingleRowOnly", "[LockColumn] = 'X'"));
                entity.HasIndex(e => e.LockColumn).IsUnique();
            });
            builder.Entity<SyncChannel>().Property(p => p.CreatedOn).HasDefaultValueSql("GETDATE()");
            builder.Entity<EntityAttribute>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.AttributeID).UseIdentityColumn(1001, 1);
            });
            builder.Entity<S300TaxAuthority>().Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
            builder.Entity<ClientBranch>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.ClientCode, e.BranchCode });
                entity.HasAlternateKey(e => e.BranchCode);
            });
            builder.Entity<BranchCustomer>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.CustNumber, e.BranchCode });
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
            });
            builder.Entity<BranchUser>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.UserNumber, e.BranchCode });
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
            });
            builder.Entity<BranchVendor>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.VendorNumber, e.BranchCode });
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
            });

            builder.Entity<Product>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasAlternateKey(e => e.ProductCode);
                entity.HasOne(e => e.ProductData).WithOne(e => e.Product)
                    .HasForeignKey<ProductData>(e => e.ProductCode)
                    .HasPrincipalKey<Product>(e => e.ProductCode).IsRequired(true);
            });
            builder.Entity<ProductData>().Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
            builder.Entity<StockItem>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.ProductCode, e.BranchCode });
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductCode).HasPrincipalKey(e => e.ProductCode);
                //entity.Property(e => e.CacheKey).HasField("_cacheKey").HasComputedColumnSql("[BranchCode] + ':' + [ProductCode]");
            });
            builder.Entity<StockMovement>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.DocNumber, e.BranchCode });
                entity.HasAlternateKey(e => e.MovementID);
                entity.HasIndex(e => e.MovementID).IsUnique();
                entity.Property(e => e.MovementID).UseIdentityColumn(1001, 1);
                entity.HasOne(e => e.StockMovData).WithOne(e => e.StockMovement)
                    .HasForeignKey<StockMovData>(e => e.MovementID)
                    .HasPrincipalKey<StockMovement>(e => e.MovementID).IsRequired(true);
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
            });
            builder.Entity<StockMovData>().Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

            builder.Entity<PurchTransact>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.DocNumber, e.BranchCode });
                entity.HasAlternateKey(e => e.PurchaseID);
                entity.HasIndex(e => e.PurchaseID).IsUnique();
                entity.Property(e => e.PurchaseID).UseIdentityColumn(1001, 1);
                entity.HasOne(e => e.PurchTrxData).WithOne(e => e.PurchTransact)
                    .HasForeignKey<PurchTrxData>(e => e.PurchaseID)
                    .HasPrincipalKey<PurchTransact>(e => e.PurchaseID).IsRequired(true);
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
            });
            builder.Entity<PurchTrxData>().Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

            builder.Entity<SalesTransact>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.DocNumber, e.BranchCode });
                entity.HasAlternateKey(e => e.SalesTrxID);
                entity.HasIndex(e => e.SalesTrxID).IsUnique();
                entity.Property(e => e.SalesTrxID).UseIdentityColumn(1001, 1);
                entity.HasOne(e => e.SalesTrxData).WithOne(e => e.SalesTransact)
                    .HasForeignKey<SalesTrxData>(e => e.SalesTrxID)
                    .HasPrincipalKey<SalesTransact>(e => e.SalesTrxID).IsRequired(true);
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
            });
            builder.Entity<SalesTrxData>().Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

            builder.Entity<EtimsTransact>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
                entity.HasKey(e => new { e.DocNumber, e.ReqType, e.BranchCode, e.DocStamp });
                entity.HasIndex(e => e.EtimsTrxID).IsUnique();
                entity.Property(e => e.EtimsTrxID).UseIdentityColumn(1001, 1);
                entity.HasOne(e => e.ClientBranch).WithMany().HasForeignKey(e => e.BranchCode).HasPrincipalKey(e => e.BranchCode);
            });

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
            optionsBuilder.EnableSensitiveDataLogging();
        }

        // Setup Models
        public virtual DbSet<TaxClient> TaxClient { get; set; }
        public virtual DbSet<ExtSystConfig> ExtSystConfig { get; set; }
        public virtual DbSet<SyncChannel> SyncChannels { get; set; }
        public virtual DbSet<S300TaxAuthority> S300TaxAuthority { get; set; }
        public virtual DbSet<ClientBranch> ClientBranch { get; set; }
        public virtual DbSet<BranchCustomer> BranchCustomers { get; set; }
        public virtual DbSet<BranchUser> BranchUsers { get; set; }
        public virtual DbSet<BranchVendor> BranchVendors { get; set; }

        // Inventory Models
        public virtual DbSet<StockItem> StockItems { get; set; }
        public virtual DbSet<StockMovement> StockMovement { get; set; }
        public virtual DbSet<StockMovData> StockMovData { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductData> ProductData { get; set; }

        // Purchase Models
        public virtual DbSet<PurchTransact> PurchTransact { get; set; }
        public virtual DbSet<PurchTrxData> PurchTrxData { get; set; }

        // Sales Models
        public virtual DbSet<SalesTransact> SalesTransact { get; set; }
        public virtual DbSet<SalesTrxData> SalesTrxData { get; set; }

        // Transaction Models
        public virtual DbSet<EtimsTransact> EtimsTransacts { get; set; }

        // Helper Models
        public virtual DbSet<ApiRequestLog> ApiRequestLog { get; set; }
        public virtual DbSet<EntityAttribute> EntityAttribute { get; set; }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ETimsDBContext>
    {
        ETimsDBContext IDesignTimeDbContextFactory<ETimsDBContext>.CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var builder = new DbContextOptionsBuilder<ETimsDBContext>();
            var connectionString = configuration.GetConnectionString("ITaxDBConnection");
            builder.UseSqlServer(connectionString);

            return new ETimsDBContext(builder.Options);
        }
    }

}
