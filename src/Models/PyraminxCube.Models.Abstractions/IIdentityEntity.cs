using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions
{
    public interface IIdentityEntity<TIdentity>:IEntity<TIdentity>
         where TIdentity : notnull
    {
        public TIdentity Id { get; }
    }
}
