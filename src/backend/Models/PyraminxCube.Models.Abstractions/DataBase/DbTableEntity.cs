using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions.DataBase
{
    public abstract class DbTableEntity : DbEntity, IDbTableEntity
    {

    }
    public abstract class DbTableEntity<TKey> : DbEntity<TKey>, IDbEntity<TKey>
       where TKey : notnull
    {

    }
}
