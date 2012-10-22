using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZones.Internal;

namespace TimeZones
{
    /// <summary>
    /// 
    /// </summary>
    public static class TimeZoneService
    {
        private static ITimeZoneService _service;

        /// <summary>
        /// 
        /// </summary>
        static TimeZoneService()
        {
            _service = PlatformAdapter.Resolve<ITimeZoneService>(true);
        }

        /// <summary>
        ///     All available time zones
        /// </summary>
        public static IReadOnlyList<string> SystemTimeZoneIds
        {
            get { return _service.SystemTimeZoneIds; }
        }

        /// <summary>
        /// Gets a TimeZoneEx by id.
        /// </summary>
        /// <param name="id">Invariant Time Zone name. See TimeZones property for full list.</param>
        /// <returns></returns>
        public static ITimeZoneEx FindSystemTimeZoneById(string id)
        {
            return _service.FindSystemTimeZoneById(id);
        }

        /// <summary>
        /// Converts a DateTimeOffset to one in the specified system time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="destinationTimeZoneId"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
        {
            return _service.ConvertTimeBySystemTimeZoneId(dateTimeOffset, destinationTimeZoneId);
        }
    }
}
