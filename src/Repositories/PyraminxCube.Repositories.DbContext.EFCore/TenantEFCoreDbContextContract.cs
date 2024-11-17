using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PyraminxCube.Repositories.DbContext.EFCore
{
    public abstract class TenantEFCoreDbContextContract<TTentantKey>(ILoggerFactory loggerFactory)
        : EFCoreDbContextContract(loggerFactory), ITenantDbContext<TTentantKey>
    {
        public abstract TTentantKey TenantId { get; }
    }
}
