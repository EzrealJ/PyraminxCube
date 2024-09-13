using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions.DataBase
{
    public class DbIdentityTableEntity<TIdentity> : DbTableEntity<TIdentity>, IIdentityEntity<TIdentity>
        where TIdentity : notnull
    {
        public DbIdentityTableEntity()
        {
            Id = default!;
        }

        /// <summary>
        /// Id
        /// </summary>
        public TIdentity Id { get; set; }
    }
}
