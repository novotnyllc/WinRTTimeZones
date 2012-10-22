using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeZones.Internal
{
    internal interface IAdapterResolver
    {
        object Resolve(Type type);
    }
}
