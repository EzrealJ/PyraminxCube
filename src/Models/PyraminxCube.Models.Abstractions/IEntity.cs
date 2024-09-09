using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions
{
    public interface IEntity
    {
        string GetDisplayString();
    }
    public interface IEntity<TKey> : IEntity
        where TKey : notnull
    {
        TKey GetKey(Func<TKey>? defaultKeyGetter = null);
    }

}
