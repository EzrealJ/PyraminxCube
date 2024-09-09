using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions.DataBase
{
    public abstract class DbEntity : IDbEntity
    {
        public virtual string GetDisplayString() => string.Empty;
    }
    public abstract class DbEntity<TKey> : DbEntity, IDbEntity<TKey>
        where TKey : notnull
    {
        public virtual TKey GetKey(Func<TKey>? defaultKeyGetter = null)
        {
            if (defaultKeyGetter == null && default(TKey) == null)
            {
                throw new ArgumentException("泛型TKey的default为null,且未指定默认值创建器");
            }
            if (defaultKeyGetter == null)
            {
                return defaultKeyGetter!();
            }
            else
            {
                return default!;
            }
        }
    }
}
