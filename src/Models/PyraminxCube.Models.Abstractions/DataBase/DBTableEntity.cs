using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions.DataBase
{
    public abstract class DBTableEntity : DbEntity, IDBTableEntity
    {

    }
    public abstract class DBTableEntity<TKey> : DbEntity<TKey>, IDbEntity<TKey>
       where TKey : notnull
    {

    }
}
