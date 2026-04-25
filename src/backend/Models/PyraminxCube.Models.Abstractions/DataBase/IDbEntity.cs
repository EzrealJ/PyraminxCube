using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions.DataBase
{
    public interface IDbEntity : IEntity
    {
    }
    public interface IDbEntity<TKey> : IDbEntity, IEntity<TKey>
         where TKey : notnull
    {
    }
}
