using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EntityFrameworkCoreDbContext = Microsoft.EntityFrameworkCore.DbContext;
namespace PyraminxCube.Repositories.DbContext.EFCore;

public abstract class EFCoreDbContextContract : EntityFrameworkCoreDbContext, IDbContext
{
    public EFCoreDbContextContract(ILoggerFactory loggerFactory) => LoggerFactory = loggerFactory;

    public abstract string ConnectionString { get; }
    public ILoggerFactory LoggerFactory { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(LoggerFactory);
        base.OnConfiguring(optionsBuilder);
    }
}
