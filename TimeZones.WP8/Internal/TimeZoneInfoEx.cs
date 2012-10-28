using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TimeZones_WP8_Native;

namespace TimeZones.Internal
{
    /// <summary>
    ///     Class to perform time zone conversions
    /// </summary>
    internal sealed class TimeZoneInfoEx
    {
        private static readonly Lazy<List<string>> _timeZones;
        private static readonly Lazy<IDictionary<string, TimeZoneInfoEx>> _timeZoneData;
        private DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED _source;

        static TimeZoneInfoEx()
        {
            _timeZoneData = new Lazy<IDictionary<string, TimeZoneInfoEx>>(FillData);
            _timeZones = new Lazy<List<string>>(() => new List<string>(_timeZoneData.Value.Keys));
        }

        private TimeZoneInfoEx(DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED source)
        {
            _source = source;
            Name = source.TimeZoneKeyName;
            StandardName = source.StandardName;
            DaylightName = source.DaylightName;

            BaseUtcOffset = new TimeSpan(0, -(source.Bias + source.StandardBias), 0);
        }

        /// <summary>
        ///     All available time zones
        /// </summary>
        public static IReadOnlyList<string> SystemTimeZoneIds
        {
            get { return _timeZones.Value; }
        }

        /// <summary>
        ///     Invariant name such as "Eastern Standard Time"
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Localized name for the standard time
        /// </summary>
        public string StandardName { get; private set; }

        /// <summary>
        ///     Localized name for the daylight time
        /// </summary>
        public string DaylightName { get; private set; }

        public TimeSpan BaseUtcOffset { get; private set; }
        

        /// <summary>
        ///     Gets a TimeZoneInfo by id.
        /// </summary>
        /// <param name="id">Invariant Time Zone name. See TimeZones property for full list.</param>
        /// <returns></returns>
        public static TimeZoneInfoEx FindSystemTimeZoneById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");

            TimeZoneInfoEx tz;
            if (_timeZoneData.Value.TryGetValue(id, out tz))
            {
                return tz;
            }

            throw new TimeZoneInfoExException(-1, "Time Zone is not in the list of TimeZones");
        }

        private bool Equals(TimeZoneInfoEx other)
        {
            return string.Equals(Name, other.Name);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        /// <param name="obj"> The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is TimeZoneInfoEx && Equals((TimeZoneInfoEx)obj);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" /> .
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        ///     Gets a DateTimeOffset for this time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = FromDateTime(dateTimeOffset.UtcDateTime);

            //TIME_ZONE_INFORMATION_WRAPPED tzi;
            var tzi = TimeZoneInfoMethods.GetTimeZoneInformationForYearWrapped(utcDateTime.Year, _source);
            
                //SYSTEMTIME_WRAPPED destDateTime;
            var destDateTime = TimeZoneInfoMethods.SystemTimeToTzSpecificLocalTimeWrapped(tzi, utcDateTime);
            
                
                    var dt = FromSystemTime(destDateTime);
                    var daylight = IsDaylightTime(dt, ref tzi);

                    var bias = tzi.Bias + tzi.StandardBias;

                    if (daylight)
                        bias += tzi.DaylightBias;

                    var ts = new TimeSpan(0, -bias, 0);

                    var dto = new DateTimeOffset(dt, ts);
                    return dto;
                
            

            //var error = Marshal.GetLastWin32Error();
            //Marshal.ThrowExceptionForHR(error);
            //throw new TimeZoneInfoExException(error, "Win32 error occured");
        }

        private static SYSTEMTIME_WRAPPED FromDateTime(DateTime dateTime)
        {
            var st = new SYSTEMTIME_WRAPPED
                {
                    Year = (short)dateTime.Year,
                    Month = (short)dateTime.Month,
                    DayOfWeek = (short)dateTime.DayOfWeek,
                    Day = (short)dateTime.Day,
                    Hour = (short)dateTime.Hour,
                    Minute = (short)dateTime.Minute,
                    Second = (short)dateTime.Second,
                    Milliseconds = (short)dateTime.Millisecond
                };

            return st;
        }

        /// <summary>
        ///     Determines if the current datetime value is in daylight time or not
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = FromDateTime(dateTimeOffset.UtcDateTime);

            //TIME_ZONE_INFORMATION_WRAPPED tzi;
            var tzi = TimeZoneInfoMethods.GetTimeZoneInformationForYearWrapped(utcDateTime.Year, _source);
            
                //SYSTEMTIME_WRAPPED destDateTime;
            var destDateTime = TimeZoneInfoMethods.SystemTimeToTzSpecificLocalTimeWrapped(tzi, utcDateTime);
              
                    var dt = FromSystemTime(destDateTime);
                    var daylight = IsDaylightTime(dt, ref tzi);

                    return daylight;
               
            

            var error = Marshal.GetLastWin32Error();
            Marshal.ThrowExceptionForHR(error);
            throw new TimeZoneInfoExException(error, "Win32 error occured");
        }

        private static DateTime FromSystemTime(SYSTEMTIME_WRAPPED systemTime, short? yearOverride = null)
        {
            return new DateTime(yearOverride ?? systemTime.Year,
                                systemTime.Month,
                                systemTime.Day,
                                systemTime.Hour,
                                systemTime.Minute,
                                systemTime.Second,
                                systemTime.Milliseconds,
                                DateTimeKind.Unspecified);
        }

        private static bool IsDaylightTime(DateTime date, ref TIME_ZONE_INFORMATION_WRAPPED tzi)
        {
            if (tzi.StandardDate.Month == 0 || tzi.DaylightDate.Month == 0) // Not daylight time in the TZ
                return false;

            // Reuse the DateTime greater/less-than operators and rules
            var stdDate = FromSystemTime(tzi.StandardDate, (short)date.Year);
            var dltDate = FromSystemTime(tzi.DaylightDate, (short)date.Year);

            if (date >= dltDate && date < stdDate)
                return true;

            return false;
        }


        /// <summary>
        ///     Converts a DateTimeOffset to one in the specified system time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="destinationTimeZoneId"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
        {
            var tz = FindSystemTimeZoneById(destinationTimeZoneId);
            return tz.ConvertTime(dateTimeOffset);
        }

        private static IDictionary<string, TimeZoneInfoEx> FillData()
        {
            return EnumerateSystemTimeZones().Select(tz => new TimeZoneInfoEx(tz)).ToDictionary(tz => tz.Name);
        }


        internal static IEnumerable<DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED> EnumerateSystemTimeZones()
        {
            var i = 0;
            while (true)
            {
                DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED tz;
                var result = TimeZoneInfoMethods.EnumDynamicTimeZoneInformationWrapped(i++);

                if (result.count != 0)
                    yield break;

                yield return result.TZ;
            }
        }
        
    }
}