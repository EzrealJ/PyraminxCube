using System.Data;
using Microsoft.Extensions.Logging;

namespace PyraminxCube.Repositories.DbContext;

public interface IDbContext
{
    /// <summary>
    /// 数据库连接配置
    /// </summary>
    string ConnectionString { get; }
    
    /// <summary>
    /// 日志工厂
    /// </summary>
    ILoggerFactory LoggerFactory { get; }
}
