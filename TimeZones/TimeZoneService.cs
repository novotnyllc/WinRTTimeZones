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
        private static readonly ITimeZoneServiceAdapter _service = PlatformAdapter.Resolve<ITimeZoneServiceAdapter>(true);

        private static readonly Lazy<ITimeZoneEx> _utcTimeZone = new Lazy<ITimeZoneEx>(() => _service.FindSystemTimeZoneById("UTC"));

        /// <summary>
        /// UTC Time Zone
        /// </summary>
        public static ITimeZoneEx Utc
        {
            get { return _utcTimeZone.Value; }
        }

        /// <summary>
        /// Local Time Zone
        /// </summary>
        public static ITimeZoneEx Local
        {
            get
            {
                return _service.Local;
            }
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
            if (string.IsNullOrWhiteSpace(destinationTimeZoneId))
                throw new ArgumentNullException();

            var tz = _service.FindSystemTimeZoneById(destinationTimeZoneId);
            return tz.ConvertTime(dateTimeOffset);
        }


        /// <summary>
        /// Sets the specified timezone on the date without converting the time
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public static DateTimeOffset SpecifyTimeZone(DateTimeOffset dateTimeOffset, ITimeZoneEx timeZone)
        {
            if (timeZone == null)
                throw new ArgumentNullException("timeZone");

            // Treat the date as UTC
            var dateAsUtc = DateTime.SpecifyKind(dateTimeOffset.DateTime, DateTimeKind.Utc);

            // This is to find the correct offset
            var asLocal = timeZone.ConvertTime(dateAsUtc);

            return new DateTimeOffset(DateTime.SpecifyKind(dateAsUtc, DateTimeKind.Unspecified), asLocal.Offset);
        }
       
    }
}
