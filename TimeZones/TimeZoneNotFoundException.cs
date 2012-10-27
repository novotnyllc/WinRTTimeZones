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
    public sealed class TimeZoneInfoExException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public TimeZoneInfoExException(int code, string message) : base(message)
        {
            if (code >= 0)
                throw new ArgumentOutOfRangeException("code");

            HResult = code;
        }
    }
}
