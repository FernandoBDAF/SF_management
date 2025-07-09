using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SFManagement.Models;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Entities;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using SFManagement.Services;

namespace SFManagement.Data;

// Design-time factory for EF tools
public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        
        // Read configuration from appsettings.json for design-time
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseSqlServer(connectionString);
        
        // Create minimal services for design-time
        var httpContextAccessor = new HttpContextAccessor();
        var loggingService = new DesignTimeLoggingService();
        
        return new DataContext(optionsBuilder.Options, httpContextAccessor, loggingService);
    }
}

// Minimal logging service for design-time
public class DesignTimeLoggingService : ILoggingService
{
    public void LogUserAction(string action, string resource, object? data = null, LogLevel level = LogLevel.Information) { }
    public void LogSecurityEvent(string eventType, string details, LogLevel level = LogLevel.Warning) { }
    public void LogDataAccess(string operation, string entityType, Guid? entityId = null, object? changes = null) { }
    public void LogFinancialOperation(string operation, decimal amount, string currency, Guid? clientId = null) { }
    public void LogAuthenticationEvent(string eventType, string userId, bool success, string? reason = null) { }
    public void LogAuthorizationEvent(string resource, string action, bool granted, string? reason = null) { }
}

// Minimal HttpContextAccessor for design-time
public class HttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}

