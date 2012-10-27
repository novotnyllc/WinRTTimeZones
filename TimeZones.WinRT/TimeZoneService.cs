using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZones.Internal;
using ITimeZonePcl = TimeZones.ITimeZoneEx;

namespace TimeZones.WinRT
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TimeZoneService
    {
        private static readonly ITimeZoneService _service = PlatformAdapter.Resolve<ITimeZoneService>(true);

        /// <summary>
        ///     All available time zones
        /// </summary>
        public static IReadOnlyList<string> SystemTimeZoneIds
        {
            get { return _service.SystemTimeZoneIds; }
        }

        /// <summary>
        ///     Gets a TimeZoneEx by id.
        /// </summary>
        /// <param name="id">Invariant Time Zone name. See TimeZones property for full list.</param>
        /// <returns></returns>
        public static ITimeZoneEx FindSystemTimeZoneById(string id)
        {
            return new TimeZoneEx(_service.FindSystemTimeZoneById(id));
        }

        /// <summary>
        ///     Converts a DateTimeOffset to one in the specified system time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="destinationTimeZoneId"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
        {
            return _service.ConvertTimeBySystemTimeZoneId(dateTimeOffset, destinationTimeZoneId);
        }

        private class TimeZoneEx : ITimeZoneEx
        {
            private readonly ITimeZonePcl _wrapped;

            public TimeZoneEx(ITimeZonePcl wrapped)
            {
                _wrapped = wrapped;
            }

            public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
            {
                return _wrapped.IsDaylightSavingTime(dateTimeOffset);
            }

            public DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset)
            {
                return _wrapped.ConvertTime(dateTimeOffset);
            }

            public TimeSpan BaseUtcOffset
            {
                get { return _wrapped.BaseUtcOffset; }
            }


            public string StandardName
            {
                get { return _wrapped.StandardName; }
            }

            public string DaylightName
            {
                get { return _wrapped.DaylightName; }
            }

            public string Id
            {
                get { return _wrapped.Id; }
            }
        }
    }

    /// <summary>
    ///     Represents a time zone
    /// </summary>
    public interface ITimeZoneEx
    {
        /// <summary>
        ///     Normal offset from UTC
        /// </summary>
        TimeSpan BaseUtcOffset { get; }

        /// <summary>
        ///     Localized name for standard time
        /// </summary>
        string StandardName { get; }
        
        /// <summary>
        ///     Localized name for daylight time
        /// </summary>
        string DaylightName { get; }

        /// <summary>
        ///     System Id for this time zone
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Determines if the current datetime value is in daylight time or not
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset);

        /// <summary>
        ///     Gets a DateTimeOffset for this time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset);
    }
}