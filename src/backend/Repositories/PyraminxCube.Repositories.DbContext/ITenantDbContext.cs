namespace PyraminxCube.Repositories.DbContext;

public interface ITenantDbContext<TTenantKey>
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public TTenantKey TenantId { get; }
}