public class DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService) : DbContext(options)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILoggingService _loggingService = loggingService;

    public DbSet<BaseAssetHolder> BaseAssetHolders { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<PokerManager> PokerManagers { get; set; }
    
    public DbSet<Address> Addresses { get; set; }
    public DbSet<InitialBalance> InitialBalances { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<AssetWallet> AssetWallets { get; set; }
    public DbSet<WalletIdentifier> WalletIdentifiers { get; set; }
    
    public DbSet<FiatAssetTransaction> FiatAssetTransactions { get; set; }
    public DbSet<DigitalAssetTransaction> DigitalAssetTransactions { get; set; }
    public DbSet<SettlementTransaction> SettlementTransactions { get; set; }
    
    public DbSet<Ofx> Ofxs { get; set; }
    public DbSet<OfxTransaction> OfxTransactions { get; set; }
    public DbSet<Excel> Excels { get; set; }
    public DbSet<ExcelTransaction> ExcelTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the one-to-one relationships between BaseAssetHolder and specific entities
        modelBuilder.Entity<Client>()
            .HasOne(c => c.BaseAssetHolder)
            .WithOne(bah => bah.Client)
            .HasForeignKey<Client>(c => c.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bank>()
            .HasOne(b => b.BaseAssetHolder)
            .WithOne(bah => bah.Bank)
            .HasForeignKey<Bank>(b => b.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Member>()
            .HasOne(m => m.BaseAssetHolder)
            .WithOne(bah => bah.Member)
            .HasForeignKey<Member>(m => m.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PokerManager>()
            .HasOne(pm => pm.BaseAssetHolder)
            .WithOne(bah => bah.PokerManager)
            .HasForeignKey<PokerManager>(pm => pm.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure WalletIdentifier relationships
        modelBuilder.Entity<WalletIdentifier>()
            .HasOne(wi => wi.AssetWallet)
            .WithMany(aw => aw.WalletIdentifiers)
            .HasForeignKey(wi => wi.AssetWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AssetWallet relationships
        modelBuilder.Entity<AssetWallet>()
            .HasOne(aw => aw.BaseAssetHolder)
            .WithMany(bah => bah.AssetWallets)
            .HasForeignKey(aw => aw.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure FiatAssetTransaction sender/receiver relationships
        modelBuilder.Entity<FiatAssetTransaction>()
            .HasOne(ft => ft.SenderWalletIdentifier)
            .WithMany()
            .HasForeignKey(ft => ft.SenderWalletIdentifierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FiatAssetTransaction>()
            .HasOne(ft => ft.ReceiverWalletIdentifier)
            .WithMany()
            .HasForeignKey(ft => ft.ReceiverWalletIdentifierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure DigitalAssetTransaction sender/receiver relationships
        modelBuilder.Entity<DigitalAssetTransaction>()
            .HasOne(dt => dt.SenderWalletIdentifier)
            .WithMany()
            .HasForeignKey(dt => dt.SenderWalletIdentifierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DigitalAssetTransaction>()
            .HasOne(dt => dt.ReceiverWalletIdentifier)
            .WithMany()
            .HasForeignKey(dt => dt.ReceiverWalletIdentifierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure SettlementTransaction sender/receiver relationships
        modelBuilder.Entity<SettlementTransaction>()
            .HasOne(st => st.SenderWalletIdentifier)
            .WithMany()
            .HasForeignKey(st => st.SenderWalletIdentifierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SettlementTransaction>()
            .HasOne(st => st.ReceiverWalletIdentifier)
            .WithMany()
            .HasForeignKey(st => st.ReceiverWalletIdentifierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure other transaction relationships
        modelBuilder.Entity<FiatAssetTransaction>()
            .HasOne(ft => ft.OfxTransaction)
            .WithMany()
            .HasForeignKey(ft => ft.OfxTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DigitalAssetTransaction>()
            .HasOne(dt => dt.Excel)
            .WithMany()
            .HasForeignKey(dt => dt.ExcelId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure transaction indexes for performance on concrete tables
        
        // FiatAssetTransaction indexes
        modelBuilder.Entity<FiatAssetTransaction>()
            .HasIndex(ft => ft.Date)
            .HasDatabaseName("IX_FiatAssetTransaction_Date");

        modelBuilder.Entity<FiatAssetTransaction>()
            .HasIndex(ft => new { ft.SenderWalletIdentifierId, ft.Date })
            .HasDatabaseName("IX_FiatAssetTransaction_Sender_Date");

        modelBuilder.Entity<FiatAssetTransaction>()
            .HasIndex(ft => new { ft.ReceiverWalletIdentifierId, ft.Date })
            .HasDatabaseName("IX_FiatAssetTransaction_Receiver_Date");

        modelBuilder.Entity<FiatAssetTransaction>()
            .HasIndex(ft => ft.DeletedAt)
            .HasDatabaseName("IX_FiatAssetTransaction_DeletedAt");

        // DigitalAssetTransaction indexes
        modelBuilder.Entity<DigitalAssetTransaction>()
            .HasIndex(dt => dt.Date)
            .HasDatabaseName("IX_DigitalAssetTransaction_Date");

        modelBuilder.Entity<DigitalAssetTransaction>()
            .HasIndex(dt => new { dt.SenderWalletIdentifierId, dt.Date })
            .HasDatabaseName("IX_DigitalAssetTransaction_Sender_Date");

        modelBuilder.Entity<DigitalAssetTransaction>()
            .HasIndex(dt => new { dt.ReceiverWalletIdentifierId, dt.Date })
            .HasDatabaseName("IX_DigitalAssetTransaction_Receiver_Date");

        modelBuilder.Entity<DigitalAssetTransaction>()
            .HasIndex(dt => dt.DeletedAt)
            .HasDatabaseName("IX_DigitalAssetTransaction_DeletedAt");

        // SettlementTransaction indexes
        modelBuilder.Entity<SettlementTransaction>()
            .HasIndex(st => st.Date)
            .HasDatabaseName("IX_SettlementTransaction_Date");

        modelBuilder.Entity<SettlementTransaction>()
            .HasIndex(st => new { st.SenderWalletIdentifierId, st.Date })
            .HasDatabaseName("IX_SettlementTransaction_Sender_Date");

        modelBuilder.Entity<SettlementTransaction>()
            .HasIndex(st => new { st.ReceiverWalletIdentifierId, st.Date })
            .HasDatabaseName("IX_SettlementTransaction_Receiver_Date");

        modelBuilder.Entity<SettlementTransaction>()
            .HasIndex(st => st.DeletedAt)
            .HasDatabaseName("IX_SettlementTransaction_DeletedAt");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetDefaultProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetDefaultProperties()
    {
        var userId = GetCurrentUserId();
        
        foreach (var auditableEntity in ChangeTracker.Entries<BaseDomain>())
        {
            if (auditableEntity.State == EntityState.Added)
            {
                auditableEntity.Entity.CreatedAt = DateTime.UtcNow;
                auditableEntity.Entity.LastModifiedBy = userId;
                
                // Log creation event
                _loggingService.LogDataAccess("create", auditableEntity.Entity.GetType().Name, 
                    auditableEntity.Entity.Id, new { 
                        EntityType = auditableEntity.Entity.GetType().Name,
                        EntityId = auditableEntity.Entity.Id,
                        CreatedBy = userId,
                        CreatedAt = auditableEntity.Entity.CreatedAt
                    });
            }
            else if (auditableEntity.State == EntityState.Modified)
            {
                if (auditableEntity.Entity.DeletedAt.HasValue)
                {
                    // Soft delete operation
                    auditableEntity.Property(p => p.UpdatedAt).IsModified = false;
                    
                    if (auditableEntity.Entity.DeletedAt == DateTime.MinValue)
                    {
                        auditableEntity.Entity.DeletedAt = DateTime.UtcNow;
                        auditableEntity.Entity.LastModifiedBy = userId;
                        
                        // Log deletion event
                        _loggingService.LogDataAccess("delete", auditableEntity.Entity.GetType().Name, 
                            auditableEntity.Entity.Id, new { 
                                EntityType = auditableEntity.Entity.GetType().Name,
                                EntityId = auditableEntity.Entity.Id,
                                DeletedBy = userId,
                                DeletedAt = auditableEntity.Entity.DeletedAt
                            });
                    }
                }
                else
                {
                    // Regular update operation
                    auditableEntity.Entity.UpdatedAt = DateTime.UtcNow;
                    auditableEntity.Entity.LastModifiedBy = userId;
                    
                    // Log update event
                    _loggingService.LogDataAccess("update", auditableEntity.Entity.GetType().Name, 
                        auditableEntity.Entity.Id, new { 
                            EntityType = auditableEntity.Entity.GetType().Name,
                            EntityId = auditableEntity.Entity.Id,
                            UpdatedBy = userId,
                            UpdatedAt = auditableEntity.Entity.UpdatedAt
                        });
                }
            }
        }
    }

    private Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user != null && user.Identity?.IsAuthenticated == true)
        {
            var subClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(subClaim))
            {
                // Generate consistent Guid from Auth0 sub claim
                var hash = System.Security.Cryptography.SHA256.Create();
                var hashBytes = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(subClaim));
                return new Guid(hashBytes.Take(16).ToArray());
            }
        }
        
        // Return a default system user ID if no authenticated user
        return Guid.Parse("00000000-0000-0000-0000-000000000000");
    }
}