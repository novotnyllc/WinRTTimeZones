using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeZones
{
    /// <summary>
    /// Exception thrown from Time Zone
    /// </summary>
    internal sealed class TimeZoneInfoExException : Exception
    {
        internal TimeZoneInfoExException(int code, string message) : base(message)
        {
            if (code >= 0)
                throw new ArgumentOutOfRangeException("code");

            HResult = code;
        }
    }
}
