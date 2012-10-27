using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeZones
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITimeZoneService
    {
        /// <summary>
        ///     All available time zones
        /// </summary>
        IReadOnlyList<string> SystemTimeZoneIds { get; }

        /// <summary>
        /// Gets a TimeZoneEx by id.
        /// </summary>
        /// <param name="id">Invariant Time Zone name. See TimeZones property for full list.</param>
        /// <returns></returns>
        ITimeZoneEx FindSystemTimeZoneById(string id);

        /// <summary>
        /// Converts a DateTimeOffset to one in the specified system time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="destinationTimeZoneId"></param>
        /// <returns></returns>
        DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId);
    }

    /// <summary>
    /// Represents a time zone
    /// </summary>
    public interface ITimeZoneEx
    {
        /// <summary>
        /// Determines if the current datetime value is in daylight time or not
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset);

        /// <summary>
        ///  Gets a DateTimeOffset for this time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset);

        /// <summary>
        /// Normal offset from UTC
        /// </summary>
        TimeSpan BaseUtcOffset { get; }
        
        /// <summary>
        /// Localized name for standard time
        /// </summary>
        string StandardName
        {
            get;
        }
        
        /// <summary>
        /// Localized name for daylight time
        /// </summary>
        string DaylightName { get; }

        /// <summary>
        /// System Id for this time zone
        /// </summary>
        string Id { get; }
    }
}
