using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PyraminxCube.Repositories.DbContext.EFCore
{
    public abstract class TenantEFCoreDbContextContract<TTentantKey> : EFCoreDbContextContract, ITenantDbContext<TTentantKey>
    {
        protected TenantEFCoreDbContextContract(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public abstract TTentantKey TenantId { get; }
    }
}
