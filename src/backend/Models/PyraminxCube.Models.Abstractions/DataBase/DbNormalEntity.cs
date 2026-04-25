using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions.DataBase
{
    public class DbNormalEntity<TKey, TChangeTime> : DbIdentityTableEntity<TKey>, IChangeUserEntity<TKey, TChangeTime>, IDbSoftDeletedEntity
        where TChangeTime : struct
        where TKey : notnull
    {
        public DbNormalEntity()
        {
            CreateUserId = default!;
            CreateTime = default!;
            ModifyUserId = default!;
            ModifyTime = default!;
        }

        public TKey CreateUserId { get; set; }
        public TChangeTime CreateTime { get; set; }
        public TKey ModifyUserId { get; set; }
        public TChangeTime ModifyTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
