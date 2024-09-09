using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions.DataBase
{
    public class DBIdentityTableEntity<TIdentity> : DBTableEntity<TIdentity>, IIdentityEntity<TIdentity>
        where TIdentity : notnull
    {
        public DBIdentityTableEntity()
        {
            Id = default!;
        }

        /// <summary>
        /// Id
        /// </summary>
        public TIdentity Id { get; set; }
    }
}
